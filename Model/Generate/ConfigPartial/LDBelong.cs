using System.Collections.Generic;

namespace ET
{
    public partial class LDBelongCategory
    {
        private readonly Dictionary<int, List<int>> childrenDict = new Dictionary<int, List<int>>();

        public override void AfterEndInit()
        {
            this.childrenDict.Clear();

            foreach (LDBelong config in this.GetAll().Values)
            {
                if (config.Belong_Id <= 0)
                {
                    continue;
                }

                if (!this.childrenDict.TryGetValue(config.Belong_Id, out List<int> children))
                {
                    children = new List<int>();
                    this.childrenDict.Add(config.Belong_Id, children);
                }

                children.Add(config.Id);
            }

            foreach (List<int> children in this.childrenDict.Values)
            {
                children.Sort((a, b) => this.Get(a).Order_SL.CompareTo(this.Get(b).Order_SL));
            }
        }

        public int[] GetChildIds(int belongId)
        {
            if (!this.childrenDict.TryGetValue(belongId, out List<int> children) || children.Count == 0)
            {
                return System.Array.Empty<int>();
            }

            return children.ToArray();
        }
    }
}
