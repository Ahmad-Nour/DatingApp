namespace API.Helpers
{
    public class PaginationHeader
    {

        public PaginationHeader(int currentPage, int itemsForPage, int totalItems, int totalPages)
        {
            this.CurrentPage = currentPage;
            this.ItemsForPage = itemsForPage;
            this.TotalItems = totalItems;
            this.TotalPages = totalPages;

        }
        public int CurrentPage { get; set; }
        public int ItemsForPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}