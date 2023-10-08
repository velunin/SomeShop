using SomeShop.Common.Domain.Ids;
using static System.Environment;

namespace SomeShop.IntegrationTests;

public static class Data
{
    private const string ApiUrlEnvVarName = "SOMESHOP_API_URL";
    private const string DefaultApiUrl = "http://localhost:13001";

    public static string ApiUrl => GetEnvironmentVariable(ApiUrlEnvVarName) ?? DefaultApiUrl;

    public static ProductId AmazingProductId = new(Guid.Parse("89572dc1-2fb1-488b-83db-1059c63cef37"));
    public static ProductId GorgeousProductId = new(Guid.Parse("9560242f-eb23-414b-9602-d5248f700b3c"));
    public static ProductId IncredibleProductId = new(Guid.Parse("d18853b6-ae63-4a07-807d-6fcaaa0bbb41"));
}