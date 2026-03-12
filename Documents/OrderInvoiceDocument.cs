using EShopMVC.Modules.Orders.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class OrderInvoiceDocument : IDocument
{
    private readonly Order _order;

    public OrderInvoiceDocument(Order order)
    {
        _order = order;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            page.Header().Text("E-TİCARET FATURA")
                .FontSize(20)
                .Bold()
                .AlignCenter();

            page.Content().Column(col =>
            {
                col.Spacing(10);

                col.Item().Text($"Sipariş No: {_order.Id}");
                col.Item().Text($"Tarih: {_order.CreatedDate:dd.MM.yyyy}");
                col.Item().Text($"Müşteri: {_order.User.FullName}");
                col.Item().Text($"Email: {_order.User.Email}");

                col.Item().LineHorizontal(1);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.ConstantColumn(60);
                        c.ConstantColumn(80);
                        c.ConstantColumn(80);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Ürün").Bold();
                        h.Cell().Text("Adet").Bold();
                        h.Cell().Text("Fiyat").Bold();
                        h.Cell().Text("Toplam").Bold();
                    });

                    foreach (var item in _order.OrderItems)
                    {
                        table.Cell().Text(item.Product.Name);
                        table.Cell().Text(item.Quantity.ToString());
                        table.Cell().Text($"{item.Price} ₺");
                        table.Cell().Text($"{item.Quantity * item.Price} ₺");
                    }
                });

                col.Item().LineHorizontal(1);

                col.Item().AlignRight().Text($"TOPLAM: {_order.TotalPrice} ₺")
                    .Bold()
                    .FontSize(14);
            });

            page.Footer()
                .AlignCenter()
                .Text("Teşekkür ederiz 💙");
        });
    }
}