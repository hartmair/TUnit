﻿using Microsoft.CodeAnalysis;
using TUnit.Engine.SourceGenerator.CodeGenerators.Helpers;
using TUnit.Engine.SourceGenerator.Enums;
using TUnit.Engine.SourceGenerator.Models;

namespace TUnit.Engine.SourceGenerator.CodeGenerators.Writers.Hooks;

internal static class AssemblyHooksWriter
{
    public static void Execute(SourceProductionContext context, HooksDataModel? model, HookLocationType hookLocationType)
    {
        if (model is null)
        {
            return;
        }
        
        var className = $"AssemblyHooks_{model.MinimalTypeName}";
        var fileName = $"{className}_{Guid.NewGuid():N}";

        using var sourceBuilder = new SourceCodeWriter();
                
        sourceBuilder.WriteLine("// <auto-generated/>");
        sourceBuilder.WriteLine("using global::System.Linq;");
        sourceBuilder.WriteLine("using global::System.Reflection;");
        sourceBuilder.WriteLine("using global::System.Runtime.CompilerServices;");
        sourceBuilder.WriteLine("using global::TUnit.Core;");
        sourceBuilder.WriteLine("using global::TUnit.Core.Interfaces;");
        sourceBuilder.WriteLine("using global::TUnit.Engine;");
        sourceBuilder.WriteLine("using global::TUnit.Engine.Helpers;");
        sourceBuilder.WriteLine("using global::TUnit.Engine.Hooks;");
        sourceBuilder.WriteLine();
        sourceBuilder.WriteLine("namespace TUnit.Engine;");
        sourceBuilder.WriteLine();
        sourceBuilder.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sourceBuilder.WriteLine($"file partial class {className}");
        sourceBuilder.WriteLine("{");
        sourceBuilder.WriteLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
        sourceBuilder.WriteLine("public static void Initialise()");
        sourceBuilder.WriteLine("{");

        if (hookLocationType == HookLocationType.Before)
        {
            sourceBuilder.WriteLine(
                $$$"""
                   AssemblyHookOrchestrator.RegisterBeforeHook(typeof({{{model.FullyQualifiedTypeName}}}).Assembly, new StaticHookMethod<AssemblyHookContext>
                   		{ 
                              MethodInfo = typeof({{{model.FullyQualifiedTypeName}}}).GetMethod("{{{model.MethodName}}}", 0, [{{{string.Join(", ", model.ParameterTypes.Select(x => $"typeof({x})"))}}}]),
                              Body = (context, cancellationToken) => AsyncConvert.Convert(() => {{{model.FullyQualifiedTypeName}}}.{{{model.MethodName}}}({{{GetArgs(model)}}})),
                              HookExecutor = {{{HookExecutorHelper.GetHookExecutor(model.HookExecutor)}}},
                              Order = {{{model.Order}}},
                              FilePath = @"{{{model.FilePath}}}",
                              LineNumber = {{{model.LineNumber}}},
                   		});
                   """);
        }
        else if (hookLocationType == HookLocationType.After)
        {
            sourceBuilder.WriteLine(
                $$$"""
                   AssemblyHookOrchestrator.RegisterAfterHook(typeof({{{model.FullyQualifiedTypeName}}}).Assembly, new StaticHookMethod<AssemblyHookContext>
                   		{ 
                             MethodInfo = typeof({{{model.FullyQualifiedTypeName}}}).GetMethod("{{{model.MethodName}}}", 0, [{{{string.Join(", ", model.ParameterTypes.Select(x => $"typeof({x})"))}}}]),
                             Body = (context, cancellationToken) => AsyncConvert.Convert(() => {{{model.FullyQualifiedTypeName}}}.{{{model.MethodName}}}({{{GetArgs(model)}}})),
                             HookExecutor = {{{HookExecutorHelper.GetHookExecutor(model.HookExecutor)}}},
                             Order = {{{model.Order}}},
                             FilePath = @"{{{model.FilePath}}}",
                             LineNumber = {{{model.LineNumber}}},
                   		});
                   """);
        }

        sourceBuilder.WriteLine("}");
        sourceBuilder.WriteLine("}");

        context.AddSource($"{fileName}.Generated.cs", sourceBuilder.ToString());
    }
    
    private static string GetArgs(HooksDataModel model)
    {
        List<string> args = [];
        
        foreach (var type in model.ParameterTypes)
        {
            if (type == WellKnownFullyQualifiedClassNames.AssemblyHookContext.WithGlobalPrefix)
            {
                args.Add("context");
            }
            
            if (type == WellKnownFullyQualifiedClassNames.CancellationToken.WithGlobalPrefix)
            {
                args.Add("cancellationToken");
            }
        }

        return string.Join(", ", args);
    }
}