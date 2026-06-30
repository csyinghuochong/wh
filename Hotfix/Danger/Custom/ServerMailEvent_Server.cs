using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public class ServerMailEvent_Server : AEvent<EventType.ServerMail>
    {
        protected override void Run(EventType.ServerMail args)
        {
            args.MailScene.GetComponent<MailSceneComponent>().OnServerMail(args.Message);

        }
    }
}
