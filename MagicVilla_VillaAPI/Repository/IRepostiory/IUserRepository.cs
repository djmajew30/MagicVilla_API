using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Models;

namespace MagicVilla_VillaAPI.Repository.IRepostiory
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);

        //added 123 register identity net
        //Task<LocalUser> Register(RegistrationRequestDTO registerationRequestDTO);

        Task<UserDTO> Register(RegistrationRequestDTO registerationRequestDTO);

    }
}
