using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRespository _userRespository;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(IMessageRepository messageRepository,IUserRespository userRespository,
                            IMapper mapper,IHubContext<PresenceHub> presenceHub)
        {
            _messageRepository = messageRepository;
            _userRespository = userRespository;
            _mapper = mapper;
            _presenceHub = presenceHub;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var otherUser = httpContext.Request.Query["user"];

            var groupName = GetGroupName(Context.User.GetUserName(),otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepository.GetMessageThread(Context.User.GetUserName(),otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
           var group =  await RemoveFromMessageGroup();
           await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller, string other){
            var stringCompare = string.CompareOrdinal(caller,other) < 0;

            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var username = Context.User.GetUserName();
            if(username == createMessageDTO.RecipientUsername.ToLower())
            {
                throw new HubException("you cannot send mesages to yourself");
            }

            var sender = await _userRespository.GetUserByUserNameAsync(username);
            var recipient = await _userRespository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if(recipient == null) throw new HubException("not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _messageRepository.GetMessageGroup(groupName);

            if(group.connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                                new {username = sender.UserName, knownAs = sender.KnownAs});
                }
            }

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) 
            {
                
                await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDTO>(message));
            }
            
            }

            private async Task<Group> AddToGroup(string groupName)
            {
                var group = await _messageRepository.GetMessageGroup(groupName);
                var conneciton = new Connection(Context.ConnectionId,Context.User.GetUserName());

                if(group == null){
                    group = new Group();
                    _messageRepository.AddGroup(group);

                }

                group.connections.Add(conneciton);
                if(await _messageRepository.SaveAllAsync()) return group;

                throw new HubException("Failed to add to group");  
            }

            private async Task<Group> RemoveFromMessageGroup(){
                var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
                var connection = group.connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                _messageRepository.RemoveConnection(connection);
                if(await _messageRepository.SaveAllAsync()) return group;
                throw new HubException("Failed to remove from group");                
            }
    }
}