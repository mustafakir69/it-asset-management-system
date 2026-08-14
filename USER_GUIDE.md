# Kullanıcı Rehberi

## 1. Giriş yapma

Login ekranında kullanıcı adı/e-posta ve parolanızı girin. Başarılı girişten sonra rolünüze uygun Dashboard açılır.

## 2. Dashboard

Dashboard; cihaz, zimmet, stok, garanti, lisans ve bakım özetlerini gerçek sistem verilerinden gösterir. Alt listeler son hareketleri ve öncelikli uyarıları içerir.

## 3. Cihaz ekleme ve düzenleme

Admin/IT, **Envanter → Yeni Cihaz** ile kayıt oluşturabilir. Cihaz listesindeki işlem menüsünden detay açılır veya düzenleme yapılır. Varlık kodu ve seri numarası benzersiz olmalıdır.

## 4. Zimmet oluşturma

Admin/IT, **Zimmet → Yeni Zimmet** ekranında yalnızca uygun ve stoktaki cihazları aktif çalışana atayabilir. Aynı cihazın ikinci aktif zimmeti oluşturulamaz.

## 5. İade alma

Aktif Zimmetler veya İade İşlemleri ekranından kayıt seçilir; iade tarihi, teslim alan ve not girilir. İşlem tamamlanınca cihaz yeniden stok durumuna döner.

## 6. Stok işlemleri

Stok ekranından ürünleri inceleyin. Admin/IT giriş veya çıkış hareketi yapabilir. Sistem mevcut miktardan fazla çıkışı ve negatif stoğu engeller. Minimum seviyedeki ürünler Kritik olarak gösterilir.

## 7. Garanti takibi

Garantiler ekranında cihazların bitiş tarihleri ve kalan günleri görünür. 30 gün veya daha az kalan kayıtlar Yaklaşıyor olarak işaretlenir.

## 8. Lisans yönetimi

Admin/IT lisans ekleyip düzenleyebilir. Kullanılan lisans hakkı toplam haktan fazla olamaz. Auditor liste ve detayları salt okunur görür.

## 9. Bakım planı

Admin/IT periyodik plan oluşturur; cihazı, sıklığı, başlangıç tarihini ve sorumlu teknisyeni belirler. Plan tamamlanan görevden sonra bir sonraki görevi üretir.

## 10. Bakım talebi

Bakım Talepleri ekranında talep oluşturulabilir, teknisyen atanabilir, işleme alınabilir ve sonuçlandırılabilir. **Bakım Taleplerim**, güvenli çalışan ilişkisi sonraki sürümde eklendiğinde aktif olacaktır.

## 11. Rapor alma

Admin/IT/Auditor, Rapor Merkezi’nden Envanter, Zimmet, Stok ve Bakım raporlarına gider. Filtrelenen rapor CSV olarak indirilebilir.

## 12. Audit Log

Admin ve Auditor, **Yönetim → Audit Logları** üzerinden kritik işlemleri tarih, kullanıcı, entity ve işlem türüyle inceler. Uzun eski/yeni değerler detay penceresinde açılır.

## 13. Bildirimleri işleme

Admin/IT, Swagger üzerinden `POST /api/notifications/process` çağrısıyla kritik stok ve bakım bildirimlerini işler. Development ortamında gerçek e-posta yerine LogOnly kayıtları oluşur.

## 14. Çıkış yapma

Sağ üstteki **Çıkış Yap** düğmesi oturumu güvenli biçimde sonlandırır.

## Rol farkları

- **Admin:** Tüm operasyonlar, kullanıcı yönetimi, rapor ve audit.
- **IT:** Operasyonel yazma işlemleri, kullanıcı oluşturma kuralları ve raporlar; audit erişimi yoktur.
- **Employee:** Kendi zimmetleri ve kendine yönelik ekranlar.
- **Auditor:** Envanter/operasyon verileri, raporlar ve audit için salt okunur erişim.
