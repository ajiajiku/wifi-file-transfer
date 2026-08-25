# Arsitektur Sistem

## 1. Tujuan

Sistem terdiri dari aplikasi pengirim di Windows dan layanan penerimaan yang sebisa mungkin memanfaatkan kemampuan bawaan Android. Fokus utama adalah memisahkan jalur kontrol dari jalur data.

## 2. Dua jalur komunikasi

### Jalur kontrol — Bluetooth

Bluetooth digunakan untuk:

- menemukan perangkat Android di sekitar laptop;
- mengenali perangkat yang dipilih;
- melakukan handshake awal;
- membawa informasi minimal tentang permintaan transfer, bila kemampuan perangkat memungkinkan.

Bluetooth tidak digunakan sebagai jalur utama untuk file besar.

### Jalur data — Wi-Fi

Wi-Fi digunakan untuk:

- membangun koneksi transfer lokal;
- mengirim data file secara streaming;
- menampilkan progres dan kecepatan;
- mendukung file berukuran besar tanpa memuat seluruh file ke RAM.

## 3. Alur utama

```text
[Windows Sender]
      |
      | Bluetooth discovery
      v
[Android device]
      |
      | handshake / transfer request
      v
[Acceptance mechanism]
      |
      +---- Reject ----> selesai
      |
      +---- Accept ----> Wi-Fi session
                              |
                              v
                         File streaming
                              |
                              v
                           Complete
```

## 4. Validasi teknologi

Sebelum implementasi UI final, kita perlu menguji pada perangkat Android nyata:

1. Bluetooth discovery dari Windows.
2. Kemampuan Bluetooth OPP atau layanan bawaan Android untuk menerima objek/file.
3. Apakah penerimaan dapat menampilkan notifikasi/dialog sistem.
4. Cara mendapatkan informasi alamat/IP Wi-Fi setelah handshake.
5. Apakah transfer dapat dialihkan dari Bluetooth ke Wi-Fi tanpa aplikasi Android tambahan.

## 5. Fallback

Jika Android 7+ tidak menyediakan mekanisme bawaan yang dapat menerima handshake buatan secara universal, proyek akan mempertahankan desain modular sehingga kemudian dapat ditambahkan opsi penerima Android ringan tanpa mengubah protokol Wi-Fi utama.

## 6. Prinsip keamanan

- Jangan menerima file secara diam-diam.
- Permintaan harus mengandung nama file dan ukuran.
- Transfer harus hanya terjadi setelah penerimaan pengguna bila mekanisme sistem mendukungnya.
- Wi-Fi transfer harus dibatasi ke sesi yang telah melakukan handshake.
- Token sesi acak akan digunakan untuk mencegah perangkat lain di jaringan menyisipkan transfer.
