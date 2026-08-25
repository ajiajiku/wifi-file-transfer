using Makaretu.Dns;

public static class QuickSharePrototype
{
    private const string QuickShareService = "_FC9F5ED42C8A._tcp";

    public static void Run()
    {
        Console.WriteLine("WiFi File Transfer - Prototype 03");
        Console.WriteLine("Quick Share-style mDNS discovery");
        Console.WriteLine();
        Console.WriteLine($"Mencari service: {QuickShareService}");
        Console.WriteLine("Pastikan Windows dan ROSY-2 berada pada Wi-Fi yang sama.");
        Console.WriteLine();

        using var discovery = new ServiceDiscovery();
        var found = new List<string>();

        discovery.ServiceInstanceDiscovered += (_, e) =>
        {
            var name = e.ServiceInstanceName.ToString();
            lock (found)
            {
                if (!found.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    found.Add(name);
                    Console.WriteLine($"[Ditemukan] {name}");
                }
            }
        };

        discovery.QueryServiceInstances(QuickShareService);

        Console.WriteLine("Menunggu 8 detik...");
        Thread.Sleep(8000);

        Console.WriteLine();
        if (found.Count == 0)
        {
            Console.WriteLine("Tidak ada perangkat Quick Share yang ditemukan.");
            Console.WriteLine("Periksa Wi-Fi, Quick Share, dan izin firewall Windows.");
        }
        else
        {
            Console.WriteLine($"Ditemukan {found.Count} endpoint Quick Share.");
            Console.WriteLine("Tahap berikutnya: ambil alamat/port endpoint lalu koneksi Wi-Fi.");
        }

        Console.WriteLine();
        Console.Write("Tekan Enter untuk kembali...");
        Console.ReadLine();
    }
}
