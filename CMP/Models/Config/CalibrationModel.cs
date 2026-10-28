using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CMP.Models.Config
{
    public class CalibrationModel:BindableBase
    {
        private bool isAutoCalibrationActive;

        /// <summary>
        /// 自动校准当前正在采集的行（仅用于界面高亮，不写入校准文件）。
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsAutoCalibrationActive
        {
            get => isAutoCalibrationActive;
            set => SetProperty(ref isAutoCalibrationActive, value);
        }

		private float set;

		public float Set { get => set; set => SetProperty(ref set, value); }

        private float read;

        public float Read { get => read; set => SetProperty(ref read, value); }

        private float act;

        public float Act { get => act; set => SetProperty(ref act, value); }

    }
}
