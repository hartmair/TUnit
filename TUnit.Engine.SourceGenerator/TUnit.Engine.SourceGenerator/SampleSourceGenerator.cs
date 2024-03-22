using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TUnit.Engine.SourceGenerator.Extensions;

namespace TUnit.Engine.SourceGenerator;

/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class SampleSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this generator.
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var sourceBuilder = new StringBuilder("""
                                              // <auto-generated/>
                                              using System.Runtime.CompilerServices;

                                              namespace TUnit.Engine;

                                              file class TestGenerator
                                              {
                                                  [ModuleInitializer]
                                                  public static void Initialise()
                                                  {
                                              
                                              """);
        foreach (var method in context.Compilation
                     .SyntaxTrees
                     .SelectMany(st =>
                         st.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
                             .Select(m => new Method(st, m)))
                     .Where(x => x.MethodDeclarationSyntax.DescendantNodes().OfType<AttributeSyntax>().Any()))
        {
            ProcessTests(sourceBuilder, context, method);
        }

        sourceBuilder.AppendLine("""
                                    }
                                 }
                                 """);

        context.AddSource($"TestInitializer.g.cs", sourceBuilder.ToString());
    }

    private void ProcessTests(StringBuilder sourceBuilder, GeneratorExecutionContext context, Method method)
    {
        var semanticModel = context.Compilation.GetSemanticModel(method.SyntaxTree);

        var symbol = semanticModel.GetDeclaredSymbol(method.MethodDeclarationSyntax)
                     ?? semanticModel.GetSymbolInfo(method.MethodDeclarationSyntax).Symbol;

        if (symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.ContainingType.IsAbstract)
        {
            return;
        }

        var attributes = symbol.GetAttributes();

        var isAsyncMethod = methodSymbol.ReturnType
            .ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
            .StartsWith($"global::{typeof(Task).FullName}");

        var asyncKeyword = isAsyncMethod ? "async " : string.Empty;
        
        var methodAwaitablePrefix = isAsyncMethod ? "await " : string.Empty;
        
        foreach (var attributeData in attributes)
        {
            switch (attributeData.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix))
            {
                case "global::TUnit.Core.TestAttribute":
                    foreach (var classInvocation in GenerateClassInvocations(methodSymbol.ContainingType))
                    {
                        var usingDisposablePrefix = GetDisposableUsingPrefix(methodSymbol.ContainingType);
                        sourceBuilder.AppendLine($$"""
                                                        TestDictionary.AddTest("", {{asyncKeyword}}() => 
                                                        {
                                                            {{usingDisposablePrefix}}var classInstance = {{classInvocation}};
                                                            {{methodAwaitablePrefix}}classInstance.{{GenerateTestMethodInvocation(methodSymbol)}};
                                                        });
                                                 """);
                    }
                    break;
                case "global::TUnit.Core.DataDrivenTestAttribute":
                    break;
                case "global::TUnit.Core.DataSourceDrivenTestAttribute": 
                    break;
                case "global::TUnit.Core.CombinativeTestAttribute": 
                    break;
            }
        }
    }

    private string GetDisposableUsingPrefix(INamedTypeSymbol type)
    {
        if (type.IsAsyncDisposable())
        {
            return "await using ";
        }

        if (type.IsDisposable())
        {
            return "using ";
        }

        return string.Empty;
    }

    private IEnumerable<string> GenerateClassInvocations(INamedTypeSymbol namedTypeSymbol)
    {
        var className =
            namedTypeSymbol.ToDisplayString(DisplayFormats.FullyQualifiedGenericWithGlobalPrefix);

        if (namedTypeSymbol.InstanceConstructors.First().Parameters.IsDefaultOrEmpty)
        {
            yield return $"new {className}()";
        }

        foreach (var methodDataAttribute in namedTypeSymbol.GetAttributes().Where(x =>
                     x.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                         is "global::TUnit.Core.MethodDataAttribute"))
        {
            var args = methodDataAttribute.ConstructorArguments.Length == 1
                ? $"{className}.{methodDataAttribute.ConstructorArguments.First().Value}()"
                : $"{methodDataAttribute.ConstructorArguments[0].Value}.{methodDataAttribute.ConstructorArguments[1].Value}()";
            
            yield return $"new {className}({args})";
        }
        
        foreach (var classDataAttribute in namedTypeSymbol.GetAttributes().Where(x =>
                     x.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                         is "global::TUnit.Core.ClassDataAttribute"))
        {
            yield return $"new {className}(new {classDataAttribute.ConstructorArguments.First().Value}())";
        }
        
        foreach (var classDataAttribute in namedTypeSymbol.GetAttributes().Where(x =>
                     x.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                         is "global::TUnit.Core.InjectAttribute"))
        {
            var genericType = classDataAttribute.AttributeClass!.TypeArguments.First();
            var fullyQualifiedGenericType =
                genericType.ToDisplayString(DisplayFormats.FullyQualifiedGenericWithGlobalPrefix);
            var sharedArgument = classDataAttribute.NamedArguments.First(x => x.Key == "Shared").Value;

            if (sharedArgument.Type?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                is "global::TUnit.Core.None")
            {
                yield return $"new {className}(new {genericType.ToDisplayString(DisplayFormats.FullyQualifiedGenericWithGlobalPrefix)}())";
            }
            
            if (sharedArgument.Type?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                is "global::TUnit.Core.Globally")
            {

                yield return $"""
                              var obj = global::TUnit.Engine.TestDataContainer.InjectedSharedGlobally.GetOrAdd(typeof({fullyQualifiedGenericType}), x => new {fullyQualifiedGenericType}());
                              return new {className}(obj);
                              """;
            }
            
            if (sharedArgument.Type?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                is "global::TUnit.Core.ForClass")
            {

                yield return $"""
                              var obj = global::TUnit.Engine.TestDataContainer.InjectedSharedPerClassType.GetOrAdd(new global::TUnit.Engine.Models.DictionaryTypeTypeKey(typeof({className}), typeof({fullyQualifiedGenericType})), x => new {fullyQualifiedGenericType}());
                              return new {className}(obj);
                              """;
            }
            
            if (sharedArgument.Type?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                is "global::TUnit.Core.ForKey")
            {
                var key = sharedArgument.Value?.GetType().GetProperty("Key")?.GetValue(sharedArgument.Value);
                yield return $"""
                              var obj = global::TUnit.Engine.TestDataContainer.InjectedSharedPerKey.GetOrAdd(new global::TUnit.Engine.Models.DictionaryStringTypeKey("{key}", typeof({fullyQualifiedGenericType})), x => new {fullyQualifiedGenericType}());
                              return new {className}(obj);
                              """;
            }
        }
    }

    private string GenerateTestMethodInvocation(IMethodSymbol method)
    {
        var methodName = method.Name;

        if (method.GetAttributes().Any(x =>
                x.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                    is "global::TUnit.Core.TimeoutAttribute"))
        {
            return $"{methodName}(EngineCancellationToken.Token)";
        }
        
        return $"{methodName}()";
    }
}

public record Method
{
    public SyntaxTree SyntaxTree { get; }
    public MethodDeclarationSyntax MethodDeclarationSyntax { get; }

    public Method(SyntaxTree syntaxTree, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        SyntaxTree = syntaxTree;
        MethodDeclarationSyntax = methodDeclarationSyntax;
    }
}