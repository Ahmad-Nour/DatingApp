using System.Collections.Generic;
using System.IO;
using System.Linq;
using API.Models;
using Newtonsoft.Json;

namespace API.Data
{
    public class SeedData
    {
        static string pathSeedUser = "Data/UserSeedData.json";
        public static void SeedUsers(DataContext context)
        {
            if (!context.Users.Any())
            {
                if (File.Exists(pathSeedUser))
                {
                    var userData = File.ReadAllText(pathSeedUser);
                    var users = JsonConvert.DeserializeObject<List<User>>(userData);
                    foreach (var user in users)
                    {
                        byte[] passwordHash, passwordSalt;
                        CreatePasswordHash("password", out passwordHash, out passwordSalt);
                        user.Username = user.Username.ToLower();
                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = passwordSalt;
                        context.Add<User>(user);
                    }
                    context.SaveChanges();
                }
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}