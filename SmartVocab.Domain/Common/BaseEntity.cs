using System;

namespace SmartVocab.Domain.Common
{
    // abstract: Bu sınıftan tek başına nesne üretilemez, sadece miras alınır.
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique ID
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}