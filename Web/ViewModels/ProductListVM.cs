using EShopMVC.Models;

namespace EShopMVC.Web.ViewModels
{
    public class ProductListVM
    {
        //public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
        public int CurrentPage { get; set; }

        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }

        // Filtreler
        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStock { get; set; }

        public string? Search { get; set; }   // 🔥

        public int TotalCount { get; set; }

        // Data

        public List<Product> Products { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public List<Category> Categories { get; set; } = new();

        // Sayfalama
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 12;

        public string? Sort { get; set; } // newest | price_asc | price_desc | name

        // Hesaplanan
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}