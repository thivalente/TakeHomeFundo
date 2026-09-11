using FundoTakeHome.Tests.Architecture.Support;

namespace FundoTakeHome.Tests.Architecture;

public sealed class InfrastructureArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnPresentation_WhenInfrastructureTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InInfrastructure(), ArchitecturePolicies.InfrastructureForbiddenLayers);
        ArchitectureAssertions.ShouldSatisfyRule(result, "Infrastructure must not depend on API endpoints or BackgroundServices");
    }
}
