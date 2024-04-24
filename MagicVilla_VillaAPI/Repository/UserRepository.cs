using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private string secretKey;
        //next 2 prop new in 122. Login identity
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration
            //122. Login Identity NET
            , UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            //122. Login Identity NET
            _mapper = mapper;
            _userManager = userManager;
        }

        public bool IsUniqueUser(string username)
        {
            //var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == username); //removed 122
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == username);

            if (user == null) //user not found
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            ////removed 122. Login Identity net
            //var user = _db.LocalUsers.FirstOrDefault(
            //    u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower()
            //    && u.Password == loginRequestDTO.Password);

            var user = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            //added 122.
            //check if pw valid
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            //if (user == null) //not a valid login, not found
            if (user == null || isValid == false) // || isValid == false added 122.
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null
                };
            }

            //added 122
            var roles = await _userManager.GetRolesAsync(user); //role now in new table

            //if user was found generate JWT Token. need secret key token will be encrypted
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey); //converts key to bytes 

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    //new Claim(ClaimTypes.Role, user.Role) 
                    //added 122
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()) 

                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                Token = tokenHandler.WriteToken(token), //WriteToken serializes
                //added 122
                //User = user
                User = _mapper.Map<UserDTO>(user),
                Role = roles.FirstOrDefault(),
            };
            return loginResponseDTO;
        }

        public async Task<LocalUser> Register(RegistrationRequestDTO registerationRequestDTO)
        {
            //add new user
            LocalUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Password = registerationRequestDTO.Password,
                Name = registerationRequestDTO.Name,
                Role = registerationRequestDTO.Role
            };

            _db.LocalUsers.Add(user);
            await _db.SaveChangesAsync();
            //clear password before sending response
            user.Password = "";
            return user;

        }
    }
}
