# ADR-001: Modular Monolith ve Vertical Slice Yapısı

Durum: Kabul edildi (Kasım 2026)

## Bağlam
Projede ödeme, defter (ledger), mutabakat, hakediş ve bildirim gibi birbirinden ayrı iş alanları var. Tek kişi geliştirirken bunları baştan ayrı mikroservislere bölmek gereksiz dağıtık sistem ve deploy yükü getirecek. Diğer uçta tek bir `DbContext` ve katmanlı mimari (Clean Architecture) kurduğumuzda da modül başına 4 ayrı proje açılıp solution şişiyor ve sınırlar zamanla karışıyor.

## Karar
Uygulamayı Modular Monolith olarak kurguluyoruz. Her modül sadece iki projeden oluşacak:
- `LedgerFlow.Modules.<Module>`: Domain modelleri, `DbContext` ve Vertical Slice (`Features/`) klasörleri `internal` olarak burada duracak.
- `LedgerFlow.Modules.<Module>.Contracts`: Sadece modüller arası paylaşılacak DTO, event ve arayüzler burada olacak.

Tek bir PostgreSQL veritabanı kullanacağız ama her modül kendi şemasında (`payments`, `ledger` vb.) çalışacak. Modüllerin birbirinin iç sınıflarına veya şemasına doğrudan erişmemesi `ArchitectureTests` altındaki testlerle kontrol edilecek.

## Sonuçlar
Build süresi kısa kalıyor ve hakediş bakiye kontrolü gibi yerlerde ağ maliyeti olmadan bellek içi çağrı yapabiliyoruz. İleride mutabakat gibi yoğun bir modülü ayırmak gerekirse şeması ve sözleşmesi zaten ayrı olduğu için taşımak kolay olacak.
