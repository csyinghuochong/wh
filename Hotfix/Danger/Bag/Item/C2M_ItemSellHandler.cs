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
            long bagInfoID = request.OperateBagID;
            ItemLocType locType = (ItemLocType)request.LocType;
            if (!ItemNewHelper.IsValideBagLoc((int)locType))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bag.GetItemByLoc(locType, bagInfoID);
            if (useBagInfo == null )
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }

            int sellNum = request.SellNum;
            if (sellNum <= 0 || sellNum > useBagInfo.ItemNum)
            {
                Log.Error($"C2M_ItemOperateHandler 3");
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }
            useBagInfo.BindOnOperate();
            useBagInfo.ItemNum -= request.SellNum;

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            if (useBagInfo.ItemNum <= 0)
            {
                m2c_bagUpdate.BagInfoDelete.Add(useBagInfo);
                bag.GetItemByLoc(locType).Remove(useBagInfo);
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
