using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class UnitInfoComponentAwakeSystem : AwakeSystem<UnitInfoComponent>
    {
        public override void Awake(UnitInfoComponent self)
        {
            self.ResetData();
        }
    }

    [ObjectSystem]
    public class UnitInfoComponentDestroySystem : DestroySystem<UnitInfoComponent>
    {
        public override void Destroy(UnitInfoComponent self)
        {
            self.ResetData();
        }
    }

    public static class UnitInfoComponentServerSystem
    {




        public static void ResetData(this UnitInfoComponent self)
        {
            self.ZhaohuanIds.Clear();
            self.FashionEquipList.Clear();
            self.Sex = 0;
            self.TitleId = 0;
            self.Drops.Clear();
            self.GatherStartTime = 0;
            self.GatherNumber = 0;
            self.GatherLastTime = 0;
            self.GatherCellIndex = 0;
            self.SkillEntityMoveType = 0;
            self.SkillEntityTrackTargetId = 0;
            self.SkillEntityStartTime = 0;
            self.AttackMode = 0;
            self.UnitCellIndex = 0;
        }

        public static int GetZhaoHuanNumber(this UnitInfoComponent self)
        {
            int number = 0;
            for (int i = self.ZhaohuanIds.Count - 1; i >= 0; i--)
            {
                Unit zhaohuan = self.DomainScene().GetComponent<UnitComponent>().Get(self.ZhaohuanIds[i]);
                if (zhaohuan == null)
                {
                    continue;
                }
                number++;
            }
            return number;
        }

    }
}
