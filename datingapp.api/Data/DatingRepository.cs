using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recientId)
        {
            return await _context.Likers.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<User> GetUser(int id)
        {
            return await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            IQueryable<User> users = _context.Users.Include(p => p.Photos)
                .OrderByDescending(u => u.LastActive)
                .AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge - 1);

                users = users.Where(u => u.DateOfBirth >= minDob & u.DateOfBirth <= maxDob);
            }

            if (userParams.Likers)
            {
                var userLikers = await GetLikers(userParams.UserId);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            else if (userParams.Likees)
            {
                var userLikees = await GetLikees(userParams.UserId);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync<PagedList<User>>(users, userParams.PageNumber, userParams.PageSize);
        }
  
        private async Task<IEnumerable<int>> GetLikers(int userId)
        {
            return await _context.Likers.Where(l => l.LikeeId == userId).Select(l => l.LikerId).ToListAsync();
        }

        private async Task<IEnumerable<int>> GetLikees(int userId)
        {
            return await _context.Likers.Where(l => l.LikerId == userId).Select(l => l.LikeeId).ToListAsync();
        }

        public async Task UnLike(int userId, int recipientId)
        {
            var like = _context.Likers.FirstOrDefaultAsync(l => l.LikeeId == userId && l.LikerId == recipientId);

            _context.Remove(like);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveAll()
        {
            return (await _context.SaveChangesAsync() > 0);
        }


    }
}