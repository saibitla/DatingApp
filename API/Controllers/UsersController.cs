using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers;

[Authorize]
public class UsersController : BaseAPIController
{
   
    private readonly IUserRespository _userRespository;
    private readonly IMapper _mapper;

    public UsersController(IUserRespository userRespository, IMapper mapper){
       
        _userRespository = userRespository;
        _mapper = mapper;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers(){

        var users = await _userRespository.GetMembersAsync();

       // var usersToReturn = _mapper.Map<IEnumerable<MemberDTO>>(users);

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username){
       
       return await _userRespository.GetMemberAsync(username);
    }
    
}
