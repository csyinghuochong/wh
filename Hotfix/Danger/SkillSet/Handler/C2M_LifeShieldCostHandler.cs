using System;
using System.Collections.Generic;
using System.Linq;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_LifeShieldCostHandler : AMActorLocationRpcHandler<Unit, C2M_LifeShieldCostRequest, M2C_LifeShieldCostResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_LifeShieldCostRequest request, M2C_LifeShieldCostResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            int addExp =  0;
            List<long> bagidList = new List<long>();
        
            for (int i = 0; i < request.OperateBagID.Count; i++)
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID[i]);
                if (bagInfo == null)
                {
                    continue;
                }
                if (!CommonConfig.ItemAddShieldExp.TryGetValue(bagInfo.ItemID, out int addValue))
                {
                    continue;
                }
                if (addValue > 10) {
                    addValue = RandomHelper.NextInt((int)(addValue * 0.8f), (int)(addValue * 1.2f));
                }
                addExp += addValue * bagInfo.ItemNum;
                bagidList.Add(request.OperateBagID[i]);
            }
            response.AddExp = addExp;

            SkillSetComponentServer skillsetComponentServer = unit.GetComponent<SkillSetComponentServer>();

            //生命之盾必须要大于其他盾

            skillsetComponentServer.OnShieldAddExp(request.OperateType, addExp);

            //扣除装备
            bagComponentServer.OnCostItemData(bagidList, ItemLocType.ItemLocBag);

            Function_Fight.UnitUpdateProperty_Base(unit, true, true);

            response.ShieldList = skillsetComponentServer.LifeShieldList;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
