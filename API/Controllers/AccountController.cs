using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseAPIController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService){

    _context = context;
    _tokenService = tokenService; 

    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){
    
        if(await UserExists(registerDTO.UserName)) return BadRequest("User already exist");

    using var hmac = new HMACSHA512();

    var user = new AppUser{
        UserName = registerDTO.UserName.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
        PasswordSalt = hmac.Key
    };

    _context.Add(user);
    await _context.SaveChangesAsync();

    return new UserDTO
    {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
    };

    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){

        var user = await _context.Users.SingleOrDefaultAsync(x =>
        x.UserName == loginDTO.UserName);

        if(user == null) return Unauthorized("invalid username");
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for(int i = 0; i < computedPassword.Length; i++){
            if (computedPassword[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
        }

        return new UserDTO
    {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
    };



    }

    private async Task<bool> UserExists(string userName){
        return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
    }

}
