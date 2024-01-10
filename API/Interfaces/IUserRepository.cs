using API.Entities;
using API.DTO;

namespace API.Interfaces{
    public interface IUserRespository{
        void Update(AppUser user);

        Task<bool> SaveAllAsync();

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUserNameAsync(string name);

        Task<IEnumerable<MemberDTO>> GetMembersAsync();

        Task<MemberDTO> GetMemberAsync(string username);

        
    }
}