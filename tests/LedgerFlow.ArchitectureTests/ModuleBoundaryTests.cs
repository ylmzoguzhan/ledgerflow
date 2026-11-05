using LedgerFlow.Modules.Ledger;
using LedgerFlow.Modules.Payments;
using NetArchTest.Rules;
namespace LedgerFlow.ArchitectureTests;

public class ModuleBoundaryTests
{
    [Theory]
    [InlineData(typeof(PaymentsModule))]
    [InlineData(typeof(LedgerModule))]
    public void Modul_IcTipleri_Public_Olmamali(Type moduleMarkerType)
    {
        var result = Types.InAssembly(moduleMarkerType.Assembly)
            .That()
            .DoNotHaveName(moduleMarkerType.Name)
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Mimari ihlal! Bağımlılık tespit edilen sınıflar: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Theory]
    [InlineData(typeof(PaymentsModule))]
    [InlineData(typeof(LedgerModule))]
    public void Moduller_DigerAnaModullere_DogrudanReferansVeremez(Type moduleMarkerType)
    {
        var moduleAssembly = moduleMarkerType.Assembly;

        var yasakliReferanslar = moduleAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name.StartsWith("LedgerFlow.Modules.", StringComparison.Ordinal)
                        && !name.EndsWith(".Contracts", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(yasakliReferanslar);
    }
}
