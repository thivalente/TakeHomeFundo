using FundoTakeHome.Tests.Architecture.Support;

namespace FundoTakeHome.Tests.Architecture;

public sealed class ApplicationArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnOuterLayers_WhenApplicationTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InFeatureLayer("Application"), ArchitecturePolicies.ApplicationForbiddenLayers);
        ArchitectureAssertions.ShouldSatisfyRule(result, "Application must not depend on Infrastructure or presentation");
    }

    [Fact]
    public void ShouldNotDependOnWebOrPersistenceFrameworks_WhenApplicationTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InFeatureLayer("Application"), ArchitecturePolicies.ApplicationForbiddenFrameworks);
        ArchitectureAssertions.ShouldSatisfyRule(result, "Application must not depend on ASP.NET Core or Entity Framework Core");
    }
}
