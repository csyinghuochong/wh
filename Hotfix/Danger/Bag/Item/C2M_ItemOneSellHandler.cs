using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemOneSellHandler : AMActorLocationRpcHandler<Unit, C2M_ItemOneSellRequest, M2C_ItemOneSellResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemOneSellRequest request, M2C_ItemOneSellResponse response, Action reply)
        {
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            long sellGold = 0;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();

            for (int i = 0; i < request.BagInfoIds.Count; i++)
            {
                BagInfo useBagInfo = bagComponentServer.GetItemByLoc((ItemLocType)request.OperateType, request.BagInfoIds[i]);
                if (useBagInfo == null)
                {
                    continue;
                }
                LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);


                //默认出售全部
                //给与对应金币或货币奖励
                List<int> gemids = useBagInfo.GemIdList;
                List<int> gemIdList = new List<int>();
                for (int gem = 0; gem < gemids.Count; gem++)
                {
                    if (gemids[gem] == 0)
                    {
                        continue;
                    }
                    int gemId = gemids[gem];
                    gemIdList.Add(gemId);
                    LDItem ldItemConf = LDItemCategory.Instance.Get(gemId);
                    //unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData((int)ldItemConf.SellMoneyType, (ldItemConf.SellMoneyValue).ToString());
                }

                //珍宝属性价格提升
                /*int sellValue = ldItem.SellMoneyValue;
                if (useBagInfo.HideSkillLists.Contains(68000102))
                {
                    sellValue = ldItem.SellMoneyValue * 20;
                }

                if (ldItem.SellMoneyType == UserDataType.Gold)
                {
                    sellGold += (useBagInfo.ItemNum * sellValue);
                    unit.GetComponent<BagComponentServer>().OnCostItemData(useBagInfo, (ItemLocType)request.OperateType, useBagInfo.ItemNum);
                }
                else
                {
                    unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData((int)ldItem.SellMoneyType, (useBagInfo.ItemNum * sellValue).ToString(), true, 39);
                    unit.GetComponent<BagComponentServer>().OnCostItemData(useBagInfo, (ItemLocType)request.OperateType, useBagInfo.ItemNum);
                }*/
                if (useBagInfo.ItemNum == 0)
                {
                    m2c_bagUpdate.BagInfoDelete.Add(useBagInfo);
                }
                else
                {
                    m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                }
            }
            if (sellGold > 0)
            {
                roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, sellGold.ToString(), true, 39);
            }

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
