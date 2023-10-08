using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace SomeShop.ArchTests;

public class BoundariesTests
{
    private static readonly string[] BoundedContexts = {
        "Ordering",
        "Catalog",
        "StockManagement",
    };
    
    [Test]
    public void DomainLayerShouldDependOnlyOnCommonDomain()
    {
        Assert.Multiple(() =>
        {
            foreach (var bc in BoundedContexts)
            {
                var domainLayer = Types().That().ResideInAssembly($"SomeShop.{bc}.Domain", true)
                    .As($"Domain layer of {bc}");

                var allowedTypes = Types().That()
                    .Are(domainLayer)
                    .Or()
                    .Are(CommonDomainTypes);

                Types().That()
                    .Are(domainLayer).Should().OnlyDependOn(allowedTypes).Check(Architecture);
            }
        });
    }

    [Test]
    public void ContextShouldOnlyDependOnSameContextOrContractsAnotherContextOrCommon()
    {
        Assert.Multiple(() =>
        {
            foreach (var bc in BoundedContexts)
            {
                var currentContextTypes = Types().That().ResideInAssembly($"SomeShop.{bc}.", true)
                    .As($"types of '{bc}' context");
                
                var allowedTypes = Types().That()
                    .Are(currentContextTypes).Or()
                    .Are(CommonTypes).Or()
                    .Are(ContractsTypes)
                    .As("same context, common or contacts types");

                Types().That()
                    .Are(currentContextTypes).Should().OnlyDependOn(allowedTypes).Check(Architecture);
            }
        });
    }
    
    private static readonly Architecture Architecture = new ArchLoader().LoadAssembliesRecursively(
            new[] {Assembly.Load("SomeShop.Api") }, definition => 
                definition.Name.Name.StartsWith("SomeShop.") 
                    ? FilterResult.LoadAndContinue 
                    : FilterResult.SkipAndContinue).Build();

    private static readonly IObjectProvider<IType> CommonDomainTypes =
        Types().That().ResideInAssembly("^SomeShop.Common.Domain,", true).As("Common domain types");
    
    private static readonly IObjectProvider<IType> ContractsTypes =
        Types().That().ResideInAssembly("^SomeShop.[\\s\\S\\d]+.Contracts,", true).As("Contacts");
    
    private static readonly IObjectProvider<IType> CommonTypes =
        Types().That().ResideInAssembly("^SomeShop.Common.", true).As("Common types");
    
}