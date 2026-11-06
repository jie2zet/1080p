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

            var mItems = new List<SV_PLC>();
            var dBitItems = new List<(SV_PLC Item, DBitAddress DBit)>();
            foreach (var item in boolItems)
            {
                if (TryParseDBit(item.PLCLink, out var dBit))
                {
                    dBitItems.Add((item, dBit));
                }
                else
                {
                    mItems.Add(item);
                }
            }

            if (mItems.Count > 0)
            {
                var mBatch = mItems
                    .Select(item => PlcBatchReadItem<bool>.Bind(item.PLCLink, value => item.Value = value, addressType: PlcBatchAddressType.M))
                    .ToList();
                var mResult = _plc.BatchReadBool(mBatch);
                if (!mResult.IsSuccess)
                {
                    foreach (var item in mItems)
                    {
                        item.Value = _plc.ReadBoolVari(item.PLCLink);
                    }
                }
            }

            if (dBitItems.Count == 0)
            {
                return;
            }

            var dBatch = new List<PlcBatchReadItemBase>(dBitItems.Count);
            foreach (var pair in dBitItems)
            {
                var item = pair.Item;
                var dBit = pair.DBit;
                dBatch.Add(PlcBatchReadItem<bool>.Bind(
                    dBit.Address,
                    value => item.Value = value,
                    converter: MitsubishiPlcService.ToBitConverter(dBit),
                    addressType: PlcBatchAddressType.D));
            }

            var dResult = _plc.BatchRead(dBatch);
            if (dResult.IsSuccess)
            {
                return;
            }

            foreach (var pair in dBitItems)
            {
                var result = _plc.ReadDBit(pair.DBit);
                if (result.IsSuccess)
                {
                    pair.Item.Value = result.Content;
                }
            }
        }

        private static bool TryParseDBit(string? link, out DBitAddress dBit)
        {
            dBit = null!;
            if (string.IsNullOrWhiteSpace(link))
            {
                return false;
            }

            var raw = link.Trim();
            var dot = raw.IndexOf('.');
            if (dot <= 0 || !raw.StartsWith('D') && !raw.StartsWith('d'))
            {
                return false;
            }

            var address = raw[..dot];
            if (!int.TryParse(raw[(dot + 1)..], out var bit) || bit is < 0 or > 15)
            {
                return false;
            }

            dBit = new DBitAddress(address, bit);
            return true;
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
                "INT_32" => PlcBatchReadItem<int>.Bind(item.PLCLink, value => item.Value = value),
                "UINT_16" => PlcBatchReadItem<short>.Bind(item.PLCLink, value => item.Value = value),
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
                    if (TryParseDBit(item.PLCLink, out var dBit))
                    {
                        var result = _plc.ReadDBit(dBit);
                        if (result.IsSuccess)
                        {
                            item.Value = result.Content;
                        }
                    }
                    else
                    {
                        item.Value = _plc.ReadBoolVari(item.PLCLink);
                    }
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
            var item = listDataInSingleStCtrl[index];
            if (item.Type != "Bool")
            {
                return;
            }

            var bitValue = Convert.ToBoolean(value);
            if (TryParseDBit(item.PLCLink, out var dBit))
            {
                _plc.WriteDBit(dBit, bitValue);
                return;
            }

            _plc.WriteBoolVari(item.PLCLink, bitValue);
        }
    }
}
