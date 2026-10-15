using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.HostCommunication
{
    public class ECModel
    {
        public int ECID { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string SetValue { get; set; }
        public string NewValue { get; set; }
        public string DefaultValue { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Class { get; set; }
        public int TemplateType { get; set; }
        public List<string> Option { get; set; }
        public string Discription { get; set; }
        public string PLCLink { get; set; }
        public string SheetName { get; set; }
        public uint Words { get; set; }

    }
}
