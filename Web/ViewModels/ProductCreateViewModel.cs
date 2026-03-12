using Microsoft.AspNetCore.Mvc.Rendering;

public class ProductCreateViewModel
{
    public int FirmId { get; set; }

    // Dropdown için SelectListItem koleksiyonu
    public IEnumerable<SelectListItem> Firms { get; set; }

    // Ürünle ilgili diğer alanlar
    public string Name { get; set; }

    public decimal Price { get; set; }
}