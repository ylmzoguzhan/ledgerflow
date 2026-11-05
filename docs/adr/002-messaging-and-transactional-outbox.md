# ADR-002: Mesajlaşmada RabbitMQ.Client ve Kendi Outbox Yapımız

Durum: Kabul edildi (Kasım 2026)

## Bağlam
Ödeme onaylandıktan sonra defter kaydının atılması ve üye işyerine webhook gönderilmesi kuyruk üzerinden asenkron ilerleyecek. Veri kaybı olmaması için Outbox, mükerrer işlemeyi engellemek için Inbox ve bozuk mesajlar için DLQ yapısına ihtiyacımız var.

MassTransit v9 ticari lisansa geçti. v8 sürümünde kalmak hem eski pakete bağımlılık yaratıyor hem de Outbox, retry ve DLQ mekanizmalarını hazır soyutlamaların arkasına gizliyor.

## Karar
Harici bir service bus kütüphanesi kullanmayacağız. Doğrudan `RabbitMQ.Client` (v7) paketi üzerine PostgreSQL `FOR UPDATE SKIP LOCKED` sorgusuyla çalışan kendi `outbox_messages` / `inbox_messages` tablolarımızı ve Dead-Letter Exchange yapımızı kuracağız.

## Sonuçlar
Bağlantı yönetimi, mesaj serileştirme, retry ve OpenTelemetry `traceparent` taşıma kodlarını (~200 satır) kendimiz yazıp bakımını yapacağız. Karşılığında lisans sorunu olmayacak, `.NET 10` geçişlerinde paket çakışması yaşamayacağız ve kuyruk davranışının tamamı kontrolümüzde olacak. İleride karmaşık Saga akışları gerekirse Wolverine değerlendirilebilir.
