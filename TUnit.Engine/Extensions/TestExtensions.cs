﻿using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using TUnit.Core;
using TUnit.Engine.Constants;

namespace TUnit.Engine.Extensions;

internal static class TestExtensions
{
    public static TestInformation ToTestInformation(this TestCase testCase, Type classType, object? classInstance, MethodInfo methodInfo)
    {
        var classParameterTypes =
            testCase.GetPropertyValue(TUnitTestProperties.ClassParameterTypeNames, null as string[]);
        
        var methodParameterTypes =
            testCase.GetPropertyValue(TUnitTestProperties.MethodParameterTypeNames, null as string[]);
        
        return new TestInformation
        {
            TestName = testCase.GetPropertyValue(TUnitTestProperties.TestName, ""),
            MethodInfo = methodInfo,
            ClassType = classType,
            ClassInstance = classInstance,
            Categories = testCase.GetPropertyValue(TUnitTestProperties.Category, Array.Empty<string>()).ToList(),
            TestClassArguments = testCase.GetPropertyValue(TUnitTestProperties.ClassArguments, null as string).DeserializeArgumentsSafely(),
            TestMethodArguments = testCase.GetPropertyValue(TUnitTestProperties.MethodArguments, null as string).DeserializeArgumentsSafely(),
            TestClassArgumentTypes = classParameterTypes,
            TestMethodArgumentTypes = methodParameterTypes,
            Timeout = TimeSpan.FromMilliseconds(testCase.GetPropertyValue(TUnitTestProperties.Timeout, -1d)),
            RepeatCount = testCase.GetPropertyValue(TUnitTestProperties.RepeatCount, 0),
            RetryCount = testCase.GetPropertyValue(TUnitTestProperties.RetryCount, 0),
        };
    }

    public static TestCase ToTestCase(this TestDetails testDetails)
    {
        var testCase = new TestCase(testDetails.UniqueId, TestAdapterConstants.ExecutorUri, testDetails.Source)
        {
            DisplayName = testDetails.DisplayName,
            CodeFilePath = testDetails.FileName,
            LineNumber = testDetails.MinLineNumber,
        };

        testCase.SetPropertyValue(TUnitTestProperties.UniqueId, testDetails.UniqueId);

        var testMethodName = testDetails.MethodInfo.Name;
        
        testCase.SetPropertyValue(TUnitTestProperties.TestName, testMethodName);
        testCase.SetPropertyValue(TUnitTestProperties.AssemblyQualifiedClassName, testDetails.ClassType.AssemblyQualifiedName);

        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.IsSkipped, testDetails.IsSkipped);
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.IsStatic, testDetails.MethodInfo.IsStatic);
        
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.Category, testDetails.Categories.ToArray());
        
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.NotInParallelConstraintKey, testDetails.NotInParallelConstraintKey);
        
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.Timeout, testDetails.Timeout.TotalMilliseconds);
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.RepeatCount, testDetails.RepeatCount);
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.RetryCount, testDetails.RetryCount);

        var testParameterTypes = TestDetails.GetParameterTypes(testDetails.MethodParameterTypes);
        
        var managedMethod = $"{testMethodName}{testParameterTypes}";
        
        var hierarchy = new StringBuilder()
            .Append(testDetails.FullyQualifiedClassName)
            .ToString()
            .Split('.')
            .Append(managedMethod)
            .ToArray();
        
        testCase.SetPropertyValue(TUnitTestProperties.Hierarchy, hierarchy);
        testCase.SetPropertyValue(TUnitTestProperties.ManagedType, testDetails.FullyQualifiedClassName);
        testCase.SetPropertyValue(TUnitTestProperties.ManagedMethod, managedMethod);
        
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.MethodParameterTypeNames, testDetails.MethodParameterTypes?.Select(x => x.FullName).ToArray());
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.ClassParameterTypeNames, testDetails.ClassParameterTypes?.Select(x => x.FullName).ToArray());
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.MethodArguments, testDetails.MethodArgumentValues.SerializeArgumentsSafely());
        testCase.SetPropertyValueIfNotDefault(TUnitTestProperties.ClassArguments, testDetails.ClassArgumentValues.SerializeArgumentsSafely());

        if (testDetails.TestName.Contains("ParameterisedTests1"))
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestCase2.json");
            File.AppendAllText(path, JsonConvert.SerializeObject(testCase));
        }
        
        return testCase;
    }

    private static void SetPropertyValueIfNotDefault<T>(this TestCase testCase, TestProperty property, T value)
    {
        if (EqualityComparer<T>.Default.Equals(value,  default))
        {
            return;
        }
        
        testCase.SetPropertyValue(property, value);
    }
}