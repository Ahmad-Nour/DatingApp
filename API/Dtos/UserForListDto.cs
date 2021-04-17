using System;

namespace API.Dtos
{
    public class UserForListDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string KnownAs { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int Age { get; set; }
        public DateTime CreatedAccount { get; set; }
        public DateTime LastActive { get; set; }
        public string PhotosUrl { get; set; }
    }
}