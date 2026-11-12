using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_FashionWearHandler : AMActorLocationRpcHandler<Unit, C2M_FashionWearRequest, M2C_FashionWearResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FashionWearRequest request, M2C_FashionWearResponse response, Action reply)
        {

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            int fashionId = FashionHelper.ResolveFashionId(request.FashionId);

            // 只允许 1穿戴 / 2卸下，非法值不能落到 else 里当卸下处理
            if (request.OperatateType != 1 && request.OperatateType != 2)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            // 持有状态统一走 GetState：未拥有、已过期都不允许操作，有激活记录不等于有效
            if (FashionHelper.GetState(bagComponentServer.FashionActiveList, fashionId) != FashionState.Owned)
            {
                Log.Error($"C2M_FashionWearRequest.1 FashionId={fashionId}");
                response.Error = ErrorCode.ERR_FashionNoActive;
                reply();
                return;
            }

            bool worn = FashionHelper.IsWorn(bagComponentServer.FashionEquipList, fashionId);

            if (request.OperatateType == 1)
            {
                if (worn)
                {
                    response.Error = ErrorCode.ERR_EquipRepeat;
                    reply();
                    return;
                }

                if (!LDFashionCategory.Instance.Contain(fashionId))
                {
                    Log.Error($"C2M_FashionWearRequest.2 FashionId={fashionId}");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                LDFashion ldFashion = LDFashionCategory.Instance.Get(fashionId);
                if (ldFashion.Part <= 0)
                {
                    Log.Error($"C2M_FashionWearRequest.3 FashionId={fashionId}");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                int roleSex = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Sex;
                if (ldFashion.Sex != SexEnum.None && ldFashion.Sex != roleSex)
                {
                    Log.Error($"C2M_FashionWearRequest.4 FashionId={fashionId}");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                for (int i = bagComponentServer.FashionEquipList.Count - 1; i >= 0; i--)
                {
                    int wearId = bagComponentServer.FashionEquipList[i];
                    if (!LDFashionCategory.Instance.Contain(wearId))
                    {
                        bagComponentServer.FashionEquipList.RemoveAt(i);
                        continue;
                    }

                    if (LDFashionCategory.Instance.Get(wearId).Part == ldFashion.Part)
                    {
                        bagComponentServer.FashionEquipList.RemoveAt(i);
                    }
                }

                bagComponentServer.FashionEquipList.Add(fashionId);
            }
            else
            {
                // 没穿戴就不允许卸下，先判再删，避免静默 Remove 失败走成「网络错误」
                if (!worn)
                {
                    Log.Error($"C2M_FashionWearRequest.5 FashionId={fashionId}");
                    response.Error = ErrorCode.ERR_FashionNoWear;
                    reply();
                    return;
                }

                bagComponentServer.FashionEquipList.Remove(fashionId);
            }

            UnitHelper.BroadcastUnitAppear(unit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
