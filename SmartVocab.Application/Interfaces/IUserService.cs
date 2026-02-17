using SmartVocab.Application.DTOs.User;
using System;
using System.Threading.Tasks;

namespace SmartVocab.Application.Interfaces
{
    public interface IUserService
    {
        // Geriye "Guid" (oluşan kullanıcının ID'si) döneceğiz.
        Task<Guid> RegisterAsync(RegisterUserDto registerDto);
        
        // Mevcut Register metodunun altına:
        // Geriye "string" (JWT Token) döneceğiz.
        Task<string> LoginAsync(LoginUserDto loginDto);


        //profil ile ilgili detaylar
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        
    }
}