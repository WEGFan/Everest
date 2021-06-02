using System;
using Mono.Cecil;
using MonoMod.Cil;

namespace MonoMod {
    [MonoModCustomMethodAttribute("PatchErrorLogGetLogPath")]
    internal class PatchErrorLogGetLogPathAttribute : Attribute {
    }

    static partial class MonoModRules {
        public static void PatchErrorLogGetLogPath(ILContext il, CustomAttribute attrib) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdstr("errorLog.txt"))
            ) {
                cursor.Next.Operand = "error_log.txt";
            }
        }
    }
}