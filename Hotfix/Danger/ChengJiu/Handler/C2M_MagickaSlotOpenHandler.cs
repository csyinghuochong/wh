using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MagickaSlotOpenHandler : AMActorLocationRpcHandler<Unit, C2M_MagickaSlotOpenRequest, M2C_MagickaSlotOpenResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_MagickaSlotOpenRequest request, M2C_MagickaSlotOpenResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();

            int curid = chengJiuComponentServer.GetCurrentMagickaSlotIdByPosition(request.Position);
            //if (curid > 0)
            //{
            //    response.Error = ErrorCode.ERR_AlreadyOpen;
            //    reply();
            //    return;
            //}

            int nexid = chengJiuComponentServer.GetNextMagickaSlotIdByPosition(request.Position);
            if (curid == nexid)
            {
                response.Error = ErrorCode.ERR_MagicMaxLevel;
                reply();
                return;
            }
            MagickaSlotConfig magickaSlotConfig = MagickaSlotConfigCategory.Instance.Get(nexid);

            int totallevel = chengJiuComponentServer.GetCurrentMagickaTotalLevel();
            if (totallevel < magickaSlotConfig.NeedTotalLevel)
            {
                response.Error = ErrorCode.ERR_MagicLevelNotEnough;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
           
            bool sucesss = bagComponentServer.OnCostItemData(magickaSlotConfig.OpenCostItem,ItemLocType.ItemLocBag, ItemGetWay.CostItem );
            if (!sucesss)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            chengJiuComponentServer.OnOpenMagicka(request.Position,  nexid);
            response.MagickaSlotIds = chengJiuComponentServer.MagickaSlotIdList;
            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}
