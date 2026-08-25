# Protokol Komunikasi

Dokumen ini adalah rancangan awal dan belum menyatakan bahwa seluruh tahap dapat dilakukan tanpa komponen Android tambahan.

## Sesi

Setiap transfer mempunyai `session_id` acak.

Contoh metadata permintaan:

```json
{
  "version": 1,
  "session_id": "random-session-id",
  "sender_name": "Laptop-Aji",
  "file_name": "photo.jpg",
  "file_size": 8421376,
  "sha256": "..."
}
```

## Tahapan

### DISCOVERY

Windows mencari perangkat yang tersedia melalui Bluetooth.

### REQUEST

Windows menyiapkan metadata file dan meminta Android menerima transfer.

### ACCEPT / REJECT

Sesi hanya boleh masuk ke tahap transfer setelah penerimaan berhasil.

### WIFI_CONNECT

Kedua sisi menentukan endpoint Wi-Fi untuk sesi tersebut.

### TRANSFER

File dikirim sebagai byte stream dengan ukuran blok yang dapat disesuaikan. Pengirim mengirim progres berdasarkan jumlah byte yang sudah diterima oleh endpoint.

### VERIFY

Penerima menghitung SHA-256 file hasil transfer dan membandingkannya dengan metadata.

### COMPLETE

Jika verifikasi berhasil, sesi ditutup.

## Kegagalan

Sistem harus menangani:

- Bluetooth terputus sebelum handshake selesai;
- Wi-Fi endpoint tidak tersedia;
- pengguna menolak transfer;
- transfer terputus;
- checksum tidak cocok;
- file tujuan sudah ada;
- perangkat kehabisan ruang penyimpanan.

## Catatan Android 7+

Bagian yang paling berisiko adalah mekanisme `REQUEST` dan `ACCEPT/REJECT` tanpa aplikasi Android. Implementasi awal akan menguji layanan Bluetooth/File Transfer bawaan terlebih dahulu. Bila layanan sistem tidak menyediakan hook yang diperlukan, protokol ini tetap dapat dipakai dengan penerima Android tambahan sebagai fallback.
