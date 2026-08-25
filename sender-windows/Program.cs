using System.Text;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

Console.WriteLine("WiFi File Transfer - Native Windows OPP");
Console.WriteLine();

Console.WriteLine("Mencari layanan Bluetooth OPP (0x1105)...");
var services = await DeviceInformation.FindAllAsync(
    RfcommDeviceService.GetDeviceSelector(RfcommServiceId.ObexObjectPush));

var targetInfo = services.FirstOrDefault(s =>
    s.Name.Equals("ROSY-2", StringComparison.OrdinalIgnoreCase));

if (targetInfo == null)
{
    Console.WriteLine("Layanan OPP ROSY-2 tidak ditemukan.");
    Console.WriteLine("Pastikan ROSY-2 sudah paired dan Bluetooth aktif.");
    return;
}

Console.WriteLine($"Target: {targetInfo.Name}");
Console.Write("Path file (contoh C:\\Users\\ajiaj\\Desktop\\test.txt): ");
var localPath = Console.ReadLine()?.Trim().Trim('"');

if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
{
    Console.WriteLine("File tidak ditemukan.");
    return;
}

try
{
    using var service = await RfcommDeviceService.FromIdAsync(targetInfo.Id)
        ?? throw new Exception("Gagal membuka layanan OPP ROSY-2.");

    using var socket = new StreamSocket();
    Console.WriteLine("Membuka koneksi OPP native Windows...");
    await socket.ConnectAsync(
        service.ConnectionHostName,
        service.ConnectionServiceName,
        SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

    var reader = new DataReader(socket.InputStream)
    {
        InputStreamOptions = InputStreamOptions.Partial
    };
    var writer = new DataWriter(socket.OutputStream);

    Console.WriteLine("Mengirim OBEX CONNECT...");
    await Send(writer, new byte[]
    {
        0x80, 0x00, 0x07,
        0x10, 0x00,
        0xFF, 0xFE
    });

    var connectResponse = await ReadPacket(reader);
    if (connectResponse.Length < 3 || connectResponse[0] != 0xA0)
        throw new Exception($"OBEX CONNECT ditolak. Opcode: 0x{connectResponse[0]:X2}");

    var fileName = Path.GetFileName(localPath);
    var data = await File.ReadAllBytesAsync(localPath);
    var nameBytes = Encoding.BigEndianUnicode.GetBytes(fileName + "\0");
    var typeBytes = Encoding.ASCII.GetBytes("text/plain\0");

    using var packet = new MemoryStream();
    packet.WriteByte(0x82); // PUT, final
    packet.WriteByte(0);
    packet.WriteByte(0);

    WriteU16Header(packet, 0x01, nameBytes);
    WriteU8Header(packet, 0x42, typeBytes);
    WriteU32Header(packet, 0xC3, (uint)data.Length);
    WriteU16Header(packet, 0x49, data);

    var bytes = packet.ToArray();
    var packetLength = (ushort)bytes.Length;
    bytes[1] = (byte)(packetLength >> 8);
    bytes[2] = (byte)packetLength;

    Console.WriteLine($"Mengirim '{fileName}' melalui OPP native Windows...");
    Console.WriteLine("Perhatikan ROSY-2: seharusnya muncul permintaan file masuk.");
    await Send(writer, bytes);

    var putResponse = await ReadPacket(reader);
    Console.WriteLine($"Response OPP: 0x{putResponse[0]:X2}");

    await Send(writer, new byte[] { 0x81, 0x00, 0x03 });
    Console.WriteLine("Selesai.");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("Transfer gagal.");
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
}

static async Task Send(DataWriter writer, byte[] bytes)
{
    writer.WriteBytes(bytes);
    await writer.StoreAsync();
    await writer.FlushAsync();
}

static async Task<byte[]> ReadPacket(DataReader reader)
{
    await reader.LoadAsync(3);
    var header = new byte[3];
    reader.ReadBytes(header);

    var length = (header[1] << 8) | header[2];
    if (length < 3 || length > 65535)
        throw new Exception("Panjang paket OBEX tidak valid.");

    var result = new byte[length];
    Buffer.BlockCopy(header, 0, result, 0, 3);

    var remaining = (uint)(length - 3);
    if (remaining > 0)
    {
        await reader.LoadAsync(remaining);
        var tail = new byte[(int)remaining];
        reader.ReadBytes(tail);
        Buffer.BlockCopy(tail, 0, result, 3, (int)remaining);
    }

    return result;
}

static void WriteU16Header(Stream stream, byte id, byte[] value)
{
    var length = value.Length + 3;
    stream.WriteByte(id);
    stream.WriteByte((byte)(length >> 8));
    stream.WriteByte((byte)length);
    stream.Write(value);
}

static void WriteU8Header(Stream stream, byte id, byte[] value)
{
    var length = value.Length + 3;
    stream.WriteByte(id);
    stream.WriteByte((byte)(length >> 8));
    stream.WriteByte((byte)length);
    stream.Write(value);
}

static void WriteU32Header(Stream stream, byte id, uint value)
{
    stream.WriteByte(id);
    stream.WriteByte(0x00);
    stream.WriteByte(0x07);
    stream.WriteByte((byte)(value >> 24));
    stream.WriteByte((byte)(value >> 16));
    stream.WriteByte((byte)(value >> 8));
    stream.WriteByte((byte)value);
}
