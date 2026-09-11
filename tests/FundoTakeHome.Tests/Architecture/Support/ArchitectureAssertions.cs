using NetArchTest.Rules;
using Shouldly;

namespace FundoTakeHome.Tests.Architecture.Support;

internal static class ArchitectureAssertions
{
    internal static TestResult ShouldNotDependOnAny(PredicateList types, params string[] forbiddenDependencies)
    {
        return types.ShouldNot().HaveDependencyOnAny(forbiddenDependencies).GetResult();
    }

    internal static void ShouldSatisfyRule(TestResult result, string ruleName)
    {
        var failingTypes = string.Join(Environment.NewLine, result.FailingTypes?.Select(type => $"- {type.FullName}") ?? Enumerable.Empty<string>());
        result.IsSuccessful.ShouldBeTrue($"Architecture rule '{ruleName}' failed.{Environment.NewLine}{failingTypes}");
    }
}
