using System;
using System.Collections.Generic;
using System.Text;
using CMP;
using CMP.Constants;

namespace CMP.Models.Resource
{
    public class WaferSequenceModel
    {
        public string? ChamberName { get; set; }
        /// <summary>
        /// 代表Chamber所在顺序是第几步
        /// </summary>
        public int ChamberSequnceStep { get; set; }
        /// <summary>
        /// 此chamber的步骤代表值
        /// </summary>
        public int ChamberStepValue
        {
            get { return 1 << ChamberExponentMap.ChamberExponentMapping[ChamberName!]; }
        }

        public WaferSequenceModel()
        {           
        }
    }
}
