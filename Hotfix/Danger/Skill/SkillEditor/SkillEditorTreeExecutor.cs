using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Walks a parsed skill tree and invokes registered helper functions.
    /// </summary>
    public static class SkillEditorTreeExecutor
    {
        /// <summary>已提示过「缺少 IF结果」的技能，避免热路径刷屏。</summary>
        private static readonly HashSet<int> MissingIfResultWarnedSkills = new HashSet<int>();

        public static void Execute(Skill_TreeEditor handler, SkillEditorSkillLogic logic)
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
                case SkillEditorNodeType.IfResult:
                    ExecuteChildren(ctx, node);
                    break;

                case SkillEditorNodeType.IfRoot:
                    WarnMissingIfResultOnce(ctx, node);
                    ExecuteChildren(ctx, node);
                    break;

                case SkillEditorNodeType.ForRoot:
                    ExecuteForRoot(ctx, node);
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

        private static void WarnMissingIfResultOnce(SkillEditorFunctionContext ctx, SkillEditorTreeNode ifRoot)
        {
            if (ifRoot == null || ifRoot.Children == null)
            {
                return;
            }

            bool hasCondition = false;
            bool hasResult = false;
            for (int i = 0; i < ifRoot.Children.Count; i++)
            {
                SkillEditorNodeType childType = ifRoot.Children[i].NodeType;
                if (childType == SkillEditorNodeType.IfCondition)
                {
                    hasCondition = true;
                }
                else if (childType == SkillEditorNodeType.IfResult)
                {
                    hasResult = true;
                }
            }

            if (!hasCondition || hasResult)
            {
                return;
            }

            int skillId = ctx.Logic != null ? ctx.Logic.SkillId : 0;
            if (!MissingIfResultWarnedSkills.Add(skillId))
            {
                return;
            }

            string skillDesc = ctx.Logic != null ? ctx.Logic.Desc : string.Empty;
            Log.Warning($"SkillEditor skill={skillId}({skillDesc}) IF根缺少IF结果节点，结果分支不会执行");
        }

        /// <summary>
        /// v0: VECTOR_LOOP_START(targets, target) - iterate all skill targets, set TheUnitTarget each round.
        /// </summary>
        private static void ExecuteForRoot(SkillEditorFunctionContext ctx, SkillEditorTreeNode node)
        {
            if (ctx.Handler == null)
            {
                return;
            }

            ctx.SetVariable("__break", "0");

            Unit savedTarget = ctx.Handler.TheUnitTarget;
            List<long> targetIds = CollectLoopTargetIds(ctx.Handler);
            UnitComponent unitComponent = ctx.Handler.TheUnitFrom?.GetParent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            for (int i = 0; i < targetIds.Count; i++)
            {
                if (ctx.GetVariable("__break", 0) != 0)
                {
                    break;
                }

                Unit unit = unitComponent.Get(targetIds[i]);
                if (unit == null || unit.IsDisposed)
                {
                    continue;
                }

                ctx.Handler.TheUnitTarget = unit;
                ExecuteChildren(ctx, node);
            }

            ctx.Handler.TheUnitTarget = savedTarget;
        }

        /// <summary>
        /// Prefer HurtIds collected at runtime; fall back to SkillInfo.TargetID for single-target skills.
        /// </summary>
        private static List<long> CollectLoopTargetIds(Skill_TreeEditor handler)
        {
            List<long> targetIds = new List<long>();
            if (handler?.HurtIds != null && handler.HurtIds.Count > 0)
            {
                targetIds.AddRange(handler.HurtIds);
                return targetIds;
            }

            long targetId = handler?.SkillInfo?.TargetID ?? 0;
            if (targetId > 0)
            {
                targetIds.Add(targetId);
            }

            return targetIds;
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
