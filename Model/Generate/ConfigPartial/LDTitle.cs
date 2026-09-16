using System.Collections.Generic;

namespace ET
{
    public partial class LDTitle
    {
        public List<AttributeItem> AttributeItems = new List<AttributeItem>();

        public override void EndInit()
        {
            this.AttributeItems.Clear();
            if (string.IsNullOrEmpty(this.Attribute))
            {
                return;
            }

            string[] attributeList = this.Attribute.Split('|');
            for (int i = 0; i < attributeList.Length; i++)
            {
                string raw = attributeList[i];
                if (string.IsNullOrEmpty(raw))
                {
                    continue;
                }

                string[] parts = raw.Split('~');
                if (parts.Length < 2)
                {
                    continue;
                }

                if (!int.TryParse(parts[0], out int key) || !long.TryParse(parts[1], out long value))
                {
                    continue;
                }

                this.AttributeItems.Add(new AttributeItem { AttributeID = key, AttributeValue = value });
            }
        }
    }

    public partial class LDTitleCategory
    {
        static readonly List<AttributeItem> EmptyAttribute = new List<AttributeItem>();

        public List<AttributeItem> GetTitleAttri(int titleId)
        {
            if (!this.Contain(titleId))
            {
                return EmptyAttribute;
            }

            return this.Get(titleId).AttributeItems;
        }
    }
}
