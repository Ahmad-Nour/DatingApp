namespace API.Helpers
{
    public class UserParams
    {
        private const int maxPageSize = 100;
        private int pageSize = 10;

        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > maxPageSize ? maxPageSize : value; }
        }

        public int UserId { get; set; }
        public string Gender { get; set; }
        public int MaxAge { get; set; } = 78;
        public int MinAge { get; set; } = 18;
        public string OrderBy { get; set; }

    }
}