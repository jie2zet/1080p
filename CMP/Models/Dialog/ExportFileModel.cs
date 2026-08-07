using CMP.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Dialog
{
    public class ExportFileModel:BindableBase
    {
        private string _fileName = string.Empty;
        public string FileName { get => _fileName; set =>SetProperty(ref _fileName, value); }

        private string _directory = string.Empty;
        public string Directory { get => _directory; set => SetProperty(ref _directory, value); }

        private ExportWay _selectedExportWay = ExportWay.All;

        public ExportWay SelectedExportWay { get => _selectedExportWay; set => SetProperty(ref _selectedExportWay, value); }
    }
}
