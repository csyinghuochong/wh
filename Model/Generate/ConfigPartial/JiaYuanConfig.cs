using System.Collections.Generic;

namespace ET
{
    public partial class JiaYuanConfigCategory
    {

        public Dictionary<int, Dictionary<int, int>> JiaYuanProMax = new Dictionary<int, Dictionary<int, int>>();


        public int GetProMax(int jiayuanlv, int keyid)
        {
            if (!JiaYuanProMax.ContainsKey(jiayuanlv))
            {
                return 0;
            }
            if (!JiaYuanProMax[jiayuanlv].ContainsKey(keyid))
            {
                return 0;
            }
            return JiaYuanProMax[jiayuanlv][keyid];
        }

        public override void AfterEndInit()
        {
            foreach (JiaYuanConfig functionConfig in this.GetAll().Values)
            {
   
                if (!JiaYuanProMax.ContainsKey(functionConfig.Id))
                {
                    Dictionary<int, int> keyValuePairs = new Dictionary<int, int>() { };

                    JiaYuanProMax.Add(functionConfig.Id, keyValuePairs);
                    string proMax = functionConfig.ProMax;
                    string[] prolist = proMax.Split(';');

                    for (int i = 0; i < prolist.Length; i++)
                    {
                        if (string.IsNullOrEmpty(prolist[i]))
                        {
                            continue;
                        }
                        string[] proinfo = prolist[i].Split(',');
                        if (proinfo.Length < 2)
                        {
                            continue;
                        }

                        if (!int.TryParse(proinfo[0], out int key))
                        {
                            Log.Error($"int.TryParse error: {proinfo[0]} jiaYuanId:{functionConfig.Id}");
                            continue;
                        }

                        if (!int.TryParse(proinfo[1], out int val))
                        {
                            Log.Error($"int.TryParse error: {proinfo[1]} jiaYuanId:{functionConfig.Id}");
                            continue;
                        }

                        if (!keyValuePairs.ContainsKey(key))
                        {
                            keyValuePairs.Add( key, val );
                        }
                    }
                }

            }
        }
    }
}
