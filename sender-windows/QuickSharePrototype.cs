using System.Net.NetworkInformation;
using System.Net.Sockets;

public static class QuickSharePrototype
{
    public static void Run()
    {
        Console.WriteLine("WiFi File Transfer - Prototype 02");
        Console.WriteLine("Quick Share-style discovery / Wi-Fi transport scaffold");
        Console.WriteLine();

        Console.WriteLine("Network interfaces:");
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up) continue;
            var ips = nic.GetIPProperties().UnicastAddresses
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.Address.ToString());
            Console.WriteLine($"- {nic.Name}: {string.Join(", ", ips)}");
        }

        Console.WriteLine();
        Console.WriteLine("Prototype 02 status:");
        Console.WriteLine("[1] Bluetooth discovery: Prototype 01 verified");
        Console.WriteLine("[2] Quick Share protocol: next implementation stage");
        Console.WriteLine("[3] Wi-Fi transport: scaffold ready");
        Console.WriteLine();
        Console.WriteLine("Belum melakukan transfer file. Jalur Quick Share akan ditambahkan bertahap.");
        Console.WriteLine();
        Console.Write("Tekan Enter untuk kembali...");
        Console.ReadLine();
    }
}
