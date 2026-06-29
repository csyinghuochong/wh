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
            bool hasFunctionChild = false;
            for (int i = 0; i < conditionNode.Children.Count; i++)
            {
                SkillEditorTreeNode child = conditionNode.Children[i];
                if (child.NodeType == SkillEditorNodeType.Function)
                {
                    hasFunctionChild = true;
                    ctx.Node = child;
                    SkillEditorFunctionRegistry.TryInvoke(child.Name, ctx);
                }
            }

            if (conditionNode.Operators == null || conditionNode.Operators.Count == 0)
            {
                return hasFunctionChild ? ctx.LastConditionResult : true;
            }

            long rs = ctx.GetVariable("rs", 0);
            bool result = rs > 0;
            for (int i = 0; i < conditionNode.Operators.Count; i++)
            {
                switch (conditionNode.Operators[i])
                {
                    case SkillEditorCompareOp.And:
                        result = result && rs > 0;
                        break;
                    case SkillEditorCompareOp.Or:
                        result = result || rs > 0;
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
    }
}
