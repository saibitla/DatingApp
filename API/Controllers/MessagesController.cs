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
    private readonly IUserRespository _userRespository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRespository userRespository, 
                    IMessageRepository messageRepository,IMapper mapper)
    
    {
        _userRespository = userRespository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var username = User.GetUserName();
        if(username == createMessageDTO.RecipientUsername.ToLower())
        {
            return BadRequest("you cannot send message tp yourself");
        }

        var sender = await _userRespository.GetUserByUserNameAsync(username);
        var recipient = await _userRespository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

        if(recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.Content
        };

        _messageRepository.AddMessage(message);

        if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));

        return BadRequest("Failed to send message");
    }

       [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery]
        MessageParams messageParams)
    {
        messageParams.Username = User.GetUserName();
        var messages = await _messageRepository.GetMessagesForCurrentUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,messages.PageSize,
                                    messages.TotalCount, messages.TotalPages));

        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
    {
        var currentUserName = User.GetUserName();

        return Ok(await _messageRepository.GetMessageThread(currentUserName, username));

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id){
        var username = User.GetUserName();
        var message = await _messageRepository.GetMessage(id);

        if(message.SenderUsername != username && message.RecipientUsername != username){
            return Unauthorized();
        }

        if(message.SenderUsername == username) message.SenderDeleted = true;
        if(message.RecipientUsername == username) message.RecipientDeleted = true;

        if(message.SenderDeleted && message.RecipientDeleted) 
        {
            _messageRepository.DeleteMessage(message);
        }

        if(await _messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("problem deleting the message");

    }

}