using NetArchTest.Rules;

namespace FundoTakeHome.Tests.Architecture.Support;

internal static class ArchitectureTypes
{
    internal static PredicateList InFeatureLayer(string layerName)
    {
        return InPrefixes(GetFeatureLayerPrefixes(layerName));
    }

    internal static PredicateList InInfrastructure()
    {
        return InPrefixes(["FundoTakeHome.Api.Infrastructure", ..GetFeatureLayerPrefixes("Infrastructure")]);
    }

    internal static PredicateList InNamespace(string namespaceName)
    {
        return InPrefixes([namespaceName]);
    }

    internal static string[] FeatureLayerDependencies(params string[] layerNames)
    {
        return layerNames.SelectMany(GetFeatureLayerPrefixes).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static PredicateList InPrefixes(IEnumerable<string> namespacePrefixes)
    {
        var prefixes = namespacePrefixes.Distinct(StringComparer.Ordinal).ToArray();
        var types = Types.InAssembly(ArchitectureAssembly.Api).That().ResideInNamespaceStartingWith(prefixes[0]);

        foreach (var prefix in prefixes.Skip(1))
        {
            types = types.Or().ResideInNamespaceStartingWith(prefix);
        }

        return types;
    }

    private static string[] GetFeatureLayerPrefixes(string layerName)
    {
        return ArchitectureAssembly.Api.GetTypes()
            .Select(type => type.Namespace)
            .Where(namespaceName => namespaceName?.StartsWith("FundoTakeHome.Api.Features.", StringComparison.Ordinal) == true)
            .Select(namespaceName => namespaceName!.Split('.'))
            .Where(parts => parts.Length >= 5 && string.Equals(parts[4], layerName, StringComparison.Ordinal))
            .Select(parts => string.Join('.', parts.Take(5)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
