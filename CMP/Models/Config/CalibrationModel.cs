using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Config
{
    public class CalibrationModel:BindableBase
    {
		private float set;

		public float Set { get => set; set => SetProperty(ref set, value); }

        private float read;

        public float Read { get => read; set => SetProperty(ref read, value); }

        private float act;

        public float Act { get => act; set => SetProperty(ref act, value); }

    }
}
