using SmartVocab.Domain.Common;
using System;
using System.Collections.Generic;

namespace SmartVocab.Domain.Entities
{
    public class User : BaseEntity
    {
        // --- KİMLİK BİLGİLERİ (Identity) ---
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Şifreyi asla düz metin tutmayız!

        // --- DEMOGRAFİK VERİLER (AI için Statik Girdiler) ---
        
        // Yaş, öğrenme hızını doğrudan etkiler (Nöroplastisite).
        // Veritabanında yaş tutulmaz, Doğum Tarihi tutulur. Yaş hesaplanır.
        public DateTime DateOfBirth { get; set; } 
        
        // Ana dil, öğrenilen dildeki hataları tahmin ettirir. 
        // Örn: Türkler "I go to home" der (yanlış), çünkü Türkçede "Eve gidiyorum" denir.
        public string NativeLanguage { get; set; } = "Turkish"; 

        // Eğitim seviyesi veya meslek. 
        // Mühendisler teknik terimleri daha hızlı öğrenir.
        public string? Occupation { get; set; } 

        // --- HEDEF VE MOTİVASYON (Gamification) ---
        
        // Günlük hedef (Dakika cinsinden). Model buna göre kelime sayısı belirler.
        public int DailyGoalMinutes { get; set; } = 15;
        
        // Kullanıcının mevcut seviyesi (Placement Test sonucu).
        public CEFRLevel CurrentLevel { get; set; } = CEFRLevel.A1;

        // --- NAVİGASYON PROPERTY'LERİ (İlişkiler) ---
        // Bir kullanıcının birden fazla kelime geçmişi olabilir.
        // Bu, veritabanında "One-to-Many" ilişki kurar.
        public ICollection<UserWordState> UserWordStates { get; set; }
    }
}