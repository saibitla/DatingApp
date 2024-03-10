 using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Connection
    {
        public Connection()
        {
            
        }
        public Connection(string connectionId,string userName)
        {
            Username = userName;
            ConnectionId = connectionId;
        }

        public string Username { get; set; }

        public string ConnectionId { get; set; }

    }
}