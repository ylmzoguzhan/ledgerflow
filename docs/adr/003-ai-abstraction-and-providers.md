# ADR-003: AI Soyutlama Katmanı, Chat Modeli ve Lokal Embedding Seçimi

- **Durum:** Kabul Edildi
- **Tarih:** 2026-11-05

## Bağlam
`AICopilot` modülü dört temel yetenek sunar:
1. Üye işyeri sözleşmeleri üzerinde tenant bazlı filtrelenen **RAG** ile mutabakat uyuşmazlıklarını (`Discrepancy`) açıklama,
2. İtiraz ve destek e-postalarını yapılandırılmış JSON çıktısıyla sınıflandırma,
3. İnsan onayı (`Human-in-the-Loop`) ile iade/düzeltme araçlarını (`Tool Calling`) çağırma,
4. Operasyon paneline **SSE (Server-Sent Events)** ile akışlı yanıt verme.

Burada iki teknik kısıt vardır:
- **Sağlayıcı kilidi ve maliyet:** Doğrudan tek bir sağlayıcının SDK'sına bağımlı kalmak fiyat/kota değişimlerinde kırılganlık yaratır. Ayrıca geliştirme ve CI `Eval` çalışmalarının toplam maliyeti çok düşük tutulmalıdır (< 200 TL).
- **Embedding ayrımı ve veri gizliliği:** Sohbet için fiyat/performans oranı çok yüksek olan **DeepSeek API** resmi bir embedding endpoint'i sunmamaktadır. Öte yandan üye işyeri sözleşme metinlerinin vektörleştirme aşamasında şirket dışına çıkmaması fintech veri gizliliği (KVKK / PCI-DSS sınırları) açısından avantajlıdır.

## Karar
1. **Standart Soyutlama:** Uygulama kodunda sağlayıcıya özel SDK tipleri kullanılmayacak; tüm LLM ve vektör işlemleri resmi **`Microsoft.Extensions.AI`** arayüzleri (`IChatClient` ve `IEmbeddingGenerator<string, Embedding<float>>`) üzerinden enjekte edilecektir.
2. **Sohbet ve Araç Çağrısı (`IChatClient`):**
   - Varsayılan sağlayıcı OpenAI uyumlu endpoint üzerinden **DeepSeek API (`deepseek-chat`)** (veya yapılandırma ile **Gemini Flash**) olacaktır.
   - Çevrimdışı geliştirme için lokal Ollama (`qwen2.5:7b`) aynı `IChatClient` arayüzüyle desteklenecektir.
3. **Embedding (`IEmbeddingGenerator`) ve `pgvector` Boyutu:**
   - Vektör üretimi için **lokal Ollama üzerinde `nomic-embed-text` (274 MB, 768 boyut)** kullanılacaktır.
   - PostgreSQL `pgvector` kolonları **`vector(768)`** olarak standartlaştırılacaktır. (CI ortamında küçük konteyner imajı veya aynı 768 boyutlu bulut karşılığı kullanılabilir.)
4. **Veri Sınırı:** Bulut LLM sağlayıcısına yalnızca sentetik veya maskelenmiş operasyonel veriler gönderilir; ham kart bilgisi, API anahtarı veya kişisel veri (`PII`) prompt içerisine asla dahil edilmez.

## Reddedilen Alternatifler
- **Doğrudan Sağlayıcı SDK'sı (`OpenAI` / `Google` SDK'sını iş katmanına gömmek):** Sağlayıcı değişimini ve birim/eval testlerinde sahte (`FakeChatClient`) kullanımını zorlaştırdığı için reddedildi.
- **Sohbet modelini de tamamen lokal CPU/GPU'da koşturmak:** 7B–8B modeller basit özetlemede yeterli olsa da karmaşık çok adımlı `Tool Calling` ve katı JSON şema uyumunda bulut modelleri (`deepseek-chat` / `gemini-flash`) çok düşük maliyetle (~50–150 TL toplam proje maliyeti) çok daha yüksek doğruluk sağlar.

## Sonuçlar ve Ödünleşimler
- **Olumlu:** Embedding maliyeti **0 TL** ve sözleşme metinleri lokal ortamdan dışarı çıkmaz; sohbet tarafında düşük maliyetle yüksek `Tool Calling` doğruluğu elde edilir; `IChatClient` sayesinde sağlayıcı değişimi tek bir DI kaydıyla yapılır.
- **Olumsuz / Dikkat Edilecek Nokta:** Embedding modeli (`vector(768)`) değiştirildiğinde mevcut vektör uzayı uyumsuz olacağından `copilot` şemasındaki dokümanların yeniden vektörleştirilmesi (`re-indexing`) gerekir. Ayrıca lokal geliştirme ortamında `ollama serve` servisinin ayakta olması gerekir.
