# ADR-003: AI Soyutlaması, Chat Modeli ve Lokal Embedding

Durum: Kabul edildi (Kasım 2026)

## Bağlam
Operasyon tarafında mutabakat farklarını üye işyeri sözleşmelerine göre açıklayan (RAG), itiraz e-postalarını sınıflandıran ve insan onaylı iade/düzeltme araçlarını çalıştıran bir asistan modülü olacak.

Burada iki kısıtımız var: Maliyetin düşük kalması ve tek bir sağlayıcı SDK'sına bağımlı olunmaması. Chat tarafında kullanacağımız DeepSeek API uygun maliyetli olsa da resmi bir embedding servisi sunmuyor. Ayrıca sözleşme metinlerini vektörleştirirken dış servise göndermemek veri gizliliği açısından daha güvenli.

## Karar
Kod içinde doğrudan sağlayıcı SDK'ları yerine `Microsoft.Extensions.AI` (`IChatClient` ve `IEmbeddingGenerator`) arayüzlerini kullanacağız.
- Chat ve tool calling için DeepSeek API (`deepseek-chat`) veya yapılandırmaya göre Gemini Flash kullanılacak.
- Embedding üretimi lokalde çalışan Ollama üzerindeki `nomic-embed-text` (768 boyut) modeliyle yapılacak ve `pgvector` kolonları `vector(768)` olarak açılacak.
- Dış LLM servislerine gerçek kart veya kişisel veri gönderilmeyecek, sadece maskelenmiş/sentetik operasyon verisi çıkacak.

## Sonuçlar
Embedding maliyeti sıfır oluyor ve doküman metinleri lokal ortamda kalıyor. Buna karşılık geliştirme ortamında Ollama servisinin açık olması gerekiyor. İleride farklı boyutta bir embedding modeline geçilirse mevcut vektör kayıtlarının yeniden üretilmesi gerekecek.
