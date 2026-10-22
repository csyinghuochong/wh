using System;
using System.Collections.Generic;

namespace ET
{
    public static class RoleNoHelper
    {
        public const int PlayerNoStart = 10000001;
        public const int PlayerNoSpan = 1000000000;
        public const int ServerIdMax = 9999;

        static readonly object SyncRoot = new object();
        static readonly Dictionary<int, int> nextOffset = new Dictionary<int, int>();
        static readonly long epoch2026;

        static RoleNoHelper()
        {
            long epoch1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            epoch2026 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970;
        }

        public static long Allocate(int serverId)
        {
            if (serverId <= 0 || serverId > ServerIdMax)
            {
                return 0;
            }

            int second = (int)((TimeHelper.ServerNow() - epoch2026) / 1000);
            if (second < 0)
            {
                second = 0;
            }

            lock (SyncRoot)
            {
                int candidate = second;
                if (nextOffset.TryGetValue(serverId, out int offset) && second <= offset)
                {
                    candidate = offset + 1;
                }
                if (candidate >= PlayerNoSpan - PlayerNoStart)
                {
                    return 0;
                }

                nextOffset[serverId] = candidate;
                int playerNo = PlayerNoStart + candidate;
                return (long)serverId * PlayerNoSpan + playerNo;
            }
        }
    }
}
