using DocumentFormat.OpenXml.InkML;
using EShopMVC.Domain.Base;
using EShopMVC.Modules.Catalog.Models;
using EShopMVC.Shared.Domain;
using Iyzipay.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int ProductId { get; private set; }

        public int Quantity { get; private set; }

        public decimal Price { get; private set; }

        public int OrderId { get; private set; }
        public Order Order { get; private set; }

        public string ProductName { get; set; }
        public int? VariantId { get; private set; }

        public int RefundedQuantity { get; private set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public OrderItem(int productId, int quantity, decimal price)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }

        private OrderItem()
        {
        }

        public void IncreaseRefund(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Invalid refund quantity");

            if (RefundedQuantity + quantity > Quantity)
                throw new InvalidOperationException("Refund exceeds quantity");

            RefundedQuantity += quantity;
        }

        public OrderItem(int productId, int quantity, decimal price, int? variantId = null)
        {
            ProductId = productId;
            Quantity = quantity;
            Price = price;
            VariantId = variantId;
        }
    }
}