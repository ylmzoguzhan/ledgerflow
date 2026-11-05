# LedgerFlow

Üye işyeri ödemelerini alan, bakiyeleri çift kayıtlı defterde (double-entry ledger) tutan, PSP/banka dosyalarıyla mutabakat yapan ve hakediş ödemelerini yöneten `.NET 10` backend projesi.

## Teknik Yapı
- `.NET 10`, ASP.NET Core Minimal API, `.NET Aspire`
- PostgreSQL 17 (`pgvector`), modül bazlı şema ayrımı
- Modular Monolith + Vertical Slice yapısı
- `xUnit`, `Testcontainers`, `WebApplicationFactory`, `NetArchTest`

## Dokümanlar
- [Domain Notları](docs/domain.md)
- [ADR-001: Modular Monolith ve Vertical Slice Yapısı](docs/adr/001-modular-monolith-and-vertical-slice.md)
- [ADR-002: Mesajlaşmada RabbitMQ.Client ve Kendi Outbox Yapımız](docs/adr/002-messaging-and-transactional-outbox.md)
- [ADR-003: AI Soyutlaması, Chat Modeli ve Lokal Embedding](docs/adr/003-ai-abstraction-and-providers.md)

## Çalıştırma

Gereksinimler: `.NET 10 SDK` ve `Docker`.

```bash
# Aspire ile API, Simulators ve PostgreSQL'i başlat
dotnet run --project src/LedgerFlow.AppHost

# Testleri çalıştır (mimari testler + Testcontainers)
dotnet test LedgerFlow.slnx
```

## Lab Notları
Geliştirme sürecinde denenen darboğaz ve hata senaryoları `docs/labs/` altında tutulacaktır.
