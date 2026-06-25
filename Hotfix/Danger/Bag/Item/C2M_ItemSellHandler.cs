using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    
    /// <summary>
    /// 出售道具
    /// </summary>
    [ActorMessageHandler]
    public class C2M_ItemSellHandler: AMActorLocationRpcHandler<Unit, C2M_ItemSellRequest, M2C_ItemSellResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemSellRequest request, M2C_ItemSellResponse response, Action reply)
        {

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo useInfo = roleInfoComponentServer.RoleInfo;
            long bagInfoID = request.OperateBagID;

            ItemLocType locType = ItemLocType.ItemLocBag;
          
            BagInfo useBagInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(locType, bagInfoID);
            if (useBagInfo == null )
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            int sellNum = request.SellNum;
            if (sellNum <= 0 || sellNum > useBagInfo.ItemNum)
            {
                Log.Error($"C2M_ItemOperateHandler 3");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            
            
            string[] gemids = useBagInfo.GemIDNew.Split('_');
            LDItem ldItemConf = null;
            for (int i = 0; i < gemids.Length; i++)
            {
                if (gemids[i] == "0")
                {
                    continue;
                }
                ldItemConf = LDItemCategory.Instance.Get(int.Parse(gemids[i]));
                //unit.GetComponent<RoleInfoComponent>().UpdateRoleData((int)ldItemConf.SellMoneyType, (ldItemConf.SellMoneyValue).ToString());
            }

            useBagInfo.ItemNum -= request.SellNum;
            
            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            
            if (useBagInfo.ItemNum <= 0)
            {
                m2c_bagUpdate.BagInfoDelete.Add(useBagInfo);
            }
            else
            {
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }
            
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            
            await ETTask.CompletedTask;
        }
    }
}