﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Vandelay.Fody.Extensions;

namespace Vandelay.Fody
{
  partial class ModuleWeaver
  {
    [CanBeNull]
    MethodReference CreateCompositionBatch { get; set; }

    void InjectCompositionBatchHelper()
    {
      if (null != CreateCompositionBatch)
      {
        return;
      }

      // internal static class CompositionBatchHelper
      var compositionBatch = new TypeDefinition(
        "Vandelay", "CompositionBatchHelper",
        TypeAttributes.AnsiClass | TypeAttributes.Sealed |
        TypeAttributes.AutoClass | TypeAttributes.Abstract,
        TypeSystem.ObjectReference);

      var createCompositionBatch = InjectCreate();
      compositionBatch.Methods.Add(createCompositionBatch);
      CreateCompositionBatch = createCompositionBatch;

      ModuleDefinition.Types.Add(compositionBatch);
    }

    [NotNull]
    MethodDefinition InjectCreate()
    {
      // public static CompositionBatch Create(object[] array)
      var compositionBatch = new MethodDefinition("Create",
        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
        ModuleDefinition.ImportReference(typeof(CompositionBatch)));
      compositionBatch.Parameters.Add(new ParameterDefinition(
        ModuleDefinition.ImportReference(typeof(object[]))));
      compositionBatch.CustomAttributes.MarkAsGeneratedCode();

      compositionBatch.Body.Variables.Add(new VariableDefinition(
        ModuleDefinition.ImportReference(typeof(CompositionBatch))));
      compositionBatch.Body.Variables.Add(new VariableDefinition(
        TypeSystem.Int32Reference));
      compositionBatch.Body.Variables.Add(new VariableDefinition(
        TypeSystem.ObjectReference));
      compositionBatch.Body.Variables.Add(new VariableDefinition(
        ModuleDefinition.ImportReference(typeof(Type))));
      compositionBatch.Body.InitLocals = true;

      foreach (var instruction in GetInstructions())
      {
        compositionBatch.Body.Instructions.Add(instruction);
      }

      return compositionBatch;
    }

    [NotNull]
    IEnumerable<Instruction> GetInstructions()
    {
      // var compositionBatch = new CompositionBatch();
      yield return Instruction.Create(OpCodes.Newobj,
        ModuleDefinition.ImportReference(Info.OfConstructor(
          "System.ComponentModel.Composition",
          "System.ComponentModel.Composition.Hosting.CompositionBatch")));
      yield return Instruction.Create(OpCodes.Stloc_0);

      // var i = 0;
      yield return Instruction.Create(OpCodes.Ldc_I4_0);
      yield return Instruction.Create(OpCodes.Stloc_1);

      var loopConditionCheck = Instruction.Create(OpCodes.Ldloc_1);

      // goto loopConditionCheck;
      yield return Instruction.Create(OpCodes.Br_S,
        loopConditionCheck);

      // loopStart:
      // var obj = array[i];
      var loopStart = Instruction.Create(OpCodes.Ldarg_0);
      yield return loopStart;
      yield return Instruction.Create(OpCodes.Ldloc_1);
      yield return Instruction.Create(OpCodes.Ldelem_Ref);
      yield return Instruction.Create(OpCodes.Stloc_2);

      // var type = obj.GetType();
      yield return Instruction.Create(OpCodes.Ldloc_2);
      yield return Instruction.Create(OpCodes.Callvirt,
        ModuleDefinition.ImportReference(Info.OfMethod(
          "mscorlib", "System.Object", "GetType")));
      yield return Instruction.Create(OpCodes.Stloc_3);

      // var contractName = AttributedModelServices.GetContractName(type);
      yield return Instruction.Create(OpCodes.Ldloc_0);
      yield return Instruction.Create(OpCodes.Ldloc_3);
      yield return Instruction.Create(OpCodes.Call,
        ModuleDefinition.ImportReference(Info.OfMethod(
          "System.ComponentModel.Composition",
          "System.ComponentModel.Composition.AttributedModelServices",
          "GetContractName", "Type")));

      // var metaData = new Dictionary<string, object>
      // {
      //   ["ExportTypeIdentity"] = AttributedModelServices.GetTypeIdentity(type)
      // }
      yield return Instruction.Create(OpCodes.Newobj,
        ModuleDefinition.ImportReference(Info.OfConstructor("mscorlib", "System.Collections.Generic.Dictionary`2<" +
          "mscorlib|System.String,mscorlib|System.Object>")));
      yield return Instruction.Create(OpCodes.Dup);
      yield return Instruction.Create(OpCodes.Ldstr,
        "ExportTypeIdentity");
      yield return Instruction.Create(OpCodes.Ldloc_3);
      yield return Instruction.Create(OpCodes.Call,
        ModuleDefinition.ImportReference(Info.OfMethod(
          "System.ComponentModel.Composition",
          "System.ComponentModel.Composition.AttributedModelServices",
          "GetTypeIdentity", "Type")));
      yield return Instruction.Create(OpCodes.Callvirt,
        ModuleDefinition.ImportReference(Info.OfPropertySet("mscorlib",
        "System.Collections.Generic.Dictionary`2<" +
        "mscorlib|System.String,mscorlib|System.Object>", "Item")));

      // var valueProvider = new ExportValueProvider(obj);
      yield return Instruction.Create(OpCodes.Ldloc_2);
      yield return Instruction.Create(OpCodes.Newobj,
        ExportValueProvider.GetConstructors().First());

      // var valueFunc = new Func<object>(valueProvider.GetValue);
      yield return Instruction.Create(OpCodes.Ldftn,
        ExportValueProvider.Methods.First(m => !m.IsConstructor));
      yield return Instruction.Create(OpCodes.Newobj,
        ModuleDefinition.ImportReference(Info.OfConstructor("mscorlib",
        "System.Func`1<mscorlib|System.Object>", "Object,IntPtr")));

      // var export = new Export(contractName, metaData, valueFunc);
      yield return Instruction.Create(OpCodes.Newobj,
        ModuleDefinition.ImportReference(Info.OfConstructor(
          "System.ComponentModel.Composition",
          "System.ComponentModel.Composition.Primitives.Export",
          "String,IDictionary`2,Func`1")));

      // compositionBatch.AddExport(export);
      yield return Instruction.Create(OpCodes.Callvirt,
        ModuleDefinition.ImportReference(Info.OfMethod(
          "System.ComponentModel.Composition",
          "System.ComponentModel.Composition.Hosting.CompositionBatch",
          "AddExport", "Export")));
      yield return Instruction.Create(OpCodes.Pop);

      // i++;
      yield return Instruction.Create(OpCodes.Ldloc_1);
      yield return Instruction.Create(OpCodes.Ldc_I4_1);
      yield return Instruction.Create(OpCodes.Add);
      yield return Instruction.Create(OpCodes.Stloc_1);

      // loopConditionCheck:
      // if (i < array.Length) goto loopStart;
      yield return loopConditionCheck;
      yield return Instruction.Create(OpCodes.Ldarg_0);
      yield return Instruction.Create(OpCodes.Ldlen);
      yield return Instruction.Create(OpCodes.Conv_I4);
      yield return Instruction.Create(OpCodes.Blt_S, loopStart);

      // return compositionBatch;
      yield return Instruction.Create(OpCodes.Ldloc_0);
      yield return Instruction.Create(OpCodes.Ret);
    }
  }
}