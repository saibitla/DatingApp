using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data{
    public class UserRepository : IUserRespository
    {
        private DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            
            return await _context.Users
                    .Include(p => p.photos)
                    .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string name)
        {
            return await _context.Users
                    .Include(p => p.photos)
                    .SingleOrDefaultAsync(x => x.UserName == name);
        }

  
        public void Update(AppUser user)
        {
           _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var query =  _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.currentUserName);
            query = query.Where(u => u.Gender == userParams.Gender);

             var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
             var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

             query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };


            return await PagedList<MemberDTO>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDTO>(_mapper.ConfigurationProvider),
            userParams.Pagenumber,userParams.PageSize);
        }

        public async Task<MemberDTO> GetMemberAsync(string username)
        {
            return await _context.Users
                    .Where(x => x.UserName == username)
                    .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(x => x.UserName == username)
                        .Select(x => x.Gender).FirstOrDefaultAsync();
        }
    }
}