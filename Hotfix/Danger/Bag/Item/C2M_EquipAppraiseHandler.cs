using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_EquipAppraiseHandler : AMActorLocationRpcHandler<Unit, C2M_EquipAppraiseRequest, M2C_EquipAppraiseResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_EquipAppraiseRequest request, M2C_EquipAppraiseResponse response, Action reply)
        {

            await ETTask.CompletedTask;

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

            BagInfo baginfoOpera  = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip, request.OperateItemID);
            if(baginfoOpera == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            BagInfo baginfoCost = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.CostItemId);
            if (baginfoCost == null)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            //道具鉴定，扣除道具
            bagComponentServer.OnCostItemData(request.CostItemId, 1);


            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
         

            LDItem appraise = LDItemCategory.Instance.Get(baginfoCost.ItemID);
            int appraiseLevel = appraise.GetTypeParam1();


            LDEquip lDEquip = LDEquipCategory.Instance.Get(baginfoOpera.ItemID);
            int appraiseLv = lDEquip.Appraise_Lv;
            int[] appraise_Attribute = lDEquip.Appraise_Attribute;


            if (appraise_Attribute == null)
            {
                appraise_Attribute = new int[1];
                appraise_Attribute[0] = 56;
            }

            int randomAttri = 56;
            appraiseLv = 10;
            baginfoOpera.AppraiseAttrList.Clear();

            foreach (LDEquip_Appraise lDEquip_Appraise in LDEquip_AppraiseCategory.Instance.GetAll().Values)
            {
                if (lDEquip_Appraise.Attribute_Type == randomAttri 
                    && lDEquip_Appraise.Appraise_Lv == appraiseLv)
                {
                    int randomAttriValue = RandomHelper.RandomNumber(lDEquip_Appraise.Attribute_Min, lDEquip_Appraise.Attribute_Max);
                    baginfoOpera.AppraiseAttrList.Add( new AttributeItem()
                    {
                        AttributeID = randomAttri,
                        AttributeValue = randomAttriValue   
                    });
                }
            }
            
            m2c_bagUpdate.BagInfoUpdate.Add(baginfoOpera);

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
        }
    }


}
