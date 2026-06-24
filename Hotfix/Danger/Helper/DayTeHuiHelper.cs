using System;
using System.Collections.Generic;

namespace ET
{
    public static class DayTeHuiHelper
    {

        public static List<int> GetDayTeHuiList(int activityType, int level)
        { 
            List<int> sour = new List<int>();
            List<int> dest = new List<int>();
            //2
          
            RandomHelper.GetRandListByCount(sour, dest, 4);
            return dest;
        }

    }
}
