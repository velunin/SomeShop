using SomeShop.Common.Domain;

namespace SomeShop.Common.Proto;

public partial class Money
{
    private const decimal NanoFactor = 1_000_000_000;
    public Money(decimal value, Currency currency)
    {
        var units = decimal.ToInt64(value);
        var nanos = decimal.ToInt32((value - units) * NanoFactor);

        Units = units;
        Nanos = nanos;
        CurrencyCode = currency.ToString("G");
    }

    public Domain.Money ToValueObject()
    {
        return new Domain.Money(Units + Nanos / NanoFactor, CurrencyCode);
    }
}