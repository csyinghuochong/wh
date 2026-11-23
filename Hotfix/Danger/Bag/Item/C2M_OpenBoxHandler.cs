using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_OpenBoxHandler : AMActorLocationRpcHandler<Unit, C2M_OpenBoxRequest, M2C_OpenBoxResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_OpenBoxRequest request, M2C_OpenBoxResponse response, Action reply)
        {
            Unit boxUnit = unit.GetParent<UnitComponent>().Get(request.UnitId);
            if (boxUnit == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            NumericComponent boxNumeric = boxUnit.GetComponent<NumericComponent>();
            if (boxNumeric.GetAsInt(NumericType.Now_Dead) == 1)
            {
                response.Error = ErrorCode.ERR_Success;
                reply();
                return;
            }
            int monsterid = boxUnit.ConfigId;
            LDMonster ldMonster = LDMonsterCategory.Instance.Get(monsterid);
            string itemneeds = "";
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
          
            if (!string.IsNullOrEmpty(itemneeds) && itemneeds.Length > 2 && !bag.OnCostItemData(itemneeds, ItemLocType.ItemLocBag, ItemGetWay.ItemBox_6))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                reply();
                return;
            }

            /*if (ldMonster.MonsterSonType == 57) 
            {
                //背包是否满
                if (unit.GetComponent<BagComponentServer>().IsBagFull())
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                    reply();
                    return;
                }

                //宠物已满
                if (unit.GetComponent<PetComponent>().PetIsFull())
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_No_Volume);
                    reply();
                    return;
                }
            }*/

            boxUnit.GetComponent<UnitLifeComponent>()?.OnDead(unit);

            TaskComponentServer taskComponent = unit.GetComponent<TaskComponentServer>();
            taskComponent.OnOpenBox();


            response.Error = ErrorCode.ERR_Success;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
