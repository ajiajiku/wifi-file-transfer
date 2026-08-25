using System.Net;
using System.Net.Sockets;
using System.Text;

public static class QuickSharePrototype
{
    private const string ServiceEnumeration = "_services._dns-sd._udp.local";
    private static readonly IPAddress MulticastAddress = IPAddress.Parse("224.0.0.251");
    private const int MdnsPort = 5353;

    public static void Run()
    {
        Console.WriteLine("WiFi File Transfer - Prototype 03");
        Console.WriteLine("mDNS service scanner");
        Console.WriteLine();
        Console.WriteLine("Mencari semua service yang diumumkan di jaringan lokal...");
        Console.WriteLine("Pastikan ROSY-2 dan laptop berada pada Wi-Fi yang sama.");
        Console.WriteLine();

        try
        {
            using var udp = new UdpClient(AddressFamily.InterNetwork);
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, MdnsPort));
            udp.JoinMulticastGroup(MulticastAddress);

            var query = BuildPtrQuery(ServiceEnumeration);
            udp.Send(query, query.Length, new IPEndPoint(MulticastAddress, MdnsPort));

            Console.WriteLine("Query mDNS service enumeration dikirim.");
            Console.WriteLine("Menunggu jawaban 8 detik...");
            Console.WriteLine();

            var deadline = DateTime.UtcNow.AddSeconds(8);
            var services = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (DateTime.UtcNow < deadline)
            {
                var remaining = deadline - DateTime.UtcNow;
                udp.Client.ReceiveTimeout = Math.Max(100, (int)remaining.TotalMilliseconds);

                try
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = udp.Receive(ref remote);

                    foreach (var record in ParseRecords(buffer))
                    {
                        if (record.Type == 12 && !string.IsNullOrWhiteSpace(record.Target))
                        {
                            if (services.Add(record.Target))
                                Console.WriteLine($"[SERVICE] {record.Target}");
                        }
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    break;
                }
            }

            Console.WriteLine();
            if (services.Count == 0)
            {
                Console.WriteLine("Tidak ada service mDNS yang ditemukan.");
                Console.WriteLine("Jika ROSY-2 memiliki Quick Share aktif tetapi hasil tetap kosong,");
                Console.WriteLine("kemungkinan discovery dibatasi oleh versi Android/jaringan.");
            }
            else
            {
                Console.WriteLine($"Selesai. Ditemukan {services.Count} service:");
                foreach (var service in services.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    Console.WriteLine($"  {service}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"mDNS gagal: {ex.Message}");
            Console.WriteLine("Pastikan firewall mengizinkan UDP 5353 pada jaringan Private.");
        }

        Console.WriteLine();
        Console.Write("Tekan Enter untuk kembali...");
        Console.ReadLine();
    }

    private static byte[] BuildPtrQuery(string name)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        WriteU16(bw, 0x1234);
        WriteU16(bw, 0);
        WriteU16(bw, 1);
        WriteU16(bw, 0);
        WriteU16(bw, 0);
        WriteU16(bw, 0);
        WriteDnsName(bw, name);
        WriteU16(bw, 12);
        WriteU16(bw, 1);
        return ms.ToArray();
    }

    private static void WriteDnsName(BinaryWriter bw, string name)
    {
        foreach (var label in name.TrimEnd('.').Split('.'))
        {
            var bytes = Encoding.ASCII.GetBytes(label);
            bw.Write((byte)bytes.Length);
            bw.Write(bytes);
        }
        bw.Write((byte)0);
    }

    private static void WriteU16(BinaryWriter bw, int value)
    {
        bw.Write((byte)(value >> 8));
        bw.Write((byte)value);
    }

    private sealed class DnsRecord
    {
        public int Type { get; init; }
        public string Target { get; init; } = "";
    }

    private static List<DnsRecord> ParseRecords(byte[] data)
    {
        var records = new List<DnsRecord>();
        if (data.Length < 12) return records;

        int offset = 12;
        int qd = U16(data, 4);
        int an = U16(data, 6);
        int ns = U16(data, 8);
        int ar = U16(data, 10);

        for (int i = 0; i < qd; i++)
        {
            ReadName(data, ref offset);
            if (offset + 4 > data.Length) return records;
            offset += 4;
        }

        int total = an + ns + ar;
        for (int i = 0; i < total && offset < data.Length; i++)
        {
            ReadName(data, ref offset);
            if (offset + 10 > data.Length) break;

            int type = U16(data, offset); offset += 2;
            offset += 2;
            offset += 4;
            int rdLength = U16(data, offset); offset += 2;
            if (offset + rdLength > data.Length) break;

            if (type == 12)
            {
                int p = offset;
                string target = ReadName(data, ref p);
                records.Add(new DnsRecord { Type = type, Target = target });
            }

            offset += rdLength;
        }

        return records;
    }

    private static int U16(byte[] data, int p) => (data[p] << 8) | data[p + 1];

    private static string ReadName(byte[] data, ref int offset)
    {
        var labels = new List<string>();
        int p = offset;
        bool jumped = false;
        int guard = 0;

        while (p < data.Length && guard++ < 64)
        {
            int len = data[p++];
            if (len == 0) break;

            if ((len & 0xC0) == 0xC0)
            {
                if (p >= data.Length) break;
                int pointer = ((len & 0x3F) << 8) | data[p++];
                if (!jumped) offset = p;
                p = pointer;
                jumped = true;
                continue;
            }

            if (p + len > data.Length) break;
            labels.Add(Encoding.ASCII.GetString(data, p, len));
            p += len;
        }

        if (!jumped) offset = p;
        return string.Join('.', labels);
    }
}
