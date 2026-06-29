namespace ET
{
    /// <summary>
    /// Walks a parsed skill tree and invokes registered helper functions.
    /// </summary>
    public static class SkillEditorTreeExecutor
    {
        public static void Execute(SkillHandler handler, SkillEditorSkillLogic logic)
        {
            if (handler == null || logic?.Root == null)
            {
                return;
            }

            SkillEditorFunctionContext ctx = new SkillEditorFunctionContext
            {
                Handler = handler,
                Logic = logic,
            };

            ExecuteNode(ctx, logic.Root);
        }

        private static void ExecuteNode(SkillEditorFunctionContext ctx, SkillEditorTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            ctx.Node = node;

            switch (node.NodeType)
            {
                case SkillEditorNodeType.Action:
                case SkillEditorNodeType.IfRoot:
                case SkillEditorNodeType.ForRoot:
                case SkillEditorNodeType.IfResult:
                    ExecuteChildren(ctx, node);
                    break;

                case SkillEditorNodeType.Function:
                    if (!string.IsNullOrEmpty(node.Name))
                    {
                        SkillEditorFunctionRegistry.TryInvoke(node.Name, ctx);
                    }
                    break;

                case SkillEditorNodeType.IfCondition:
                    if (EvaluateCondition(ctx, node))
                    {
                        ExecuteChildren(ctx, node);
                    }
                    break;

                case SkillEditorNodeType.BlankText:
                    break;
            }
        }

        private static void ExecuteChildren(SkillEditorFunctionContext ctx, SkillEditorTreeNode node)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                ExecuteNode(ctx, node.Children[i]);
            }
        }

        private static bool EvaluateCondition(SkillEditorFunctionContext ctx, SkillEditorTreeNode conditionNode)
        {
            if (conditionNode.Operators == null || conditionNode.Operators.Count == 0)
            {
                return true;
            }

            // TODO: evaluate condition expressions from child function nodes / params
            return true;
        }
    }
}
