# ADR-001: Modular Monolith ve Vertical Slice Mimarisi

- **Durum:** Kabul Edildi
- **Tarih:** 2026-11-05

## Bağlam
LedgerFlow; kimlik (`Identity`), ödeme (`Payments`), çift kayıtlı defter (`Ledger`), mutabakat (`Reconciliation`), hakediş (`Payouts`), bildirim (`Notifications`), kur (`FX`) ve operasyon asistanı (`AICopilot`) olmak üzere 8 farklı iş alanını kapsar.

Tek kişilik / küçük bir mühendislik ekibiyle başlarken iki uç tehlike vardır:
1. **Erken Mikroservis (Premature Microservices):** 8 ayrı deploy birimi, ağ gecikmeleri, dağıtık transaction karmaşası ve operasyonel yükün daha ilk günden hızı öldürmesi.
2. **Big Ball of Mud / Aşırı Katmanlı Monolith:** Tek bir `AppDbContext` üzerinden tüm tabloların birbirine `JOIN` atması veya her modül için 4 yatay Clean Architecture projesi (`8 × 4 = 32 .csproj`) açılarak build süresinin ve boilerplate kodun şişmesi.

## Karar
1. Sistem **Modular Monolith** olarak tasarlanacak; dağıtılabilir birimler yalnızca `LedgerFlow.Api`, `LedgerFlow.Worker` ve `LedgerFlow.Backoffice` olacaktır.
2. Her modül yalnızca **2 `.csproj`** içerir:
   - `LedgerFlow.Modules.<Module>`: Tüm domain modelleri, `DbContext` ve iş özellikleri **`internal`** erişim belirleyicisiyle bu projede yaşar.
   - `LedgerFlow.Modules.<Module>.Contracts`: Yalnızca modüller arası `public` DTO'lar, Integration Event'ler ve dar kapsamlı modül arayüzleri (`ILedgerModule` gibi) burada bulunur.
3. Her modül içi yatay katmanlar yerine **Vertical Slice (`Features/<UseCase>/`)** şeklinde organize edilir; ilgili endpoint, komut/sorgu, doğrulayıcı ve handler aynı klasörde durur.
4. Veri izolasyonu tek bir PostgreSQL sunucusu üzerinde **modül başına ayrı DB şeması** (`payments.*`, `ledger.*`, `reconciliation.*` vb.) ile sağlanır. Modüller arası şema erişimi ve proje referansı kuralları CI'da `NetArchTest` ile zorunlu kılınır.

## Reddedilen Alternatifler
- **Mikroservis Mimarisi:** Trafik ve ekip ölçeği modül bazlı bağımsız ölçeklemeyi henüz gerektirmiyor; modül sınırları `.Contracts` ve ayrı DB şemalarıyla korunduğu için ileride dar boğaz olan tek bir modülü (ör. `Reconciliation`) ayırmak düşük maliyetlidir.
- **Modül Başına 4 Katmanlı Clean Architecture (`Domain` / `Application` / `Infrastructure` / `Presentation`):** 32+ proje oluşturarak derleme süresini uzatır ve tek bir özellik eklemek için 4 farklı projede dosya açmayı zorunlu kılar. `internal` görünürlük sınırı C# derleyicisi seviyesinde zaten modül kapsüllemesini sağlar.

## Sonuçlar ve Ödünleşimler
- **Olumlu:** Tek repo ve hızlı derleme; `Payouts` → `Ledger` bakiye rezervasyonu gibi kritik akışlarda ağ maliyeti olmadan senkron bellek içi çağrı imkânı; ihtiyaç duyulduğunda bir modülü ayrı servise koparmaya hazır şema izolasyonu.
- **Olumsuz / Risk:** Geliştiricinin (veya kod üreten AI ajanının) yanlışlıkla başka modülün şemasına SQL yazması veya `internal` sınırını delmesi riski vardır; bu risk `NetArchTest` mimari testleri ve DB yetkileri ile kontrol altında tutulacaktır.
