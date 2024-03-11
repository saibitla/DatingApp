using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseAPIController
{
    
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService,IMapper mapper){
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){
    
        if(await UserExists(registerDTO.UserName)) return BadRequest("User already exist");

        var user = _mapper.Map<AppUser>(registerDTO);



   
        user.UserName = registerDTO.UserName.ToLower();
   
  

        var result = await _userManager.CreateAsync(user, registerDTO.Password);
        if(!result.Succeeded) BadRequest(result.Errors); 

        var roleResult = await _userManager.AddToRoleAsync(user,"Member");

        if(!roleResult.Succeeded) return BadRequest(result.Errors);
    return new UserDTO
    {
        userName = user.UserName,
        Token = await _tokenService.CreateToken(user),
        photoUrl = user.photos.FirstOrDefault(x => x.IsMain)?.Url,
        knownAs = user.KnownAs,
        Gender = user.Gender
    };

    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){

        var user = await _userManager.Users
        .Include(p => p.photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName);

        if(user == null) return Unauthorized("invalid username");

        var result = await _userManager.CheckPasswordAsync(user,loginDTO.Password);

        if(!result) return Unauthorized("Invalid Password");

        return new UserDTO
                    {
                        userName = user.UserName,
                        Token = await _tokenService.CreateToken(user),
                        photoUrl = user.photos.FirstOrDefault(x => x.IsMain)?.Url,
                        knownAs = user.KnownAs,
                        Gender = user.Gender
                    };



    }

    private async Task<bool> UserExists(string userName){
        return await _userManager.Users.AnyAsync(x => x.UserName == userName.ToLower());
    }

}
