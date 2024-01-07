using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers{
public class BuggyController : BaseAPIController 
{
    private readonly DataContext _context;

    public BuggyController(DataContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("Auth")]
    public ActionResult<string> GetSecret(){
        return "Secret Text";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound(){
        
        var thing = _context.Users.Find(-1);

        if(thing == null) return NotFound();

        return thing;

    }

     [HttpGet("server-error")]
    public ActionResult<string> GetServerError(){

        var thing = _context.Users.Find(-1);

        var thingToReturn = thing.ToString();

        return thingToReturn;
    }

     [HttpGet("bad-request")]
    public ActionResult<string> BadRequest(){
        return BadRequest("this was not a good request");
    }

  



}
}