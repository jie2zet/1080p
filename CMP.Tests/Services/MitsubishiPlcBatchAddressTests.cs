using CMP.Services.Plc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMP.Tests.Services;

[TestClass]
public class MitsubishiPlcBatchAddressTests
{
    [TestMethod]
    public async Task BatchReadAsync_MixedDZRAndM_SplitsByDeviceType()
    {
        var previousDebugMode = MitsubishiPlcService.DebugMode;
        try
        {
            using var plc = new MitsubishiPlcService();
            var items = new List<PlcBatchReadItemBase>
            {
                new PlcBatchReadItem<int>("D1000"),
                // 验证历史上显式标成 D、实际为 ZR 的配置也会按 ZR 处理。
                new PlcBatchReadItem<int>("ZR10000", addressType: PlcBatchAddressType.D),
                new PlcBatchReadItem<bool>("M1000")
            };

            var result = await plc.BatchReadAsync(items);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsTrue(items.All(item => item.IsSuccess));
        }
        finally
        {
            MitsubishiPlcService.DebugMode = previousDebugMode;
        }
    }

    [TestMethod]
    public void BatchRead_ZRAddress_IsCaseInsensitive()
    {
        var previousDebugMode = MitsubishiPlcService.DebugMode;
        try
        {
            using var plc = new MitsubishiPlcService();
            var item = new PlcBatchReadItem<int>("zr11924");

            var result = plc.BatchRead([item]);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsTrue(item.IsSuccess, item.OriginResult.Message);
        }
        finally
        {
            MitsubishiPlcService.DebugMode = previousDebugMode;
        }
    }
}
