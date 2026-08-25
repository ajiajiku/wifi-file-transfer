using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

Console.WriteLine("WiFi File Transfer - Prototype 01");
Console.WriteLine("Bluetooth discovery / OPP feasibility test");
Console.WriteLine();

using var client = new BluetoothClient();

Console.WriteLine("Mencari perangkat Bluetooth...");
var devices = client.DiscoverDevices();

if (devices.Count == 0)
{
    Console.WriteLine("Tidak ada perangkat Bluetooth ditemukan.");
    return;
}

var deviceList = devices.ToList();

for (int i = 0; i < deviceList.Count; i++)
{
    var device = deviceList[i];
    Console.WriteLine($"[{i}] {device.DeviceName} - {device.DeviceAddress}");
}

Console.WriteLine();
Console.Write("Pilih nomor perangkat Android untuk pengujian OPP: ");
if (!int.TryParse(Console.ReadLine(), out var index) || index < 0 || index >= deviceList.Count)
{
    Console.WriteLine("Pilihan tidak valid.");
    return;
}

var selected = deviceList[index];
Console.WriteLine($"Perangkat dipilih: {selected.DeviceName}");
Console.WriteLine($"Alamat: {selected.DeviceAddress}");
Console.WriteLine();

Console.WriteLine("Uji OPP akan dilakukan dengan koneksi langsung.");
Console.WriteLine();
Console.Write("Masukkan path file uji kecil (contoh C:\\Temp\\test.txt): ");
var localPath = Console.ReadLine()?.Trim().Trim('"');

if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
{
    Console.WriteLine("File tidak ditemukan. Prototype berhenti tanpa mengirim apa pun.");
    return;
}

var remoteName = Path.GetFileName(localPath);
Console.WriteLine();
Console.WriteLine($"Mengirim '{remoteName}' melalui Bluetooth OPP...");
Console.WriteLine("Perhatikan Android: apakah muncul permintaan/notifikasi penerimaan file?");

try
{
    var request = new ObexWebRequest(selected.DeviceAddress, remoteName);
    request.ReadFile(localPath);

    using var response = (ObexWebResponse)request.GetResponse();

    Console.WriteLine();
    Console.WriteLine($"OPP selesai. Response code: {response.StatusCode} (0x{(int)response.StatusCode:X2})");
    Console.WriteLine("Jika Android meminta Terima/Tolak, catat hasilnya untuk pengujian Prototype 01.");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("Pengiriman OPP gagal atau ditolak oleh perangkat.");
    Console.WriteLine($"Jenis error: {ex.GetType().Name}");
    Console.WriteLine($"Pesan: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"Detail: {ex.InnerException.Message}");
}
