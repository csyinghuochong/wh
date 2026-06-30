using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ET
{
    /// <summary>
    /// Loads Config/SkillEditor/TreeSave.xml at server startup.
    /// </summary>
    public static class SkillEditorTreeLoader
    {
        public const string DefaultTreeSavePath = "../Config/SkillEditor/TreeSave.xml";

        public static void Load(string path = DefaultTreeSavePath)
        {
            SkillEditorTreeRegistry.Reset();

            if (!File.Exists(path))
            {
                Log.Warning($"SkillEditor TreeSave not found: {Path.GetFullPath(path)}");
                SkillEditorTreeRegistry.SetLoaded(true);
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlElement root = doc.DocumentElement;
                if (root == null || root.Name != "root")
                {
                    Log.Error("SkillEditor TreeSave.xml: missing root element");
                    SkillEditorTreeRegistry.SetLoaded(true);
                    return;
                }

                foreach (XmlNode child in root.ChildNodes)
                {
                    if (child is XmlElement elem && elem.Name == "node" && GetAttr(elem, "type") == "action")
                    {
                        SkillEditorSkillLogic logic = ParseAction(elem);
                        if (logic != null && logic.SkillId > 0)
                        {
                            SkillEditorTreeRegistry.SkillTrees[logic.SkillId] = logic;
                        }
                    }
                }

                Log.Info($"SkillEditor TreeSave loaded: {SkillEditorTreeRegistry.SkillTrees.Count} skill(s) from {path}");
            }
            catch (Exception e)
            {
                Log.Error($"SkillEditor TreeSave load failed: {e}");
            }

            SkillEditorTreeRegistry.SetLoaded(true);
        }

        private static SkillEditorSkillLogic ParseAction(XmlElement actionElem)
        {
            SkillEditorTreeNode rootNode = ParseNode(actionElem, SkillEditorNodeType.Action);
            if (rootNode == null)
            {
                return null;
            }

            string functionType = string.Empty;
            int skillId = 0;
            if (rootNode.Params.Count >= 2)
            {
                // Legacy: params[0]=function type, params[1]=skill id
                functionType = rootNode.Params[0];
                int.TryParse(rootNode.Params[1], out skillId);
            }
            else if (rootNode.Params.Count == 1)
            {
                // Current editor: params[0]=skill id only
                int.TryParse(rootNode.Params[0], out skillId);
            }

            return new SkillEditorSkillLogic
            {
                SkillId = skillId,
                FunctionType = functionType,
                Desc = rootNode.Desc,
                Root = rootNode,
            };
        }

        private static SkillEditorTreeNode ParseNode(XmlElement elem, SkillEditorNodeType nodeType)
        {
            SkillEditorTreeNode node = new SkillEditorTreeNode
            {
                NodeType = nodeType,
                Desc = GetAttr(elem, "desc"),
                Name = GetAttr(elem, "name"),
            };

            foreach (XmlNode child in elem.ChildNodes)
            {
                if (!(child is XmlElement childElem))
                {
                    continue;
                }

                switch (childElem.Name)
                {
                    case "front_text":
                        node.FrontText = childElem.InnerText ?? string.Empty;
                        break;
                    case "back_text":
                        node.BackText = childElem.InnerText ?? string.Empty;
                        break;
                    case "params":
                        ParseParams(childElem, node.Params, node.ParamSkillIds);
                        break;
                    case "operators":
                        ParseOperators(childElem, node.Operators);
                        break;
                    case "children":
                        ParseChildren(childElem, node.Children);
                        break;
                }
            }

            return node;
        }

        private static void ParseParams(XmlElement paramsElem, List<string> output, List<string> skillIdOutput)
        {
            foreach (XmlNode child in paramsElem.ChildNodes)
            {
                if (child is XmlElement paramElem && paramElem.Name == "param")
                {
                    output.Add(paramElem.InnerText ?? string.Empty);
                    string skillIdAttr = paramElem.GetAttribute("skillID");
                    skillIdOutput.Add(skillIdAttr ?? string.Empty);
                }
            }
        }

        private static void ParseOperators(XmlElement operatorsElem, List<SkillEditorCompareOp> output)
        {
            foreach (XmlNode child in operatorsElem.ChildNodes)
            {
                if (child is XmlElement operElem && operElem.Name == "oper")
                {
                    output.Add(ParseCompareOp(operElem.InnerText));
                }
            }
        }

        private static void ParseChildren(XmlElement childrenElem, List<SkillEditorTreeNode> output)
        {
            foreach (XmlNode child in childrenElem.ChildNodes)
            {
                if (!(child is XmlElement nodeElem) || nodeElem.Name != "node")
                {
                    continue;
                }

                SkillEditorNodeType nodeType = ParseNodeType(GetAttr(nodeElem, "type"));
                SkillEditorTreeNode parsed = ParseNode(nodeElem, nodeType);
                if (parsed != null)
                {
                    output.Add(parsed);
                }
            }
        }

        private static SkillEditorNodeType ParseNodeType(string type)
        {
            switch (type)
            {
                case "action": return SkillEditorNodeType.Action;
                case "blank_text": return SkillEditorNodeType.BlankText;
                case "function": return SkillEditorNodeType.Function;
                case "if_root": return SkillEditorNodeType.IfRoot;
                case "for_root": return SkillEditorNodeType.ForRoot;
                case "if_condition": return SkillEditorNodeType.IfCondition;
                case "if_result": return SkillEditorNodeType.IfResult;
                default: return SkillEditorNodeType.BlankText;
            }
        }

        private static SkillEditorCompareOp ParseCompareOp(string op)
        {
            switch (op)
            {
                case "More": return SkillEditorCompareOp.More;
                case "Less": return SkillEditorCompareOp.Less;
                case "Equal": return SkillEditorCompareOp.Equal;
                case "NotEqual":
                case "~=": return SkillEditorCompareOp.NotEqual;
                case "MoreEqual":
                case ">=": return SkillEditorCompareOp.MoreEqual;
                case "LessEqual":
                case "<=": return SkillEditorCompareOp.LessEqual;
                case "And": return SkillEditorCompareOp.And;
                case "Or": return SkillEditorCompareOp.Or;
                default: return SkillEditorCompareOp.None;
            }
        }

        private static string GetAttr(XmlElement elem, string name)
        {
            return elem.GetAttribute(name) ?? string.Empty;
        }
    }
}
