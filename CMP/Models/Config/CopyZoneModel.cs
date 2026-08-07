using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.ViewModels.Tabs.ConfigTab
{
    public class CopyZoneModel:BindableBase
    {
		private string? name;

		public string? Name
        {
			get { return name; }
			set { name = value; RaisePropertyChanged(); }
		}


		private bool isCopy;

		public bool IsCopy
        {
			get { return isCopy; }
			set { isCopy = value; RaisePropertyChanged(); }
		}

	}
}
