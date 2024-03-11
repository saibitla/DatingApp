using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class MessagesController : BaseAPIController
{
 
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public MessagesController(IMapper mapper,IUnitOfWork uow)    
    {  
        _mapper = mapper;
        _uow = uow;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var username = User.GetUserName();
        if(username == createMessageDTO.RecipientUsername.ToLower())
        {
            return BadRequest("you cannot send message tp yourself");
        }

        var sender = await _uow.UserRepository.GetUserByUserNameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

        if(recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.Content
        };

        _uow.MessageRepository.AddMessage(message);

        if(await _uow.Complete()) return Ok(_mapper.Map<MessageDTO>(message));

        return BadRequest("Failed to send message");
    }

       [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery]
        MessageParams messageParams)
    {
        messageParams.Username = User.GetUserName();
        var messages = await _uow.MessageRepository.GetMessagesForCurrentUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,messages.PageSize,
                                    messages.TotalCount, messages.TotalPages));

        return messages;
    }



    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id){
        var username = User.GetUserName();
        var message = await _uow.MessageRepository.GetMessage(id);

        if(message.SenderUsername != username && message.RecipientUsername != username){
            return Unauthorized();
        }

        if(message.SenderUsername == username) message.SenderDeleted = true;
        if(message.RecipientUsername == username) message.RecipientDeleted = true;

        if(message.SenderDeleted && message.RecipientDeleted) 
        {
            _uow.MessageRepository.DeleteMessage(message);
        }

        if(await _uow.Complete()) return Ok();

        return BadRequest("problem deleting the message");

    }

}