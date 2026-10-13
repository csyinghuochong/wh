namespace ET
{
    public partial class LDExp_LvCategory
    {
        /// <summary>角色最大等级。该级升级经验为 -1。</summary>
        public int RoleMaxLv;

        /// <summary>坐骑最大等级。该级升级经验为 -1。</summary>
        public int MountMaxLv;

        /// <summary>宠物最大等级。宠物经验列还没有，暂时等于角色。</summary>
        public int PetMaxLv;

        public override void AfterEndInit()
        {
            int roleMaxLv = 0;
            int mountMaxLv = 0;
            foreach (LDExp_Lv config in this.GetAll().Values)
            {
                if (config.Exp_Role == -1 && config.Id > roleMaxLv)
                {
                    roleMaxLv = config.Id;
                }

                if (config.Exp_Mount == -1 && config.Id > mountMaxLv)
                {
                    mountMaxLv = config.Id;
                }
            }

            this.RoleMaxLv = roleMaxLv;
            this.MountMaxLv = mountMaxLv;
            this.PetMaxLv = roleMaxLv;
        }
    }
}
