using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_FashionWearHandler : AMActorLocationRpcHandler<Unit, C2M_FashionWearRequest, M2C_FashionWearResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FashionWearRequest request, M2C_FashionWearResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (!bagComponentServer.FashionActiveIds.Contains(request.FashionId))
            {
                Log.Error($"C2M_FashionWearRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            int occ = roleInfoComponentServer.RoleInfo.Occ;
            LDFashion ldFashion = LDFashionCategory.Instance.Get(request.FashionId);

            bool canwear = false;
           
            if (!canwear)
            {
                Log.Error($"C2M_FashionWearRequest.2");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (request.OperatateType == 1)
            {
                if (bagComponentServer.FashionEquipList.Contains(request.FashionId))
                {
                    response.Error = ErrorCode.ERR_AlreadyLearn;
                    reply();
                    return;
                }

                for (int  i = bagComponentServer.FashionEquipList.Count - 1; i >= 0 ; i--)
                {
                    LDFashion fashion2 = LDFashionCategory.Instance.Get(bagComponentServer.FashionEquipList[i]);
                 
                }

                bagComponentServer.FashionEquipList.Add(request.FashionId);
            }
            else
            {
                if (!bagComponentServer.FashionEquipList.Contains(request.FashionId))
                {
                    response.Error = ErrorCode.ERR_NetWorkError;
                    reply();
                    return;
                }
                bagComponentServer.FashionEquipList.Remove(request.FashionId);
            }

            M2C_FashionUpdate m2C_FashionUpdate = new M2C_FashionUpdate();
            m2C_FashionUpdate.UnitID = unit.Id;
            m2C_FashionUpdate.FashionEquipList = bagComponentServer.FashionEquipList;
            MessageHelper.Broadcast(unit, m2C_FashionUpdate);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
