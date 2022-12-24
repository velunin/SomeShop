using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SomeShop.Common.Domain;

namespace SomeShop.Common.EF;

public static class ModelBuilderExtensions
{
    public static void UsePropertyConfigurators(
        this ModelBuilder modelBuilder,
        params IPropertyConfigurator[] configurators)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                foreach (var propertyConfigurator in configurators)
                {
                    if (propertyConfigurator.IsSatisfiedBy(property))
                    {
                        propertyConfigurator.Configure(property);
                    }
                }
            }
        }
    }
}

public interface IPropertyConfigurator
{
    bool IsSatisfiedBy(IMutableProperty property);
    void Configure(IMutableProperty property);
}

public class StringEnumConverterConfigurator : IPropertyConfigurator
{
    public bool IsSatisfiedBy(IMutableProperty property)
    {
        return property.ClrType.BaseType == typeof(Enum);
    }

    public void Configure(IMutableProperty property)
    {
        var type = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
        var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;

        property.SetValueConverter(converter);
    }
}

public class RowVersionConfigurator : IPropertyConfigurator
{
    public bool IsSatisfiedBy(IMutableProperty property)
    {
        return property.ClrType == typeof(uint) && property.Name == "RowVersion";
    }

    public void Configure(IMutableProperty property)
    {
        property.SetColumnName("xmin");
        property.SetColumnType("xid");
        property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
        property.IsConcurrencyToken = true;
    }
}

public class CurrencyEnumColumnTypeConfigurator : IPropertyConfigurator
{
    public bool IsSatisfiedBy(IMutableProperty property)
    {
        return property.ClrType == typeof(Currency);
    }

    public void Configure(IMutableProperty property)
    {
        property.SetColumnType("varchar(3)");
    }
}


public class CreatedDateConfigurator : IPropertyConfigurator
{
    public bool IsSatisfiedBy(IMutableProperty property)
    {
        return property.ClrType == typeof(DateTimeOffset) && property.Name == "CreatedAt";
    }

    public void Configure(IMutableProperty property)
    {
        property.SetDefaultValueSql("CURRENT_TIMESTAMP");
        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    }
}
