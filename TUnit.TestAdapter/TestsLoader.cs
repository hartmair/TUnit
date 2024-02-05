﻿using System.Reflection;
using TUnit.Core;
using TUnit.Engine;

namespace TUnit.TestAdapter;

internal class TestsLoader(SourceLocationHelper sourceLocationHelper, ClassLoader classLoader, TestDataSourceRetriever testDataSourceRetriever)
{
    private static readonly Type[] TestAttributes = [typeof(TestAttribute), typeof(TestWithDataAttribute), typeof(TestDataSourceAttribute)];

    public IEnumerable<TestDetails> GetTests(TypeInformation typeInformation, Assembly[] allAssemblies)
    {
        var methods = typeInformation.Types.SelectMany(x => x.GetMethods());

        foreach (var methodInfo in methods)
        {
            if (!HasTestAttributes(methodInfo))
            {
                continue;
            }
            
            var sourceLocation = sourceLocationHelper
                .GetSourceLocation(typeInformation.Assembly.Location, methodInfo.DeclaringType!.FullName!, methodInfo.Name);

            var allClasses = classLoader.GetAllTypes(allAssemblies).ToArray();
            
            var nonAbstractClassesContainingTest = allClasses
                .Where(t => t.IsAssignableTo(methodInfo.DeclaringType!) && !t.IsAbstract)
                .ToArray();

            var repeatCount = methodInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType == typeof(RepeatAttribute))
                ?.ConstructorArguments.First().Value as int? ?? 0;

            var runCount = repeatCount + 1;
            
            foreach (var test in CollectTestWithDataAttributeTests(methodInfo, nonAbstractClassesContainingTest, runCount, sourceLocation))
            {
                yield return test;
            }

            foreach (var test in CollectStandardTests(methodInfo, nonAbstractClassesContainingTest, runCount, sourceLocation))
            {
                yield return test;
            }

            foreach (var test in CollectTestDataSourceTests(methodInfo, nonAbstractClassesContainingTest, runCount, sourceLocation, allClasses))
            {
                yield return test;
            }
        }
    }

    private IEnumerable<TestDetails> CollectTestDataSourceTests(MethodInfo methodInfo,
        Type[] nonAbstractClassesContainingTest, int runCount, SourceLocation sourceLocation, Type[] allClasses)
    {
        var testDataSourceAttributes = methodInfo.GetCustomAttributes<TestDataSourceAttribute>().ToList();
        
        if (!testDataSourceAttributes.Any())
        {
            yield break;
        }
        
        var count = 0;
        
        foreach (var testDataSourceAttribute in testDataSourceAttributes)
        {
            count++;
            foreach (var classType in nonAbstractClassesContainingTest)
            {
                for (var i = 1; i <= runCount; i++)
                {
                    yield return new TestDetails(
                        methodInfo: methodInfo,
                        classType: classType,
                        sourceLocation: sourceLocation,
                        arguments: testDataSourceRetriever.GetTestDataSourceArguments(methodInfo,
                            testDataSourceAttribute, allClasses),
                        count: count * i
                    );
                }
            }
        }
    }

    private static IEnumerable<TestDetails> CollectStandardTests(MethodInfo methodInfo, Type[] nonAbstractClassesContainingTest,
        int runCount, SourceLocation sourceLocation)
    {
        if (!methodInfo.GetCustomAttributes<TestAttribute>().Any())
        {
            yield break;
        }

        foreach (var classType in nonAbstractClassesContainingTest)
        {
            for (var i = 1; i <= runCount; i++)
            {
                yield return new TestDetails(
                    methodInfo: methodInfo,
                    classType: classType,
                    sourceLocation: sourceLocation,
                    arguments: null,
                    count: i
                );
            }
        }
    }

    private static IEnumerable<TestDetails> CollectTestWithDataAttributeTests(MethodInfo methodInfo,
        Type[] nonAbstractClassesContainingTest, int runCount, SourceLocation sourceLocation)
    {
        var testWithDataAttributes = methodInfo.GetCustomAttributes<TestWithDataAttribute>().ToList();
        
        if (!testWithDataAttributes.Any())
        {
            yield break;
        }
        
        var count = 0;
        
        foreach (var testWithDataAttribute in testWithDataAttributes)
        {
            count++;
            var arguments = testWithDataAttribute.Values.Select(x => new ParameterArgument(x?.GetType() ?? typeof(object), x)).ToArray();
                    
            foreach (var classType in nonAbstractClassesContainingTest)
            {
                for (var i = 1; i <= runCount; i++)
                {
                    yield return new TestDetails(
                        methodInfo: methodInfo,
                        classType: classType,
                        sourceLocation: sourceLocation,
                        arguments: arguments,
                        count: count * i
                    );
                }
            }
        }
    }

    private static bool HasTestAttributes(MethodInfo methodInfo)
    {
        return methodInfo.CustomAttributes
            .Select(x => x.AttributeType)
            .Intersect(TestAttributes)
            .Any();
    }
}