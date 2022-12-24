// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS8618
namespace SomeShop.Common.App.Configs;

public class PostgresConfig
{
    public const string Section = "Postgres";

    public string Host { get; set; }
    public string Port { get; set; }
    public string Name { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public string ToConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Name};Username={User};Password={Password}";
    }
}
