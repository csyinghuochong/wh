using System;

namespace ET
{
    public class ActorMessageAttribute: System.Attribute
    {
        public ushort Opcode { get; private set; }

        public ActorMessageAttribute(ushort opcode)
        {
            this.Opcode = opcode;
        }
    }
}