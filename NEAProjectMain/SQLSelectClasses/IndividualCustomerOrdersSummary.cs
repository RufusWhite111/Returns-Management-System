using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAProjectMain.SQLSelectClasses
{
    public class IndividualCustomerOrderSummary
    {
        public int TotalOrders { get; set; }
        public List<IndividualCustomerOrder> Orders { get; set; } = new();
    }
}
