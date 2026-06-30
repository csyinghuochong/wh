namespace ET
{
    /// <summary>
    /// Skill handler that runs logic from SkillEditor TreeSave.xml.
    /// </summary>
    [SkillHandler]
    public class Skill_TreeEditor : SkillHandler
    {
        public override void OnInit(SkillInfo skillId, Unit theUnitFrom)
        {
            this.BaseOnInit(skillId, theUnitFrom);
        }

        public override void OnExecute()
        {
            if (this.LdSkillConf != null
                && SkillEditorTreeRegistry.TryGetTree(this.LdSkillConf.Id, out SkillEditorSkillLogic logic))
            {

                SkillEditorTreeExecutor.Execute(this, logic);
            }
        }

        public override void OnUpdate()
        {
            this.BaseOnUpdate();
            this.CheckChiXuHurt();
        }

        public override void OnFinished()
        {
            this.Clear();
        }
    }
}
