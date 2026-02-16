using System;

namespace SmartVocab.Domain.Common
{
    // abstract: bu sınıftan tek başına nesne üretimez, sadece miras alınır.
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // eşsiz ID
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}