﻿namespace TUnit.TestProject.Bugs._2083;

public class Tests
{
    [Test]
    [Arguments(0)]
    [Arguments(byte.MaxValue)]
    [Arguments(short.MaxValue)]
    [Arguments(char.MaxValue)]
    [Arguments(int.MaxValue)]
    [Arguments(long.MaxValue)]
    public void MyTest(long value)
    {
    }
}