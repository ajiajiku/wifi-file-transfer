using InTheHand.Net.Bluetooth;
using InTheHand.Net.Obex;
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

for (int i = 0; i < devices.Count; i++)
{
    var device = devices.ElementAt(i);
    Console.WriteLine($"[{i}] {device.DeviceName} - {device.DeviceAddress}");
}

Console.WriteLine();
Console.Write("Pilih nomor perangkat Android untuk pengujian OPP: ");
if (!int.TryParse(Console.ReadLine(), out var index) || index < 0 || index >= devices.Count)
{
    Console.WriteLine("Pilihan tidak valid.");
    return;
}

var selected = devices.ElementAt(index);
Console.WriteLine($"Perangkat dipilih: {selected.DeviceName}");
Console.WriteLine($"Alamat: {selected.DeviceAddress}");
Console.WriteLine();

Console.WriteLine("Mencari layanan Bluetooth OPP...");
var records = selected.GetServiceRecords(BluetoothService.ObexObjectPush);
Console.WriteLine($"Jumlah service record OPP: {records.Length}");

if (records.Length == 0)
{
    Console.WriteLine("OPP tidak terdeteksi pada perangkat ini.");
    return;
}

Console.WriteLine("OPP terdeteksi.");
Console.WriteLine();
Console.WriteLine("Prototype discovery selesai.");
Console.WriteLine("Tahap berikutnya: kirim file uji kecil melalui ObexWebRequest dan amati apakah Android menampilkan dialog penerimaan sistem.");
