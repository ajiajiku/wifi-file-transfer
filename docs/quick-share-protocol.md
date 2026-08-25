# Prototype 02 — Quick Share protocol

Kita mengubah arah proyek dari OPP murni menjadi interoperabilitas dengan protokol Quick Share/Nearby Share.

## Target
- Windows sender
- Android 7+
- Tanpa aplikasi receiver buatan kita pada Android, jika layanan Quick Share bawaan perangkat dapat menerima.
- Bluetooth/BLE untuk discovery/bootstrap.
- Wi-Fi LAN atau Wi-Fi Direct untuk transfer.

## Jalur protokol
1. Discovery/bootstrap melalui BLE.
2. Bootstrap koneksi.
3. mDNS/TCP untuk jalur jaringan bila tersedia.
4. UKEY2 untuk handshake dan autentikasi/enkripsi.
5. Negosiasi medium upgrade ke Wi-Fi/Wi-Fi Direct.
6. Transfer file melalui koneksi terenkripsi.

## Referensi implementasi
- NearDrop — implementasi open-source protokol Nearby Share/Quick Share.
- Bada — implementasi Android interoperabel Quick Share, mendukung Android 7+ dan Wi-Fi Direct.
- NearShare — implementasi desktop independen.

## Catatan desain
Kita tidak menyalin kode proprietary Google. Kita mengimplementasikan protokol berdasarkan dokumentasi, source open-source, dan format interoperabilitas yang tersedia.

## Tahap implementasi
- [ ] Discovery BLE
- [ ] Bootstrap endpoint
- [ ] Protobuf framing
- [ ] UKEY2
- [ ] Secure channel
- [ ] Wi-Fi LAN transport
- [ ] Wi-Fi Direct medium upgrade
- [ ] File sender GUI
- [ ] Interoperability test dengan Quick Share ROSY-2
