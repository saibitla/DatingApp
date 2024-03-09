using API.Data;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace API.Controllers;

[Authorize]
public class UsersController : BaseAPIController
{
   
    private readonly IUserRespository _userRespository;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUserRespository userRespository, IMapper mapper,IPhotoService photoService){
       
        _userRespository = userRespository;
        _mapper = mapper;
        _photoService = photoService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams){

        var currentUser = await _userRespository.GetUserByUserNameAsync(User.GetUserName());
        userParams.currentUserName = currentUser.UserName;

        if(string.IsNullOrEmpty(userParams.Gender)){
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }

        var users = await _userRespository.GetMembersAsync(userParams);

       // var usersToReturn = _mapper.Map<IEnumerable<MemberDTO>>(users);

       Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,users.PageSize,
       users.TotalCount,users.TotalPages));

        return Ok(users);
    }

    [Authorize(Roles = "Member")]
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username){
       
       return await _userRespository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO MemberUpdateDTO){

       
        var user = await _userRespository.GetUserByUserNameAsync( User.GetUserName());

        if(user == null) return NotFound();

        _mapper.Map(MemberUpdateDTO,user);

        if(await _userRespository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user");

    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file){

        var user = await _userRespository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if(user.photos.Count ==0 ){
            photo.IsMain = true;
        }

        user.photos.Add(photo);

        if(await _userRespository.SaveAllAsync()) 
        {
            return CreatedAtAction(nameof(GetUser), new {userName = user.UserName}, 
            _mapper.Map<PhotoDTO>(photo));
        }

        return BadRequest("problem adding photo");

    }

    [HttpPut("Set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId){
        var user = await _userRespository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return NotFound();

        var photo = user.photos.FirstOrDefault(x => x.Id == photoId);

        if(photo == null) return NotFound();

        if(photo.IsMain) return BadRequest("This is already your main photo");

        var currMain = user.photos.FirstOrDefault(x => x.IsMain);

        if(currMain != null) currMain.IsMain = false;

        photo.IsMain = true;

        if(await _userRespository.SaveAllAsync()) return NoContent();

        return BadRequest("problem setting main photo");

    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId){
        var user = await _userRespository.GetUserByUserNameAsync(User.GetUserName());

        if(user == null) return NotFound();

        var photo = user.photos.FirstOrDefault(x => x.Id == photoId);

         if(photo == null) return NotFound();

         if(photo.IsMain) return BadRequest("you cannot delete main photo");

         if(photo.PublicId != null){
            var result = await _photoService.DeletePhotoAsynv(photo.PublicId);

            if(result.Error != null) return BadRequest(result.Error.Message);
         }

         user.photos.Remove(photo);

         if(await _userRespository.SaveAllAsync()) return Ok();

         return BadRequest("problem deleting photo");

    }

}
