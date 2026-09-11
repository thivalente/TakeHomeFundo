using FundoTakeHome.Tests.Architecture.Support;

namespace FundoTakeHome.Tests.Architecture;

public sealed class DomainArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnOuterLayers_WhenDomainTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InFeatureLayer("Domain"), ArchitecturePolicies.DomainForbiddenLayers);
        ArchitectureAssertions.ShouldSatisfyRule(result, "Domain must not depend on Application, Infrastructure, or presentation");
    }

    [Fact]
    public void ShouldNotDependOnWebOrPersistenceFrameworks_WhenDomainTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InFeatureLayer("Domain"), ArchitecturePolicies.DomainForbiddenFrameworks);
        ArchitectureAssertions.ShouldSatisfyRule(result, "Domain must not depend on web, persistence, or validation frameworks");
    }
}
