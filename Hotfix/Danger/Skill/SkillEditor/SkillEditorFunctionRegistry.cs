using System;
using System.Collections.Generic;

namespace ET
{
    public delegate void SkillEditorFunctionHandler(SkillEditorFunctionContext ctx);

    /// <summary>
    /// Maps function names from TreeSave (e.g. function.test_1) to runtime handlers.
    /// Add implementations in SkillEditorFunctions.cs.
    /// </summary>
    public static class SkillEditorFunctionRegistry
    {
        private static readonly Dictionary<string, SkillEditorFunctionHandler> Handlers =
            new Dictionary<string, SkillEditorFunctionHandler>(StringComparer.Ordinal);

        static SkillEditorFunctionRegistry()
        {
            SkillEditorFunctions.RegisterAll();
        }

        public static void Register(string functionName, SkillEditorFunctionHandler handler)
        {
            Handlers[functionName] = handler;
        }

        public static bool TryInvoke(string functionName, SkillEditorFunctionContext ctx)
        {
            if (!Handlers.TryGetValue(functionName, out SkillEditorFunctionHandler handler))
            {
                Log.Warning($"SkillEditor: unknown function '{functionName}' on skill {ctx.SkillId}");
                return false;
            }

            handler(ctx);
            return true;
        }
    }
}
