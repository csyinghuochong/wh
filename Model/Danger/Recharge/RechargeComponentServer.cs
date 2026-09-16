using System.Collections.Generic;

namespace ET
{
    public class RechargeComponentServer : Entity, IAwake, ITransfer, IUnitCache, IDeserialize
    {

        public RechargePro RechargePro = new RechargePro();
    }
}
