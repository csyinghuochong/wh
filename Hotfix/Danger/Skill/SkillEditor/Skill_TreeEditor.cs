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
            this.InitSelfBuff();

            if (this.LdSkillConf != null
                && SkillEditorTreeRegistry.TryGetTree(this.LdSkillConf.Id, out SkillEditorSkillLogic logic))
            {
                SkillEditorTreeExecutor.Execute(this, logic);
            }

            this.OnUpdate();
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
