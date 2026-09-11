using FundoTakeHome.Tests.Architecture.Support;

namespace FundoTakeHome.Tests.Architecture;

public sealed class CommonArchitectureTests
{
    [Fact]
    public void ShouldNotDependOnFeatureOrOuterLayers_WhenCommonTypesAreAnalyzed()
    {
        var result = ArchitectureAssertions.ShouldNotDependOnAny(ArchitectureTypes.InNamespace("FundoTakeHome.Api.Common"), ArchitecturePolicies.CommonForbiddenLayers);
        ArchitectureAssertions.ShouldSatisfyRule(result, "Common must not depend on feature or outer layers");
    }
}
