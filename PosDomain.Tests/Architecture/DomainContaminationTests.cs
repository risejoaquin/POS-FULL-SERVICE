using System.Reflection;
using Xunit;

namespace PosDomain.Tests.Architecture;

public class DomainContaminationTests
{
    private static readonly Assembly DomainAssembly = typeof(PosDomain.Result).Assembly;

    [Fact]
    public void PosDomain_Should_NotReferenceInfrastructurePresentationOrServerAssemblies()
    {
        var forbiddenAssemblyPrefixes = new[]
        {
            "PosInfrastructure",
            "PosCore",
            "PosServer",
            "Microsoft.EntityFrameworkCore",
            "PresentationFramework",
            "PresentationCore",
            "WindowsBase"
        };

        var referencedAssemblies = DomainAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var forbiddenPrefix in forbiddenAssemblyPrefixes)
        {
            Assert.DoesNotContain(referencedAssemblies, name => name.StartsWith(forbiddenPrefix, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void PosDomain_PublicTypes_Should_NotExposeInfrastructurePresentationOrServerNamespaces()
    {
        var forbiddenNamespacePrefixes = new[]
        {
            "PosInfrastructure",
            "PosCore",
            "PosServer",
            "Microsoft.EntityFrameworkCore",
            "System.Windows",
            "Microsoft.AspNetCore"
        };

        var publicTypes = DomainAssembly
            .GetExportedTypes()
            .Where(t => t.Namespace?.StartsWith("PosDomain", StringComparison.Ordinal) == true)
            .ToArray();

        foreach (var type in publicTypes)
        {
            Assert.DoesNotContain(forbiddenNamespacePrefixes, prefix =>
                (type.Namespace ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                Assert.DoesNotContain(forbiddenNamespacePrefixes, prefix =>
                    (property.PropertyType.Namespace ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                         .Where(m => !m.IsSpecialName))
            {
                Assert.DoesNotContain(forbiddenNamespacePrefixes, prefix =>
                    (method.ReturnType.Namespace ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));

                foreach (var parameter in method.GetParameters())
                {
                    Assert.DoesNotContain(forbiddenNamespacePrefixes, prefix =>
                        (parameter.ParameterType.Namespace ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));
                }
            }
        }
    }

    [Fact]
    public void PosDomain_Should_NotContainPlaceholderClass1Type()
    {
        var placeholderType = DomainAssembly.GetType("PosDomain.Class1", throwOnError: false);

        Assert.Null(placeholderType);
    }
}
