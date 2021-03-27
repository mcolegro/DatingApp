using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
   public class UserRepository : IUserRepository
   {
      private readonly DataContext _context;
      private readonly IMapper _mapper;
      public UserRepository(DataContext context, IMapper mapper)
      {
         _mapper = mapper;
         _context = context;
      }

      public async Task<MemberDto> GetMemberAsync(string username)
      {
         return await _context.Users
                              .Where(o => o.UserName == username)
                              .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                              .SingleOrDefaultAsync();
      }

      public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
      {
         var query = _context.Users.AsQueryable();

         query = query.Where(o => o.UserName != userParams.CurrentUsername);
         query = query.Where(o => o.Gender == userParams.Gender);

         var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
         var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

         query = query.Where(o => o.DateOfBirth >= minDob && o.DateOfBirth <= maxDob);

         query = userParams.OrderBy switch
         {
            "created" => query.OrderByDescending(o => o.Created),
            _ => query.OrderByDescending(o => o.LastActive)
         };

         return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
      }

      public async Task<AppUser> GetUserByIdAsync(int id)
      {
         return await _context.Users.FindAsync(id);
      }

      public async Task<AppUser> GetUserByUsernameAsync(string username)
      {
         return await _context.Users
                              .Include(o => o.Photos)
                              .SingleOrDefaultAsync(o => o.UserName == username);
      }

      public async Task<IEnumerable<AppUser>> GetUsersAsync()
      {
         return await _context.Users
                              .Include(o => o.Photos)
                              .ToListAsync();
      }

      public async Task<bool> SaveAllAsync()
      {
         return await _context.SaveChangesAsync() > 0;
      }

      public void Update(AppUser user)
      {
         _context.Entry(user).State = EntityState.Modified;
      }
   }
}