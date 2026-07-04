using System;

namespace ET
{

    [ActorMessageHandler]
    public class Actor_OpenBoxHandler : AMActorLocationRpcHandler<Unit, Actor_OpenBoxRequest, Actor_OpenBoxResponse>
    {

        protected override async ETTask Run(Unit unit, Actor_OpenBoxRequest request, Actor_OpenBoxResponse response, Action reply)
        {
            Unit boxUnit = unit.GetParent<UnitComponent>().Get(request.UnitId);
            if (boxUnit == null)
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }
            if (boxUnit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
            {
                response.Error = ErrorCode.ERR_Success;
                reply();
                return;
            }
            int monsterid = boxUnit.ConfigId;
            LDMonster ldMonster = LDMonsterCategory.Instance.Get(monsterid);
            string itemneeds = "";
          
            if (itemneeds.Length >2 && !unit.GetComponent<BagComponentServer>().OnCostItemData(itemneeds, ItemLocType.ItemLocBag, ItemGetWay.ItemBox_6))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            /*if (ldMonster.MonsterSonType == 57) 
            {
                //背包是否满
                if (unit.GetComponent<BagComponentServer>().IsBagFull())
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }

                //宠物已满
                if (unit.GetComponent<PetComponent>().PetIsFull())
                {
                    response.Error = ErrorCode.ERR_PetIsFull;
                    reply();
                    return;
                }
            }*/

            boxUnit.GetComponent<UnitLifeComponent>()?.OnDead(unit);

            unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.OpenBox_137, 0, 1);

            response.Error = ErrorCode.ERR_Success;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
