using CMP.Models.DataCenter;
using CMP.Services.Plc;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CMP.Models.Resource
{
    public class STCtrl
    {
        private readonly string _stCtrlName;
        private readonly object _refreshLock = new();
        public ObservableCollection<SV_PLC> listDataInSingleStCtrl { get; }
        private readonly MitsubishiPlcService _plc = MitsubishiPlcService.RobotCtrlInstance;

        /// <summary>与 SV_STCtrl.csv 中 Module 列一致，用于与主界面机械手名称（如 EFEMRobot）匹配。</summary>
        public string ModuleName => _stCtrlName;

        public STCtrl(string name)
        {
            _stCtrlName = name;
            listDataInSingleStCtrl = new ObservableCollection<SV_PLC>();
        }

        public void RefreshStCtrlData()
        {
            if (!_plc.IsConnected)
                return;
            for (var i = 0; i < listDataInSingleStCtrl.Count; i++)
                RefreshItem(i);
        }

        public void RefreshStCtrlDataFast()
        {
            RefreshStCtrlDataInternal(fastOnly: true);
        }

        public void RefreshStCtrlDataSlow()
        {
            RefreshStCtrlDataInternal(fastOnly: false);
        }

        public void RefreshSingleStCtrlData(int index) => RefreshItem(index);

        private void RefreshStCtrlDataInternal(bool fastOnly)
        {
            if (!_plc.IsConnected || listDataInSingleStCtrl.Count == 0)
            {
                return;
            }

            lock (_refreshLock)
            {
                var targetItems = listDataInSingleStCtrl
                    .Where(item => item != null && IsFastPoint(item) == fastOnly)
                    .ToList();

                if (targetItems.Count == 0)
                {
                    return;
                }

                var boolItems = targetItems
                    .Where(item => item.Type == "Bool")
                    .OrderBy(item => item.PLCLink, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var wordItems = targetItems
                    .Where(item => item.Type != "Bool")
                    .OrderBy(item => item.PLCLink, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                RefreshBoolItems(boolItems);
                RefreshWordItems(wordItems);
            }
        }

        private void RefreshBoolItems(IReadOnlyCollection<SV_PLC> boolItems)
        {
            if (boolItems.Count == 0)
            {
                return;
            }

            var batchItems = boolItems
                .Select(item => PlcBatchReadItem<bool>.Bind(item.PLCLink, value => item.Value = value, addressType: PlcBatchAddressType.M))
                .Cast<PlcBatchReadItem<bool>>()
                .ToList();

            var result = _plc.BatchReadBool(batchItems);
            if (!result.IsSuccess)
            {
                foreach (var item in boolItems)
                {
                    item.Value = _plc.ReadBoolVari(item.PLCLink);
                }
            }
        }

        private void RefreshWordItems(IReadOnlyCollection<SV_PLC> wordItems)
        {
            if (wordItems.Count == 0)
            {
                return;
            }

            var batchItems = wordItems
                .Select(CreateWordBatchItem)
                .Where(item => item != null)
                .Cast<PlcBatchReadItemBase>()
                .ToList();

            if (batchItems.Count > 0)
            {
                var result = _plc.BatchRead(batchItems);
                if (result.IsSuccess)
                {
                    return;
                }
            }

            foreach (var item in wordItems)
            {
                RefreshItem(item);
            }
        }

        private PlcBatchReadItemBase? CreateWordBatchItem(SV_PLC item)
        {
            return item.Type switch
            {
                "INT_32" => PlcBatchReadItem<int>.Bind(item.PLCLink, value => item.Value = value, addressType: PlcBatchAddressType.D),
                "UINT_16" => PlcBatchReadItem<short>.Bind(item.PLCLink, value => item.Value = value, addressType: PlcBatchAddressType.D),
                _ => null
            };
        }

        private static bool IsFastPoint(SV_PLC item)
        {
            var name = item.Name ?? string.Empty;
            var description = item.Descripition ?? string.Empty;
            var comment = item.Comment ?? string.Empty;

            return name.Equals("initing", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("initdone", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("running", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("stoped", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("empty", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("saftey", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("alarm", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("WFsensor", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("door", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("sensor", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("Clamp", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("Release", StringComparison.OrdinalIgnoreCase)
                   || description.Contains("alarm", StringComparison.OrdinalIgnoreCase)
                   || description.Contains("error", StringComparison.OrdinalIgnoreCase)
                   || comment.Contains("alarm", StringComparison.OrdinalIgnoreCase)
                   || comment.Contains("error", StringComparison.OrdinalIgnoreCase);
        }

        private void RefreshItem(int index)
        {
            var item = listDataInSingleStCtrl[index];
            RefreshItem(item);
        }

        private void RefreshItem(SV_PLC item)
        {
            switch (item.Type)
            {
                case "Bool":
                    item.Value = _plc.ReadBoolVari(item.PLCLink);
                    break;
                case "INT_32":
                    {
                        var r = _plc.ReadIntVari(item.PLCLink);
                        item.Value = r.IsSuccess ? r.Content : item.Value;
                        break;
                    }
                case "UINT_16":
                    item.Value = _plc.ReadShortVari(item.PLCLink);
                    break;
            }
        }

        public void WriteSingleStCtrlData(int index, string value)
        {
            if (listDataInSingleStCtrl[index].Type == "Bool")
                _plc.WriteBoolVari(listDataInSingleStCtrl[index].PLCLink, Convert.ToBoolean(value));
        }
    }
}
