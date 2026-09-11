namespace FundoTakeHome.Tests.Architecture.Support;

internal static class ArchitecturePolicies
{
    internal static string[] ApiForbiddenLayers => [..ArchitectureTypes.FeatureLayerDependencies("Domain", "Infrastructure"), "FundoTakeHome.Api.Infrastructure", "FundoTakeHome.Api.BackgroundServices"];

    internal static string[] ApplicationForbiddenLayers => [..ArchitectureTypes.FeatureLayerDependencies("Infrastructure"), "FundoTakeHome.Api.Infrastructure", "FundoTakeHome.Api.Endpoints", "FundoTakeHome.Api.BackgroundServices", "FundoTakeHome.Api.Common.Middlewares"];

    internal static string[] ApplicationForbiddenFrameworks => ["Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore"];

    internal static string[] DomainForbiddenLayers => [..ArchitectureTypes.FeatureLayerDependencies("Application", "Infrastructure"), "FundoTakeHome.Api.Infrastructure", "FundoTakeHome.Api.Endpoints", "FundoTakeHome.Api.BackgroundServices", "FundoTakeHome.Api.Common.Middlewares"];

    internal static string[] DomainForbiddenFrameworks => ["Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "FluentValidation"];

    internal static string[] InfrastructureForbiddenLayers => ["FundoTakeHome.Api.Endpoints", "FundoTakeHome.Api.BackgroundServices"];

    internal static string[] BackgroundServicesForbiddenLayers => [..ArchitectureTypes.FeatureLayerDependencies("Domain", "Infrastructure"), "FundoTakeHome.Api.Infrastructure", "FundoTakeHome.Api.Endpoints"];

    internal static string[] CommonForbiddenLayers => ["FundoTakeHome.Api.Features", "FundoTakeHome.Api.Infrastructure", "FundoTakeHome.Api.Endpoints", "FundoTakeHome.Api.BackgroundServices"];
}
