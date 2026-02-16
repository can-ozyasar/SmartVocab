using SmartVocab.Domain.Common;
using System.Collections.Generic;

namespace SmartVocab.Domain.Entities
{
    // Kelime türlerini Enum olarak tutmak, string olarak tutmaktan daha performanslıdır.
    public enum WordType
    {
        Noun,       // İsim
        Verb,       // Fiil
        Adjective,  // Sıfat
        Adverb,     // Zarf
        PhrasalVerb // Deyimsel Fiil
    }

    public enum CEFRLevel
    {
        A1, A2, B1, B2, C1, C2
    }

    public class Word : BaseEntity
    {
        // 1. Temel Veriler
        public string Text { get; set; } // Örn: "Ambiguous"
        public string Meaning { get; set; } // Örn: "Muğlak, belirsiz"
        public string? PronunciationUrl { get; set; } // Ses dosyası linki
        public string? ExampleSentence { get; set; } // Bağlam (Context) olmadan kelime öğrenilmez.

        // 2. Dilbilgisi Verileri (NLP İçin Kritik)
        public WordType Type { get; set; } // AI, fiilleri isimlerden daha zor öğrenebilir. Bunu bilmeli.
        public CEFRLevel Level { get; set; } // Senin bahsettiğin seviye.

        // 3. Veri Bilimi ve AI İçin Kritik Alanlar (Feature Engineering)
        
        // Kelimenin uzunluğu. Uzun kelimeler (genelde) daha zordur.
        // Bunu veritabanında tutmaya gerek yok, Text.Length ile hesaplarız ama model ister.
        
        // FrequencyIndex: Bu kelime günlük hayatta ne kadar sık kullanılıyor?
        // 0.0 (Hiç kullanılmaz) - 1.0 (Çok sık - "The", "And" gibi).
        // AI bunu kullanarak "Önce sık kullanılanları öğret" diyebilir.
        public double FrequencyIndex { get; set; } 

        // SimilarityVector: Senin "Benzerlik" fikrin. 
        // Modern AI (Word2Vec, BERT) kelimeleri sayı dizilerine çevirir.
        // [0.12, -0.98, 0.45...] gibi. 
        // Eğer iki kelimenin vektörü yakınsa, kullanıcı bunları karıştırabilir!
        // Şimdilik string olarak tutalım (ileride JSON veya pgvector yapacağız).
        public string? EmbeddingVector { get; set; } 
        
        // Levenshtein Distance (Benzer yazılan kelimeler için ipucu)
        // Örn: "Effect" vs "Affect". Bu karışıklığı modelin çözmesi için etiketliyoruz.
        public string? ConfusingPairs { get; set; } // JSON: ["Affect", "Defect"]
    }
}