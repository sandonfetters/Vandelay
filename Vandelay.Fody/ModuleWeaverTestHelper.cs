﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Fody;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Scalpel;

namespace Vandelay.Fody
{
  [Remove]
  class ModuleWeaverTestHelper
  {
    [NotNull]
    public readonly string BeforeAssemblyPath;

    [NotNull]
    public readonly string AfterAssemblyPath;

    [NotNull]
    public readonly Assembly Assembly;

    [NotNull]
    public readonly List<string> Errors;

    public ModuleWeaverTestHelper([NotNull] string inputAssembly)
    {
      BeforeAssemblyPath = Path.GetFullPath(inputAssembly);
      AfterAssemblyPath = BeforeAssemblyPath.Replace(".dll", "2.dll");
      var oldPdb = BeforeAssemblyPath.Replace(".dll", ".pdb");

      Errors = new List<string>();

      using (var symbolStream = File.OpenRead(oldPdb))
      {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Directory.GetParent(BeforeAssemblyPath).FullName);

        var readerParameters = new ReaderParameters
        {
          ReadSymbols = true,
          SymbolStream = symbolStream,
          SymbolReaderProvider = new PdbReaderProvider(),
          AssemblyResolver = resolver
        };
        using (var moduleDefinition = ModuleDefinition.ReadModule(
          BeforeAssemblyPath, readerParameters))
        {
          var weavingTask = new ModuleWeaver
          {
            ModuleDefinition = moduleDefinition,
            LogError = Errors.Add
          };

          var typeCache = CacheTypes(weavingTask);
          weavingTask.FindType = typeCache.FindType;
          weavingTask.TypeSystem = new global::Fody.TypeSystem(typeCache.FindType, moduleDefinition);

          weavingTask.Execute();
          moduleDefinition.Write(AfterAssemblyPath);
        }
      }

      Assembly = Assembly.LoadFile(AfterAssemblyPath);
    }

    [NotNull]
    public Type GetType([NotNull] string className) =>
      Assembly.GetType(className, true);

#pragma warning disable 618
    [NotNull]
    static TypeCache CacheTypes([NotNull] BaseModuleWeaver weavingTask)
    {
      var assemblyResolver = Info.OfConstructor("FodyHelpers", "Fody.MockAssemblyResolver").Invoke(null);
      var resolve = Info.OfMethod("FodyHelpers", "Fody.MockAssemblyResolver", "Resolve", "String");

      var typeCache = new TypeCache(a => (AssemblyDefinition)resolve.Invoke(assemblyResolver, new object[] { a }));
      typeCache.BuildAssembliesToScan(weavingTask);

      return typeCache;
    }
#pragma warning restore 618
  }
}