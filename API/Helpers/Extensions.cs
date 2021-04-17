using System;

namespace API.Helpers
{
    public static class Extensions
    {
        public static int CalculateAge(this DateTime dateTime)
        {
            var age = DateTime.Now.Year - dateTime.Year;
            return age;
        }
    }
}