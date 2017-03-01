﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using Mono.Cecil;
using Vandelay.Fody.Extensions;

namespace Vandelay.Fody
{
  partial class ModuleWeaver
  {
    void HandleExports()
    {
      foreach (var exportable in ModuleDefinition.Assembly.CustomAttributes.Where(a =>
        a.AttributeType.Name == nameof(ExporterAttribute)))
      {
        var exportType = ModuleDefinition.ImportReference(
          (TypeReference)exportable.ConstructorArguments.Single().Value);

        if (exportType.Resolve().CustomAttributes.Any(a =>
          a.AttributeType.FullName == typeof(InheritedExportAttribute).FullName &&
          a.ConstructorArguments.Count == 0))
        {
          continue;
        }

        var export = new CustomAttribute(ModuleDefinition.ImportReference(
          typeof(ExportAttribute).GetConstructor(new[] { typeof(Type) })));
        export.ConstructorArguments.Add(new CustomAttributeArgument(
          ModuleDefinition.TypeSystem.TypedReference, exportType));

        foreach (var type in ModuleDefinition.GetTypes().Where(t =>
          t.IsClass() && !t.IsAbstract && !t.ExportsType(exportType) &&
          (t.ImplementsInterface(exportType) || t.InheritsBase(exportType))))
        {
          type.CustomAttributes.Add(export);
        }
      }
    }
  }
}
