using System.Collections.Generic;

namespace ET
{
    public class UnitTransferBuffer
    {
        public long TransferId;
        public int ChunkCount;
        public int TotalBytes;
        public int ReceivedBytes;
        public long ExpireTime;
        public List<byte[]> Chunks = new List<byte[]>();
    }
}
