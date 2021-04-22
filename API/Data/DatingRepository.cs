using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public async Task Add<T>(T entity) where T : class
        {
            await _context.AddAsync(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }


        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
            if (photo == null) return null;
            return photo;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(x => x.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return null;
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {

            var users = _context.Users.Include(p => p.Photos)
                .OrderByDescending(x => x.LastActive).AsQueryable();

            users = users.Where(x => x.Gender == userParams.Gender);
            users = users.Where(x => x.Id != userParams.UserId);

            if (userParams.Likers)
            {
                var userLiker = await GetLikeForFilter(userParams.Likers, userParams.UserId);
                users = users.Where(u => userLiker.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikee = await GetLikeForFilter(userParams.Likers, userParams.UserId);
                users = users.Where(u => userLikee.Contains(u.Id));
            }

            if (userParams.MaxAge != 78 || userParams.MinAge != 18)
            {
                // get max and min DateOfBirth for filter user..
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(dob => dob.DateOfBirth >= minDob && dob.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(x => x.CreatedAccount);
                        break;
                    default:
                        users = users.OrderByDescending(x => x.LastActive);
                        break;
                }
            }


            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }
        public async Task<Like> GetLike(int id, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(like => like.LikerId == id &&
                like.LikeeId == recipientId);
        }


        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<List<int>> GetLikeForFilter(bool liker, int id)
        {
            var user = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);


            List<int> data = new List<int>();
            if (liker)
            {
                foreach (var item in user.Likers)
                {
                    if (item.LikeeId == id)
                        data.Add(item.LikerId);
                }
                return data;
            }
            else
            {
                foreach (var item in user.Likers)
                {
                    if (item.LikerId == id)
                        data.Add(item.LikeeId);
                }
                return data;
            }
        }



    }
}