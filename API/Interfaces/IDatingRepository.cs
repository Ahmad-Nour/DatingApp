using System.Collections.Generic;
using System.Threading.Tasks;
using API.Models;

namespace API.Interfaces
{
    public interface IDatingRepository
    {
        public Task Add<T>(T entity) where T : class;
        public void Delete<T>(T entity) where T : class;
        public Task<bool> SaveAll();
        public Task<IReadOnlyList<User>> GetUsers();
        public Task<User> GetUser(int id);
        public Task<Photo> GetPhoto(int id);
        public Task<Photo> GetMainPhotoForUser(int userId);

    }
}