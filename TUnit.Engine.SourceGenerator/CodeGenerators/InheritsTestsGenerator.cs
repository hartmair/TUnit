﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TUnit.Engine.SourceGenerator.CodeGenerators.Helpers;
using TUnit.Engine.SourceGenerator.CodeGenerators.Writers;
using TUnit.Engine.SourceGenerator.Extensions;
using TUnit.Engine.SourceGenerator.Models;

namespace TUnit.Engine.SourceGenerator.CodeGenerators;

[Generator]
internal class InheritsTestsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var inheritsTestsClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName("TUnit.Core.InheritsTestsAttribute",
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);
        
        context.RegisterSourceOutput(inheritsTestsClasses, Execute);
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    static InheritsTestsDataModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return null;
        }
        
        if (namedTypeSymbol.IsAbstract)
        {
            return null;
        }

        if (namedTypeSymbol.IsStatic)
        {
            return null;
        }

        if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return null;
        }

        return new InheritsTestsDataModel(namedTypeSymbol.Name,
            namedTypeSymbol.GetMembersIncludingBase()
                .OfType<IMethodSymbol>()
                .Where(x => !x.IsAbstract)
                .Where(x => x.MethodKind != MethodKind.Constructor)
                .Where(x => x.IsTest())
                .SelectMany(x => x.ParseTestDatas(context, namedTypeSymbol))
        );
    }

    private void Execute(SourceProductionContext context, InheritsTestsDataModel? model)
    {
        if (model is null)
        {
            return;
        }

        foreach (var modelTestSourceDataModel in model.TestSourceDataModels)
        {
            var className = $"{model.MinimalTypeName}_Inherited";
            var fileName = $"{className}_{Guid.NewGuid():N}";

            using var sourceBuilder = new SourceCodeWriter();

            sourceBuilder.WriteLine("// <auto-generated/>");
            sourceBuilder.WriteLine("using global::TUnit.Core;");
            sourceBuilder.WriteLine("using global::TUnit.Engine;");
            sourceBuilder.WriteLine();
            sourceBuilder.WriteLine("namespace TUnit.Engine;");
            sourceBuilder.WriteLine();
            sourceBuilder.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
            sourceBuilder.WriteLine($"file partial class {className}");
            sourceBuilder.WriteLine("{");
            sourceBuilder.WriteLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
            sourceBuilder.WriteLine("public static void Initialise()");
            sourceBuilder.WriteLine("{");

            if(modelTestSourceDataModel.IsEnumerableClassArguments)
            {
                sourceBuilder.WriteLine($"var {VariableNames.EnumerableClassDataIndex} = 0;");
            }

            if(modelTestSourceDataModel.IsEnumerableMethodArguments)
            {
                sourceBuilder.WriteLine($"var {VariableNames.EnumerableTestDataIndex} = 0;");
            }

            sourceBuilder.WriteLine("try");
            sourceBuilder.WriteLine("{");
            GenericTestInvocationWriter.GenerateTestInvocationCode(sourceBuilder, modelTestSourceDataModel);
            sourceBuilder.WriteLine("}");
            sourceBuilder.WriteLine("catch (global::System.Exception exception)");
            sourceBuilder.WriteLine("{");
            FailedTestInitializationWriter.GenerateFailedTestCode(sourceBuilder, modelTestSourceDataModel);
            sourceBuilder.WriteLine("}");
            
            sourceBuilder.WriteLine("}");
            sourceBuilder.WriteLine("}");

            context.AddSource($"{fileName}.Generated.cs", sourceBuilder.ToString());
        }
    }
}