using System.Collections.Generic;
using System.Threading.Tasks;
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
            var photo = await _context.Photos.FirstOrDefaultAsync(p =>p.UserId ==userId && p.IsMain);
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

        public async Task<IReadOnlyList<User>> GetUsers()
        {
            var users = await _context.Users.Include(p => p.Photos).ToListAsync();
            if (users == null) return null;
            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}