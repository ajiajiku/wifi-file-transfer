using System.Net;
using System.Net.Sockets;
using System.Text;

public static class QuickSharePrototype
{
    private const string Service = "_FC9F5ED42C8A._tcp.local";
    private static readonly IPAddress MulticastAddress = IPAddress.Parse("224.0.0.251");
    private const int MdnsPort = 5353;

    public static void Run()
    {
        Console.WriteLine("WiFi File Transfer - Prototype 03");
        Console.WriteLine("Quick Share-style mDNS discovery");
        Console.WriteLine();
        Console.WriteLine($"Mencari service: {Service}");
        Console.WriteLine("Pastikan Windows dan ROSY-2 berada pada Wi-Fi yang sama.");
        Console.WriteLine();

        try
        {
            using var udp = new UdpClient(AddressFamily.InterNetwork);
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, MdnsPort));
            udp.JoinMulticastGroup(MulticastAddress);

            var query = BuildPtrQuery(Service);
            udp.Send(query, query.Length, new IPEndPoint(MulticastAddress, MdnsPort));

            Console.WriteLine("Query mDNS dikirim.");
            Console.WriteLine("Menunggu jawaban 8 detik...");

            var deadline = DateTime.UtcNow.AddSeconds(8);
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                        if (record.Type == 12)
                        {
                            if (found.Add(record.Target))
                                Console.WriteLine($"[Ditemukan] {record.Target}");
                        }
                        else if (record.Type == 33)
                        {
                            Console.WriteLine($"[SRV] {record.Name} -> {record.Target}:{record.Port}");
                        }
                        else if (record.Type == 1)
                        {
                            Console.WriteLine($"[IPv4] {record.Name} -> {record.Address}");
                        }
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    break;
                }
            }

            Console.WriteLine();
            if (found.Count == 0)
                Console.WriteLine("Tidak ada endpoint Quick Share ditemukan.");
            else
                Console.WriteLine($"Discovery selesai: {found.Count} endpoint/service ditemukan.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"mDNS gagal: {ex.Message}");
            Console.WriteLine("Pastikan Wi-Fi aktif dan firewall mengizinkan UDP 5353.");
        }

        Console.WriteLine();
        Console.Write("Tekan Enter untuk kembali...");
        Console.ReadLine();
    }

    private static byte[] BuildPtrQuery(string name)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((byte)0x12); bw.Write((byte)0x34);
        bw.Write((byte)0); bw.Write((byte)0);
        bw.Write((byte)0); bw.Write((byte)1);
        bw.Write((byte)0); bw.Write((byte)0);
        bw.Write((byte)0); bw.Write((byte)0);
        bw.Write((byte)0); bw.Write((byte)0);
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
        public string Name { get; init; } = "";
        public int Type { get; init; }
        public string Target { get; init; } = "";
        public int Port { get; init; }
        public string Address { get; init; } = "";
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
            offset += 4;
            if (offset > data.Length) return records;
        }

        int total = an + ns + ar;
        for (int i = 0; i < total && offset < data.Length; i++)
        {
            string name = ReadName(data, ref offset);
            if (offset + 10 > data.Length) break;

            int type = U16(data, offset); offset += 2;
            offset += 2;
            offset += 4;
            int rdLength = U16(data, offset); offset += 2;
            if (offset + rdLength > data.Length) break;

            var record = new DnsRecord { Name = name, Type = type };
            if (type == 1 && rdLength == 4)
            {
                record = new DnsRecord { Name = name, Type = type, Address = new IPAddress(data.AsSpan(offset, 4)).ToString() };
            }
            else if (type == 12)
            {
                int p = offset;
                record = new DnsRecord { Name = name, Type = type, Target = ReadName(data, ref p) };
            }
            else if (type == 33 && rdLength >= 6)
            {
                int p = offset;
                int port = U16(data, p + 4);
                p += 6;
                record = new DnsRecord { Name = name, Type = type, Port = port, Target = ReadName(data, ref p) };
            }

            records.Add(record);
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
