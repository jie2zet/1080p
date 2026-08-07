using CMP.Models.DataCenter;
using CMP.Services.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Resource
{
    public class Robot
    {
        string robotName = string.Empty;
        public List<SV_PLC> listDataInSingleRobot;
        MitsubishiPlcService mcBinaryPlcComm = MitsubishiPlcService.Main;

        public Robot(string name)
        {
            this.robotName = name;
            listDataInSingleRobot = new List<SV_PLC>();
           
        }

        public void RefreshRobotData()  //更新单个Robot
        {
            for (int i = 0; i < listDataInSingleRobot.Count; i++)
            {
                if (listDataInSingleRobot[i].Type == "Bool")
                {
                    listDataInSingleRobot[i].Value = mcBinaryPlcComm.ReadBoolVari(listDataInSingleRobot[i].PLCLink);
                }
                if (listDataInSingleRobot[i].Type == "INT_16")
                {
                    listDataInSingleRobot[i].Value = mcBinaryPlcComm.ReadShortVari(listDataInSingleRobot[i].PLCLink);
                }
            }
        }

        public void RefreshSingleRobotData(int index)
        {
            if (listDataInSingleRobot[index].Type == "Bool")
            {
                listDataInSingleRobot[index].Value = mcBinaryPlcComm.ReadBoolVari(listDataInSingleRobot[index].PLCLink);
            }
            if (listDataInSingleRobot[index].Type == "INT_16")
            {
                listDataInSingleRobot[index].Value = mcBinaryPlcComm.ReadShortVari(listDataInSingleRobot[index].PLCLink);
            }
        }

        public void WriteSingleRobotData(int index, string value)
        {
            if (listDataInSingleRobot[index].Type == "Bool")
            {
                mcBinaryPlcComm.WriteBoolVari(listDataInSingleRobot[index].PLCLink, Convert.ToBoolean(value));
            }
            if (listDataInSingleRobot[index].Type == "INT_16")
            {
                mcBinaryPlcComm.WriteShortVari(listDataInSingleRobot[index].PLCLink, short.Parse(value));
            }
        }
    }
}
