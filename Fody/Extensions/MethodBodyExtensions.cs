﻿using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil.Cil;

namespace Vandelay.Fody.Extensions
{
  static class MethodBodyExtensions
  {
    public static void UpdateInstructions([NotNull] this MethodBody body,
      [NotNull] Instruction oldInstruction, [NotNull] Instruction newInstruction)
    {
      foreach (var updateInstruction in body.Instructions
        .Where(i => i.Operand == oldInstruction))
      {
        updateInstruction.Operand = newInstruction;
      }

      foreach (var updateInstruction in body.ExceptionHandlers
        .Where(h => h.HandlerEnd == oldInstruction))
      {
        updateInstruction.HandlerEnd = newInstruction;
      }
    }
  }
}
