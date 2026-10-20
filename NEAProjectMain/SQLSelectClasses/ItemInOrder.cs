using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAProjectMain.SQLSelectClasses
{
    public class ItemInOrder
    {
        public string ProductName { get; set; }
        public string ProductColour { get; set; }
        public string ProductSize { get; set; }
        public string ReturnReason { get; set; }
        public bool IsExchange { get; set; }
    }
}
