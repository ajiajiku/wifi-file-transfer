# WiFi File Transfer

Proyek aplikasi Windows untuk mengirim file ke perangkat Android melalui jaringan lokal Wi-Fi, dengan Bluetooth digunakan sebagai jalur discovery/handshake.

## Tujuan

Membuat pengalaman pengiriman file sederhana seperti Bluetooth:

1. Laptop menemukan perangkat Android.
2. Laptop memilih file dan mengirim permintaan.
3. Android menampilkan permintaan penerimaan melalui mekanisme sistem yang tersedia.
4. Setelah diterima, data file dipindahkan melalui Wi-Fi untuk mendapatkan kecepatan lebih tinggi.

## Target

- Laptop: Windows 10/11 pada tahap awal.
- Android: Android 7.0+ sebagai target kompatibilitas.
- Internet tidak diperlukan; transfer berlangsung di jaringan lokal.

## Arsitektur awal

```text
                BLUETOOTH
Laptop --------------------------> Android
       discovery / handshake

                WI-FI
Laptop ==========================> Android
              file transfer
```

## Catatan penting

Android 7+ tidak memiliki API sistem universal yang menjamin aplikasi pihak ketiga di Windows dapat memunculkan dialog penerimaan file kustom tanpa adanya komponen penerima di Android. Karena itu, tahap awal proyek akan melakukan validasi terhadap kemampuan Bluetooth Object Push Profile (OPP) dan layanan file bawaan Android untuk skenario handshake/handoff.

Kita tidak akan menganggap fitur tersebut tersedia sebelum diuji pada perangkat nyata.

## Struktur proyek

- `docs/architecture.md` — rancangan sistem.
- `docs/protocol.md` — rancangan protokol komunikasi.
- `sender-windows/` — aplikasi pengirim Windows.
- `tests/` — pengujian protokol dan konektivitas.

## Status

**Tahap 0 — Perancangan dan validasi teknologi.**

Langkah pertama adalah membuktikan alur Bluetooth discovery/handshake dan jalur Wi-Fi transfer sebelum membuat antarmuka aplikasi final.
