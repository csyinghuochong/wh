namespace ET
{
    /// <summary>
    /// Hand-written helper functions referenced from TreeSave function nodes.
    /// </summary>
    public static class SkillEditorFunctions
    {
        public static void RegisterAll()
        {
            SkillEditorFunctionRegistry.Register("function.test_1", Test1);
        }

        private static void Test1(SkillEditorFunctionContext ctx)
        {
            string p0 = ctx.Node.Params.Count > 0 ? ctx.ResolveParam(ctx.Node.Params[0]) : "0";
            string p1 = ctx.Node.Params.Count > 1 ? ctx.ResolveParam(ctx.Node.Params[1]) : "0";
            string p2 = ctx.Node.Params.Count > 2 ? ctx.ResolveParam(ctx.Node.Params[2]) : "0";

            Log.Debug($"SkillEditor function.test_1 skill={ctx.SkillId} params=({p0},{p1},{p2}) desc={ctx.Node.Desc}");
        }
    }
}
