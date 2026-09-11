using FundoTakeHome.Api.Endpoints;
using System.Reflection;

namespace FundoTakeHome.Tests.Architecture.Support;

internal static class ArchitectureAssembly
{
    internal static Assembly Api { get; } = typeof(SubmitApplicationEndpoints).Assembly;
}
