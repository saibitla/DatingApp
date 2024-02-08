using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseAPIController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService,IMapper mapper){

    _context = context;
    _tokenService = tokenService;
    _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){
    
        if(await UserExists(registerDTO.UserName)) return BadRequest("User already exist");

        var user = _mapper.Map<AppUser>(registerDTO);

    using var hmac = new HMACSHA512();

   
        user.UserName = registerDTO.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
        user.PasswordSalt = hmac.Key;
  

    _context.Add(user);
    await _context.SaveChangesAsync();

    return new UserDTO
    {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        photoUrl = user.photos.FirstOrDefault(x => x.IsMain)?.Url,
        knownAs = user.KnownAs
    };

    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){

        var user = await _context.Users
        .Include(p => p.photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName);

        if(user == null) return Unauthorized("invalid username");
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for(int i = 0; i < computedPassword.Length; i++){
            if (computedPassword[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
        }

        return new UserDTO
                    {
                        Username = user.UserName,
                        Token = _tokenService.CreateToken(user),
                        photoUrl = user.photos.FirstOrDefault(x => x.IsMain)?.Url,
                        knownAs = user.KnownAs
                    };



    }

    private async Task<bool> UserExists(string userName){
        return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
    }

}
