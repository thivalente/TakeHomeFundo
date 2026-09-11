using FundoTakeHome.Tests.Architecture.Support;

namespace FundoTakeHome.Tests.Architecture;

public sealed class BackgroundServicesArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnInnerLayers_WhenBackgroundServiceTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InNamespace("FundoTakeHome.Api.BackgroundServices"), ArchitecturePolicies.BackgroundServicesForbiddenLayers);
        ArchitectureAssertions.ShouldSatisfyRule(result, "BackgroundServices must depend on Application contracts, not inner layers");
    }
}
