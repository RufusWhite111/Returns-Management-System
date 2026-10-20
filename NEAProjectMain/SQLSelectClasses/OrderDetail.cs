using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAProjectMain.SQLSelectClasses
{
    public class IndividualOrderDetails
    {
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OriginalPaymentMethod { get; set; }
        public List<ItemInOrder> Items { get; set; } = new List<ItemInOrder>();
        public int TotalItemsInOrder => Items.Count;
    }
}
