namespace EShopMVC.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }

        public string Currency { get; }

        public Money(decimal amount, string currency = "TRY")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");

            Amount = amount;
            Currency = currency;
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Currencies must match");

            return new Money(a.Amount + b.Amount, a.Currency);
        }
    }
}