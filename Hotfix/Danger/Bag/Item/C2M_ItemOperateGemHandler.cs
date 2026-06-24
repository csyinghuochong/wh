using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemOperateGemHandler : AMActorLocationRpcHandler<Unit, C2M_ItemOperateGemRequest, M2C_ItemOperateGemResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ItemOperateGemRequest request, M2C_ItemOperateGemResponse response, Action reply)
        {
            long bagInfoID = request.OperateBagID;
            BagComponentServer bagcComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bagcComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, bagInfoID);
            if (useBagInfo == null)
            {
                useBagInfo = bagcComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip, bagInfoID);
            }
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemUseError;
                reply();
                return;
            }

            if (bagcComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            // 通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            //通知客户端背包道具发生改变
            m2c_bagUpdate.BagInfoUpdate = new List<BagInfo>();

            LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);

            //镶嵌宝石
            if (request.OperateType == 9)
            {
                //宝石镶嵌
                string[] geminfos = request.OperatePar.Split('_');
                long equipid = long.Parse(geminfos[0]);
                int gemIndex = int.Parse(geminfos[1]);

                BagInfo equipInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocEquip, equipid);

                //获取装备baginfo
                if (equipInfo == null)
                {
                    equipInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocBag, equipid);
                }
                if (equipInfo == null)
                {
                    Log.Warning($"equipInfo == null {equipid}");
                    reply();
                    return;
                }

                //判断孔位是否相符
                string[] equipGeminfos = equipInfo.GemHole.Split('_');

                if (string.IsNullOrEmpty(equipInfo.GemHole) || equipInfo.GemHole.Equals(ItemHelper.DefaultGem))
                {
                    response.Error = ErrorCode.ERR_ItemUseError;
                    reply();
                    return;
                }

                if (equipGeminfos[gemIndex] != ldItem.ItemType.ToString() && ldItem.ItemType != 110 && ldItem.ItemType != 111)
                {
                    response.Error = ErrorCode.ERR_ItemUseError;
                    reply();
                    return;
                }

                //史诗宝石最多镶嵌4个
                if (ldItem.ItemType == 110) 
                {
                    int equipShiShiGemNum = 0;
                    bool isTihuan = false;
                    List<BagInfo> EquipList = unit.GetComponent<BagComponentServer>().EquipList;
                    for (int i = 0; i < EquipList.Count; i++)
                    {
                        string[] gemList = EquipList[i].GemIDNew.Split('_');
                        for (int y = 0; y < gemList.Length; y++) 
                        {
                            if (CommonHelper.IfNull(gemList[y]) == false)
                            {
                                LDItem gemLdItemCof = LDItemCategory.Instance.Get(int.Parse(gemList[y]));
                                if (gemLdItemCof.ItemType == 110)
                                {
                                    equipShiShiGemNum += 1;
                                }
                            }
                            if (EquipList[i].BagInfoID == equipid && gemIndex == y)
                            {
                                isTihuan = true;
                                break;
                            }
                        }
                    }

                    if (!isTihuan && equipShiShiGemNum >= 4)
                    {
                        response.Error = ErrorCode.ERR_GemShiShiNumFull;
                        reply();
                        return;
                    }
                }


                string[] gemIdList = equipInfo.GemIDNew.Split('_');
                gemIdList[gemIndex] = useBagInfo.ItemID.ToString();
                string gemIDNew = "";
                for (int i = 0; i < gemIdList.Length; i++)
                {
                    gemIDNew = gemIDNew + gemIdList[i] + "_";
                }
                equipInfo.GemIDNew = gemIDNew.Substring(0, gemIDNew.Length - 1);
                equipInfo.isBinging = true;
                m2c_bagUpdate.BagInfoUpdate.Add(equipInfo);
                //消耗宝石
                unit.GetComponent<BagComponentServer>().OnCostItemData(useBagInfo.BagInfoID, 1);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            }

            //卸下宝石
            if (request.OperateType == 10)
            {
                if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < 1)
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }

                int gemIndex = int.Parse(request.OperatePar);
                string[] gemIdList = useBagInfo.GemIDNew.Split('_');
                int gemItemId = int.Parse(gemIdList[gemIndex]);

                //类型110的不能卸
                if (!LDItemCategory.Instance.Contain(gemItemId))
                {
                    response.Error = ErrorCode.ERR_GemNoError;
                    reply();
                    return;
                }
                LDItem gemLdItemConfig = LDItemCategory.Instance.Get(gemItemId);
                if (gemLdItemConfig.ItemType == 110)
                {
                    response.Error = ErrorCode.ERR_GemNoError;
                    reply();
                    return;
                }

                gemIdList[gemIndex] = "0";
                string gemIDNew = "";
                for (int i = 0; i < gemIdList.Length; i++)
                {
                    gemIDNew = gemIDNew + gemIdList[i] + "_";
                }
                useBagInfo.GemIDNew = gemIDNew.Substring(0, gemIDNew.Length - 1);
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);

                //回收宝石
                if (gemItemId != 0)
                {
                    bool ret =   unit.GetComponent<BagComponentServer>().OnAddItemData($"{gemItemId};1", $"{ItemGetWay.GemHuiShou}_{TimeHelper.ServerNow()}");
                    Function_Fight.UnitUpdateProperty_Base(unit, true, true);

                    if (!ret)
                    {
                        Log.Error($"回收宝石出错: {unit.Id} {gemItemId}");
                    }
                }
            }
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
