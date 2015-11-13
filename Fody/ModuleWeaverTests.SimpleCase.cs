﻿using System;
using System.ComponentModel.Composition;
using NUnit.Framework;

namespace Vandelay.Fody
{
  [TestFixture]
  public class SimpleCaseTests
  {
    ModuleWeaverTestHelper _simpleCaseWeaver;
    Type _simpleCaseExportableType;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      _simpleCaseWeaver = new ModuleWeaverTestHelper(
        @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.SimpleCase.dll");
      Assert.That(_simpleCaseWeaver.Errors, Is.Null.Or.Empty);

      _simpleCaseExportableType = _simpleCaseWeaver.GetType(
        "AssemblyToProcess.SimpleCase.IExportable");
    }

    [TestCase("AssemblyToProcess.SimpleCase.AbstractExportable")]
    public void AbstractTest(string className)
    {
      // Arrange
      var type = _simpleCaseWeaver.GetType(className);

      // Act
      var exports = type.GetCustomAttributes(typeof(ExportAttribute), false);

      // Assert
      Assert.That(exports, Is.Not.Null);
      Assert.That(exports, Is.Empty);
    }

    [TestCase("AssemblyToProcess.SimpleCase.ExportableInstance")]
    [TestCase("AssemblyToProcess.SimpleCase.AlreadyExportedInstance")]
    [TestCase("AssemblyToProcess.SimpleCase.NonPublicExported")]
    [TestCase("AssemblyToProcess.SimpleCase.ImplementsExtended")]
    public void InstanceTest(string className)
    {
      // Arrange
      var type = _simpleCaseWeaver.GetType(className);

      // Act
      var exports = type.GetCustomAttributes(typeof(ExportAttribute), false);

      // Assert
      Assert.That(exports, Is.Not.Null);
      Assert.That(exports, Has.Length.EqualTo(1));

      var attribute = exports[0] as ExportAttribute;
      Assert.That(attribute, Is.Not.Null);
      Assert.That(attribute, Has.Property("ContractType").EqualTo(_simpleCaseExportableType));
    }

    [Test]
    public void PeVerify()
    {
      // Arrange

      // Act
      Verifier.Verify(_simpleCaseWeaver.BeforeAssemblyPath,
        _simpleCaseWeaver.AfterAssemblyPath);

      // Assert
    }
  }
}
