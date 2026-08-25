using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

Console.WriteLine("WiFi File Transfer - OPP Sender");
Console.WriteLine();

using var client = new BluetoothClient();
Console.WriteLine("Mencari perangkat Bluetooth...");

var devices = client.DiscoverDevices().ToList();
if (devices.Count == 0)
{
    Console.WriteLine("Tidak ada perangkat Bluetooth ditemukan.");
    return;
}

for (int i = 0; i < devices.Count; i++)
    Console.WriteLine($"[{i}] {devices[i].DeviceName} - {devices[i].DeviceAddress}");

var selected = devices.FirstOrDefault(d =>
    string.Equals(d.DeviceName, "ROSY-2", StringComparison.OrdinalIgnoreCase));

if (selected == null)
{
    Console.WriteLine();
    Console.Write("Pilih nomor perangkat: ");
    if (!int.TryParse(Console.ReadLine(), out var index) || index < 0 || index >= devices.Count)
    {
        Console.WriteLine("Pilihan tidak valid.");
        return;
    }
    selected = devices[index];
}

Console.WriteLine();
Console.WriteLine($"Target: {selected.DeviceName}");
Console.WriteLine($"Alamat: {selected.DeviceAddress}");
Console.WriteLine();
Console.Write("Path file (contoh C:\\Users\\ajiaj\\Desktop\\test.txt): ");
var localPath = Console.ReadLine()?.Trim().Trim('"');

if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
{
    Console.WriteLine("File tidak ditemukan.");
    return;
}

var remoteName = Path.GetFileName(localPath);
Console.WriteLine();
Console.WriteLine($"Mengirim '{remoteName}' melalui Bluetooth OPP...");
Console.WriteLine("Menunggu permintaan file di ROSY-2...");

try
{
    // Gunakan URI OBEX eksplisit agar library memilih transport OPP Bluetooth.
    var address = selected.DeviceAddress.ToString("N");
    var uri = new Uri($"obex://{address}/{Uri.EscapeDataString(remoteName)}");
    var request = new ObexWebRequest(uri)
    {
        Timeout = 15000
    };
    request.ReadFile(localPath);

    using var response = (ObexWebResponse)request.GetResponse();
    Console.WriteLine();
    Console.WriteLine($"OPP selesai. Response: {response.StatusCode}");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("OPP gagal atau ditolak.");
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"Detail: {ex.InnerException.Message}");
}
