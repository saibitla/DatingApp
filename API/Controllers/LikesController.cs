using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class LikesController : BaseAPIController
{
    private readonly IUserRespository _userRespository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(IUserRespository userRespository, ILikesRepository likesRepository)
    {
        _userRespository = userRespository;
        _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId();
        var likedUser = await _userRespository.GetUserByUserNameAsync(username);
        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

        if(likedUser == null) return NotFound();

        if(sourceUser.UserName == username) return BadRequest("you cannot like yourself");

        var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

        if(userLike != null) return BadRequest("you already like this user");

        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        
        };

        sourceUser.LikedUsers.Add(userLike);

        if(await _userRespository.SaveAllAsync()) return Ok();

        return BadRequest("Failed to like user");
        
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery]LikesParams likesParams){

        likesParams.UserId = User.GetUserId();

        var users = await _likesRepository.GetUserLikes(likesParams );

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,
                users.PageSize, users.TotalCount,users.TotalPages));

        return Ok(users);
    } 

}
