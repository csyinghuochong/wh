using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// SkillEditor TreeSave.xml node types (mirrors DocEditor DeNodeType).
    /// </summary>
    public enum SkillEditorNodeType
    {
        Action = 0,
        BlankText,
        Function,
        IfRoot,
        ForRoot,
        IfCondition,
        IfResult,
    }

    public enum SkillEditorCompareOp
    {
        None,
        More,
        Less,
        Equal,
        NotEqual,
        MoreEqual,
        LessEqual,
        And,
        Or,
    }

    /// <summary>
    /// One node in a skill logic tree parsed from TreeSave.xml.
    /// </summary>
    public class SkillEditorTreeNode
    {
        public SkillEditorNodeType NodeType;
        public string Desc;
        public string Name;
        public string FrontText;
        public string BackText;
        public List<string> Params = new List<string>();
        /// <summary>Parallel to Params: skill table column id from XML param skillID attribute.</summary>
        public List<string> ParamSkillIds = new List<string>();
        public List<SkillEditorCompareOp> Operators = new List<SkillEditorCompareOp>();
        public List<SkillEditorTreeNode> Children = new List<SkillEditorTreeNode>();
    }

    /// <summary>
    /// Root action node for one skill (params[1] = skill id).
    /// </summary>
    public class SkillEditorSkillLogic
    {
        public int SkillId;
        public string FunctionType;
        public string Desc;
        public SkillEditorTreeNode Root;
    }
}
