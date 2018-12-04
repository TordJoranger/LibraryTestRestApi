

namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        private const int MaxPageSize = 20;

        public int PageNumber { get; set; } = 1;

        private int _pageSize;

        public int PageSize
        {
            get => _pageSize = 10;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string Genre { get; set; }
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "Name";
    }
}
