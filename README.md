# LedgerFlow

> Bir ödeme kuruluşunun üye işyeri (merchant) ödemelerini aldığı, çift kayıtlı defterde (*double-entry ledger*) tuttuğu, PSP ve banka raporlarıyla **mutabakat** yaptığı, üye işyerlerine hakediş ödediği ve operasyon ekibine güvenli bir **AI asistanı** sunduğu `.NET 10` tabanlı referans backend platformu.

## Mimari ve Teknoloji Yığını

- **Mimari:** Modular Monolith + Vertical Slice ([ADR-001](docs/adr/001-modular-monolith-and-vertical-slice.md))
- **Çalışma Zamanı:** `.NET 10`, ASP.NET Core Minimal API, `.NET Aspire`
- **Veri Katmanı:** PostgreSQL 17 (`pgvector`), modül bazlı şema izolasyonu
- **Mesajlaşma:** `RabbitMQ.Client` v7 + Transactional Outbox / Idempotent Inbox ([ADR-002](docs/adr/002-messaging-and-transactional-outbox.md))
- **AI Katmanı:** `Microsoft.Extensions.AI` (`IChatClient` + lokal `nomic-embed-text` `vector(768)`) ([ADR-003](docs/adr/003-ai-abstraction-and-providers.md))
- **Test ve Kalite:** `xUnit`, `Testcontainers`, `WebApplicationFactory`, `NetArchTest`

## Dokümantasyon

- [Domain Tasarımı (Bounded Contexts, Durum Makinesi, Hesap Planı)](docs/domain.md)
- **Mimari Karar Kayıtları (ADR):**
  - [ADR-001: Modular Monolith ve Vertical Slice Mimarisi](docs/adr/001-modular-monolith-and-vertical-slice.md)
  - [ADR-002: Resmi RabbitMQ.Client v7 ve Özel Outbox/Inbox Katmanı](docs/adr/002-messaging-and-transactional-outbox.md)
  - [ADR-003: AI Soyutlama Katmanı, Chat Modeli ve Lokal Embedding Seçimi](docs/adr/003-ai-abstraction-and-providers.md)

## Hızlı Başlangıç

Gereksinimler: `.NET 10 SDK`, `Docker`.

```bash
# Tüm sistemi (.NET Aspire Dashboard + PostgreSQL 17 + Simulators + API) başlat
dotnet run --project src/LedgerFlow.AppHost

# Mimari testleri ve gerçek PostgreSQL 17 (Testcontainers) testlerini çalıştır
dotnet test LedgerFlow.slnx
```

## Mühendislik Lab Sonuçları (Before / After)

Her özellik geliştirilirken üretim ortamındaki gerçek arıza senaryoları `lab/*` dallarında yeniden üretilip ölçülmekte ve düzeltmeleriyle belgelenmektedir (`docs/labs/`):

| Lab | Konu | Hatalı Durum (Before) | Düzeltilmiş Durum (After) | Rapor |
| --- | --- | --- | --- | --- |
| *Yakında* | *Modül 03 ile başlayacak* | — | — | — |
