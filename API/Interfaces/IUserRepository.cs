using API.Entities;
using API.DTO;
using API.Helpers;

namespace API.Interfaces{
    public interface IUserRespository{
        void Update(AppUser user);

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUserNameAsync(string name);

        Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);

        Task<MemberDTO> GetMemberAsync(string username);

        Task<string> GetUserGender(string username);

        
    }
}