using FundoTakeHome.Tests.Architecture.Support;

namespace FundoTakeHome.Tests.Architecture;

public sealed class ApiArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnInnerLayers_WhenEndpointTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InNamespace("FundoTakeHome.Api.Endpoints"), ArchitecturePolicies.ApiForbiddenLayers);
        ArchitectureAssertions.ShouldSatisfyRule(result, "API endpoints must not depend on inner layers");
    }
}
