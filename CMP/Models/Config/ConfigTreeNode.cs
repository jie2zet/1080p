using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Config
{
    public class ConfigTreeNode: BindableBase
    {
		private string? name;

		public string? Name
		{
			get { return name; }
			set { name = value; RaisePropertyChanged(); }
		}

		public ObservableCollection<ConfigTreeNode>? Children { get; set; }

    }
}
