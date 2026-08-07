using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using CMP.Constants;
using CMP.Models.Config;
using CMP.Services.Plc;
using CMP.Utils;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HslCommunication.Profinet.Melsec;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nini.Config;

namespace CMP.Tests.Services;

[TestClass]
//[TestSubject(typeof(PlcService))]
public class PlcTest
{
    //private  _plcService = PlcService.Instance;

    [TestMethod]

    public void Method()
    {
        var ipAddr = "192.168.2.50";
        var port = 7002;
        MitsubishiPlcService mc = MitsubishiPlcService.Main;
       bool isConnect =  mc.ConnectPlc(ipAddr, port);
        //short a = mc.ReadShortVari("B00");
       // mc.WriteBoolVari("B0B",true);
       // Console.WriteLine(a+"-a111a0000000000000");
        ScanRoboticArmStatuses(mc);
        // ObservableCollection<ConfigModel> list =  ReadRobotConfig();


        //_plcService.Initialize(PLCAddress.IP_ADDRESS, PLCAddress.PORT, "PlcLog.txt");

        //var result = _plcService.Connect(ipAddr, port);
        //Console.WriteLine($@"Connect result: {result.IsSuccess}, Message: {result.Message}");

        //// read a variable
        //var isRunningAddr = "M5960";
        //var readResult = _plcService.Read<bool>(isRunningAddr);
        //Console.WriteLine($@"Read result: {readResult.IsSuccess}, Value: {readResult.Content}, Message: {readResult.Message}");

        //// write a variable
        //// axis1 ServOn addr
        //var axis1ServOnAddr = "M1000";
        //var writeResult = _plcService.Write(axis1ServOnAddr, true);
        //Console.WriteLine($@"Write result: {writeResult.IsSuccess}, Message: {writeResult.Message}");

        //// dynamic string type write
        //var dynamicStrTypeWriteResult = _plcService.Write("bool", axis1ServOnAddr, true);
        //Console.WriteLine($@"Dynamic string type write result: {dynamicStrTypeWriteResult.IsSuccess}, Message: {dynamicStrTypeWriteResult.Message}");

        //// disconnect from PLC
        //_plcService.Disconnect();

        //// connect plc
        //result = _plcService.Connect(ipAddr, port);
        //Console.WriteLine($@"Re-connect result: {result.IsSuccess}, Message: {result.Message}");

        // PlcAddress.All.Initialize

    }

    private void ScanRoboticArmStatuses(MitsubishiPlcService mc)
    {
        const int roboticArmCount = 15;
        const int baseAddress = 6152;
        const int addressOffsetPerArm = 48;

        // ?????: 1-Initialized, 3-Running, 5-Stopped, 7-Empty, 9-Alarm, 11-Error, 13-Warning, 15-Manual
        //var offsetsToRead = new[] { 1, 3, 5, 7, 9, 11, 13, 15 };
        //var statusNames = new[] { "Initialized", "Running", "Stopped", "Empty", "Alarm", "Error", "Warning", "Manual" };

        Console.WriteLine($"??????{roboticArmCount}??????????...");
        Console.WriteLine($"??????: M{baseAddress}, ??????????????: {addressOffsetPerArm}");
        Console.WriteLine($"???????: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

        Stopwatch stopwatch = Stopwatch.StartNew();
        MelsecMcNet melsec_net = new MelsecMcNet();
        melsec_net.IpAddress = "192.168.2.50";
        melsec_net.Port = 7002;
       var isSuccess =  melsec_net.ConnectServer();
        Console.WriteLine(  "---------"+ isSuccess);

        for (int armIndex = 0; armIndex < roboticArmCount; armIndex++)
        {
            string detectedStatus = "Unknown";
            bool statusFound = false;
            var addressOffset = addressOffsetPerArm * armIndex ;
            var address = $"M{baseAddress + addressOffset}";
            Console.WriteLine($"\n--- ?????? {armIndex + 1} ---");



            //for (int i = 0; i < 48; i++)
            //{
            //    var offset = i;
            //    var addressOffset = addressOffsetPerArm * armIndex + offset;
            //    var address = $"M{baseAddress + addressOffset}";

            //    try
            //    {
            //        bool value = mc.ReadBoolVari(address);
            //        Console.WriteLine($"?????? {address}: {value}");

            //        //if (value)
            //        //{
            //            //detectedStatus = statusNames[i];
            //            statusFound = true;
            //            Console.WriteLine($"?????? {armIndex + 1} ??: {value}");
            //           // break; // ??????true????????????????????????
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"????????? {armIndex + 1} ??? {address} ???: {ex.Message}");
            //    }
            //}
            var result = melsec_net.ReadBool(address, 48);
            var operate = result;
            if (operate.IsSuccess) {
                foreach (var item in operate.Content)
                {
                    Console.WriteLine($"address:{address}--{item}");
                }
            }

        }

        stopwatch.Stop();

        Console.WriteLine($"\n=== ?????? ===");
        Console.WriteLine($"????????: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"????: {stopwatch.ElapsedMilliseconds} ????");
        Console.WriteLine($"??????????????: {(double)stopwatch.ElapsedMilliseconds / roboticArmCount:F2} ????");
        Console.WriteLine($"????????????: {stopwatch.ElapsedMilliseconds / (roboticArmCount * 48)} ????");
    }
    private readonly string _baseConfigPath = "D:\\CMP\\Configuration\\Configs\\";
    private ObservableCollection<ConfigModel> ReadRobotConfig()
    {
        ConfigModel config = null;

        ObservableCollection<ConfigModel> robotConfigs = new ObservableCollection<ConfigModel>();
        for (int address = 10000; address <= 10009; address++)
        {
            config = new ConfigModel()
            {
                ECID = 0,
                Name = $"ZR{address}",
                SetValue = GlobalService.plcComm.ReadIntVari($"R{address}").Content + ""

            };
            Console.WriteLine(GlobalService.plcComm.ReadFloatVari($"R{address}").Content);
            Console.WriteLine(GlobalService.plcComm.ReadDoubleVari($"R{address}").Content);
            Console.WriteLine(GlobalService.plcComm.ReadLongVari($"R{address}").Content);
            Console.WriteLine(GlobalService.plcComm.ReadIntVari($"R{address}").Content);
            robotConfigs.Add(config);
        }


        //DirectoryInfo directoryInfo = new DirectoryInfo(_baseConfigPath);
        //var fileName = directoryInfo.GetFiles("RobotConfig.json");
        // ?????????JSON???
        //string json = JsonConvert.SerializeObject(robotConfigs, Formatting.Indented);
       // FileUtil.WriteFile(fileName[0].FullName, json);
        return robotConfigs;
    }

}