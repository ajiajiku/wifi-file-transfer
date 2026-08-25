# Prototype 01 — Bluetooth discovery dan OPP

## Tujuan
Menguji asumsi utama proyek: apakah Windows dapat menggunakan Bluetooth untuk menemukan Android dan mengirim objek kecil melalui Bluetooth OPP sehingga layanan bawaan Android menampilkan permintaan penerimaan tanpa aplikasi proyek terpasang.

## Hipotesis
Jika OPP tersedia dan layanan Bluetooth Android menangani permintaan tersebut, Bluetooth dapat menjadi jalur pemanggil/handshake. Setelah pengguna menerima, transfer file sebenarnya akan dicoba melalui Wi-Fi.

## Batasan
- Android 7.0+ adalah target minimum.
- Perilaku dapat berbeda antar vendor Android.
- Handoff otomatis dari OPP ke Wi-Fi belum dianggap tersedia sebelum dibuktikan dengan pengujian.

## Urutan eksperimen
1. Aktifkan Bluetooth dan Wi-Fi pada Windows dan Android.
2. Pair kedua perangkat jika diperlukan.
3. Deteksi nama dan alamat perangkat Android dari Windows.
4. Kirim file uji kecil melalui mekanisme OPP.
5. Catat apakah Android menampilkan permintaan penerimaan sistem.
6. Pilih Terima dan catat lokasi hasil serta perilaku koneksi.
7. Jika tahap 5 berhasil, rancang paket handshake yang berisi metadata file dan endpoint Wi-Fi.
8. Jangan mengirim file besar melalui Bluetooth pada prototype ini.

## Hasil
Belum diuji pada perangkat Android nyata.

## Keputusan berikutnya
- Jika OPP memunculkan UI penerimaan sistem: lanjut ke prototype handoff Bluetooth→Wi-Fi.
- Jika OPP tidak tersedia/terblokir: evaluasi mekanisme bawaan Android lain sebelum membuat aplikasi Android.
