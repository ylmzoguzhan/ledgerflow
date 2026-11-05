# ADR-002: Mesajlaşma Altyapısı — Resmi RabbitMQ.Client v7 ve Özel Outbox/Inbox Katmanı

- **Durum:** Kabul Edildi
- **Tarih:** 2026-11-05

## Bağlam
LedgerFlow'da `PaymentCaptured` → `Ledger` yevmiye kaydı, `Notifications` outbound webhook gönderimi ve `Payouts` banka transferleri gibi kritik akışlar **at-least-once** teslim garantisi, **Transactional Outbox**, **Idempotent Inbox**, **Dead-Letter Queue (DLQ)** ve uçtan uca **OpenTelemetry (`traceparent`)** yayılımı gerektirir.

.NET ekosisteminde mesajlaşma soyutlaması seçilirken 2025–2026 itibarıyla şu tablo söz konusudur:
- **MassTransit:** v9 sürümüyle birlikte ticari (ücretli) lisans modeline geçmiştir; v8 açık kaynak (Apache 2.0) kalsa da yeni özellik geliştirme ticari sürüme taşınmıştır ve `.NET 10` / `EF Core 10` geçişlerinde paket bağımlılığı kilidi oluşturma riski taşır.
- **Wolverine (`WolverineFx`):** MIT lisanslı ve yerleşik Postgres Outbox desteğine sahiptir; ancak mesaj yönlendirme, kilit yenileme ve hata kuyruğu davranışlarını kendi framework kuralları arkasına soyutlar.
- **Resmi `RabbitMQ.Client` (v7, tam asenkron API) + İnce `BuildingBlocks.Messaging` Katmanı:** Sıfır üçüncü parti lisans maliyeti, tam `.NET 10` uyumu ve kaputun altındaki her teslimat garantisinin açıkça kontrol edilebilmesi.

## Karar
Mesajlaşma katmanında üçüncü parti bir servis otobüsü (service bus framework) kullanılmayacak; resmi **`RabbitMQ.Client` (v7)** kütüphanesi üzerine ince bir **`LedgerFlow.BuildingBlocks.Messaging`** altyapısı inşa edilecektir:
1. **Transactional Outbox:** Modül `DbContext`'i `SaveChangesAsync` sırasında entegrasyon olaylarını aynı DB transaction'ı içinde `outbox_messages` tablosuna yazar. Arka plan okuyucusu PostgreSQL `SELECT ... FOR UPDATE SKIP LOCKED` ile mesajları çekip RabbitMQ'ya **Publisher Confirms** açık şekilde yayınlar.
2. **Idempotent Inbox:** Tüketiciler (`Consumer`) mesajı işlemeden/işlerken `(message_id, consumer_name)` çiftini `inbox_messages` tablosuna yazar; aynı mesaj ikinci kez gelse bile iş mantığı yalnızca bir kez tetiklenir.
3. **Retry ve DLQ:** Geçici hatalarda üstel bekleme (exponential backoff) uygulanır; maksimum deneme sayısını aşan veya şema hatası veren zehirli mesajlar (`poison messages`) `BasicNack(requeue: false)` ile RabbitMQ **Dead-Letter Exchange (`dlx`)** üzerinden `*.dlq` kuyruğuna yönlendirilir.
4. **Gözlemlenebilirlik:** Mesaj başlıklarına (`IBasicProperties.Headers`) W3C `traceparent` ve `tracestate` enjekte edilerek HTTP → EF Core → RabbitMQ → Worker zinciri tek bir OpenTelemetry trace altında birleştirilir.

## Reddedilen Alternatifler
- **MassTransit v9:** Ticari lisans maliyeti nedeniyle elendi.
- **MassTransit v8 (Sabitlenmiş):** Bakım modunda kalması, `.NET 10` / `EF Core 10` yükseltmelerinde sürüm çakışması riski ve Outbox/DLQ mekanizmalarını kara kutu haline getirmesi nedeniyle elendi.
- **WolverineFx:** Çok adımlı karmaşık Saga orkestrasyonlarına ihtiyaç duyulsaydı en güçlü MIT lisanslı aday olacaktı; ancak mevcut topolojimiz yalın olay yayını (pub/sub) ve iş kuyruklarından oluştuğu için ek soyutlama katmanı tercih edilmedi.

## Sonuçlar ve Ödünleşimler
- **Olumlu:** Sıfır lisans riski; `.NET 10` ve `EF Core 10` ile doğrudan uyum; `SKIP LOCKED`, `ACK/Nack`, `DLQ` ve `traceparent` davranışları üzerinde %100 şeffaflık ve kontrol.
- **Olumsuz / Risk:** Bağlantı kopması (auto-recovery), kanal yönetimi ve serileştirme kodunun bakım sorumluluğu bize aittir (~250 satır altyapı kodu). İleride çok adımlı durum makineli (stateful) dağıtık Saga ihtiyacı doğarsa bu karar yeniden değerlendirilip `WolverineFx`'e geçiş düşünülebilir.
