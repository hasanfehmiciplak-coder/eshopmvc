using EShopMVC.Modules.Orders.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class InvoiceDocument : IDocument
{
    private readonly Order _order;
    private readonly string _userEmail;
    private readonly string _userName;

    public InvoiceDocument(Order order, string userEmail, string userName)
    {
        _order = order;
        _userEmail = userEmail;
        _userName = userName;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            page.Header().Text("E-SHOP FATURA")
                .FontSize(20)
                .Bold()
                .AlignCenter();

            page.Content().Column(col =>
            {
                col.Spacing(10);

                col.Item().Text($"Sipariş No: #{_order.Id}");
                col.Item().Text($"Tarih: {_order.OrderDate:dd.MM.yyyy}");
                col.Item().Text($"Müşteri: {_userName}"); // ✔ FIX
                col.Item().Text($"Email: {_userEmail}"); // ✔ FIX

                col.Item().LineHorizontal(1);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Ürün").Bold();
                        header.Cell().Text("Adet").Bold();
                        header.Cell().Text("Fiyat").Bold();
                        header.Cell().Text("Toplam").Bold();
                    });

                    foreach (var item in _order.Items)
                    {
                        table.Cell().Text(item.ProductId.ToString()); // ✔ FIX
                        table.Cell().Text(item.Quantity.ToString());
                        table.Cell().Text($"{item.Price} ₺");
                        table.Cell().Text($"{item.Quantity * item.Price} ₺");
                    }
                });

                col.Item().AlignRight()
                    .Text($"Toplam: {_order.TotalPrice} ₺")
                    .FontSize(14)
                    .Bold();
            });

            page.Footer()
                .AlignCenter()
                .Text("E-Shop © 2025");
        });
    }

    public byte[] GeneratePdf()
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Text("E-Shop Fatura")
                    .FontSize(20)
                    .Bold()
                    .AlignCenter();

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text($"Sipariş No: {_order.Id}");
                    col.Item().Text($"Tarih: {_order.OrderDate:dd.MM.yyyy}");
                    col.Item().Text($"Müşteri: {_userName}");
                    col.Item().Text($"Email: {_userEmail}");

                    col.Item().LineHorizontal(1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Ürün").Bold();
                            header.Cell().Text("Adet").Bold();
                            header.Cell().Text("Fiyat").Bold();
                            header.Cell().Text("Toplam").Bold();
                        });

                        foreach (var item in _order.Items)
                        {
                            table.Cell().Text(item.ProductId.ToString());
                            table.Cell().Text(item.Quantity.ToString());
                            table.Cell().Text($"{item.Price} ₺");
                            table.Cell().Text($"{item.Price * item.Quantity} ₺");
                        }
                    });

                    col.Item().AlignRight()
                        .Text($"Toplam: {_order.TotalPrice} ₺")
                        .FontSize(14)
                        .Bold();
                });

                page.Footer().AlignCenter().Text("E-Shop ©");
            });
        }).GeneratePdf();
    }
}