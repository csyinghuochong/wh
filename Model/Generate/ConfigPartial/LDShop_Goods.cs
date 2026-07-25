using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDShop_GoodsCategory
    {

        private Dictionary<int, List<LDShop_Goods>> ShopGoodsList = new Dictionary<int, List<LDShop_Goods>>();
        private List<LDShop_Goods> EmptyList = new List<LDShop_Goods>();

        public override void AfterEndInit()
        {
            foreach (LDShop_Goods Item in this.GetAll().Values)
            {
                if (!ShopGoodsList.TryGetValue(Item.ShopId, out List<LDShop_Goods> goodlist))
                {
                    goodlist = new List<LDShop_Goods>();
                    ShopGoodsList.Add(Item.ShopId, goodlist);
                }
                
                goodlist.Add(Item);
            }
        }

        public List<LDShop_Goods> GetShopGoodsList(int shoplist)
        {
            ShopGoodsList.TryGetValue(shoplist, out List<LDShop_Goods> goodlist);
            if (goodlist == null)
            {
                return EmptyList;
            }

            return goodlist;
        }
    }
}