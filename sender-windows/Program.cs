using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

Console.WriteLine("WiFi File Transfer - Native Windows OPP");
Console.WriteLine();

const uint OPP = 0x1105;

Console.WriteLine("Mencari perangkat ROSY-2 yang sudah paired...");
var devices = await DeviceInformation.FindAllAsync(
    BluetoothDevice.GetDeviceSelectorFromPairingState(true));

var deviceInfo = devices.FirstOrDefault(d =>
    d.Name.Equals("ROSY-2", StringComparison.OrdinalIgnoreCase));

if (deviceInfo == null)
{
    Console.WriteLine("ROSY-2 tidak ditemukan di perangkat paired.");
    return;
}

Console.WriteLine($"Target: {deviceInfo.Name}");

using var device = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
if (device == null)
{
    Console.WriteLine("Gagal membuka perangkat Bluetooth ROSY-2.");
    return;
}

Console.WriteLine("Mencari semua layanan RFCOMM ROSY-2 melalui SDP...");
var result = await device.GetRfcommServicesAsync(BluetoothCacheMode.Uncached);

var service = result.Services.FirstOrDefault(s =>
    s.ServiceId.AsShortId() == OPP);

if (service == null)
{
    Console.WriteLine("Layanan OPP (0x1105) ROSY-2 tidak ditemukan melalui SDP.");
    Console.WriteLine("Tidak melakukan transfer.");
    return;
}

Console.WriteLine($"OPP ditemukan: {service.ServiceId.AsString()}");

Console.Write("Path file (contoh C:\\Users\\ajiaj\\Desktop\\test.txt): ");
var localPath = Console.ReadLine()?.Trim().Trim('"');

if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
{
    Console.WriteLine("File tidak ditemukan.");
    return;
}

try
{
    using var socket = new StreamSocket();

    Console.WriteLine("Membuka koneksi RFCOMM OPP...");
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
    packet.WriteByte(0x82); // PUT
    packet.WriteByte(0);
    packet.WriteByte(0);

    WriteU16Header(packet, 0x01, nameBytes); // Name
    WriteU8Header(packet, 0x42, typeBytes);  // Type
    WriteU32Header(packet, 0xC3, (uint)data.Length); // Length
    WriteU16Header(packet, 0x49, data); // End Of Body

    var bytes = packet.ToArray();
    var packetLength = (ushort)bytes.Length;
    bytes[1] = (byte)(packetLength >> 8);
    bytes[2] = (byte)packetLength;

    Console.WriteLine($"Mengirim '{fileName}' melalui Bluetooth OPP...");
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
    System.Buffer.BlockCopy(header, 0, result, 0, 3);

    var remaining = (uint)(length - 3);
    if (remaining > 0)
    {
        await reader.LoadAsync(remaining);
        var tail = new byte[(int)remaining];
        reader.ReadBytes(tail);
        System.Buffer.BlockCopy(tail, 0, result, 3, (int)remaining);
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
