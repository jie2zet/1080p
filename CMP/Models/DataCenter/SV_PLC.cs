using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.DataCenter
{
    public class SV_PLC : SV
    {
        public string PLCType { get; set; } = string.Empty;

        public String PLCLink { get; set; } = string.Empty;
    }
}
