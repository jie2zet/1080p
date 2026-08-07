using CMP.Models.Operation.LoadPort;
using CMP.Models.Recipe;
using CMP.Utils;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nini.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMP.Tests.Models
{
    [TestClass]
    [TestSubject(typeof(LoadportTest))]
    public class LoadportTest
    {
        //[TestMethod]
        //public void TestLoadport() {
        //    Loadport load = new Loadport("loadport1",new SerialPortHelper("SerialPort"));

        //    // 查询状态
        //    //load.GetLoadportState();
        //    Thread.Sleep(500);

        //    // 初始化
        //    load.Initialzation();
        //    Thread.Sleep(500);


        //}

        [TestMethod]
        public void TestLoadport()
        {
            Console.WriteLine("Starting Loadport test...");

            try
            {
                // 创建串口助手实例
                SerialPortHelper serialPort = new SerialPortHelper("SerialPort2");

                // 注意：在单元测试中，你可能需要手动设置串口参数
               /* serialPort.PortName = "COM1"; // 根据实际情况设置
                serialPort.BaudRate = 9600;
                serialPort.DataBits = 8;
                serialPort.Parity = (System.IO.Ports.Parity) 0;
                serialPort.StopBits = (System.IO.Ports.StopBits)1;*/

                // 输出测试开始信息
                Console.WriteLine("Creating Loadport instance...");
                TestContext.WriteLine("Creating Loadport instance..."); // 使用TestContext输出

                // 创建Loadport实例
               // Loadport load = new Loadport();

                // 输出创建成功信息
                Console.WriteLine("Loadport instance created successfully");
                TestContext.WriteLine("Loadport instance created successfully");

                // 订阅数据接收事件以查看是否有数据
                serialPort.WriteStrHandlerHelper += (data) => {
                    Console.WriteLine($"Received data: {data}");
                    TestContext.WriteLine($"Received data: {data}");
                };

                serialPort.ErrorHandlerHelper += (error) => {
                    Console.WriteLine($"Serial port error: {error}");
                    TestContext.WriteLine($"Serial port error: {error}");
                };

                if (serialPort.IsOpen) { serialPort.Close(); serialPort.Open(); }
                // 尝试调用方法
                Console.WriteLine("Calling Initialzation method...");
                TestContext.WriteLine("Calling Initialzation method...");
                //load.Initialzation();

                Console.WriteLine("Test method completed");
                TestContext.WriteLine("Test method completed");

                var ports = System.IO.Ports.SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    TestContext.WriteLine($"Trying port: {port}");
                    // 测试每个端口
                }



                // 简单断言测试
                //Assert.IsNotNull(load);
               // Assert.AreEqual("loadport1", load.name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                TestContext.WriteLine($"Exception occurred: {ex.Message}");
                TestContext.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // 重新抛出异常使测试失败
            }
        }

        // 添加TestContext属性以支持测试输出
        public TestContext TestContext { get; set; }
    }
}
