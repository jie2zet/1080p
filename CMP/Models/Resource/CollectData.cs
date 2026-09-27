using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using KHT = CMP.Models.kvsPlc.KvHostLink;
using KHST = CMP.Models.kvsPlc.Polish.KvStruct;
using KHSTRollerCollectData = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData;
using KHSTRollerIOData = CMP.Models.kvsPlc.Roll.KvsStructRollerIOData;
using KHSTPenCollectData = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData;
using KHSTPenIOData = CMP.Models.kvsPlc.Pen.KvsStructPenIOData;
using KHSTDryCollectData = CMP.Models.kvsPlc.Dry.KvsStructDryCollectData;
using KHSTDryIOData = CMP.Models.kvsPlc.Dry.KvsStructDryIOData;
using KHSTSV = CMP.Models.kvsPlc.SV.KvStruct;
using KHSTIO = CMP.Models.kvsPlc.IO.KvStruct;
using CMP.Models.Operation.RoboticArm;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Wordprocessing;
using static CMP.Models.Operation.Roller.RollViewPoco;
using static CMP.Models.Recipe.RecipeEdit.Recipe;
using System.Drawing;
using System.Security.Policy;
using static CMP.Models.kvsPlc.Polish.KvStruct;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO.Ports;
using System.Runtime.Serialization;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Reflection;
using CMP.Services.Reflection;
using static CMP.Models.kvsPlc.IO.KvStruct;
using static CMP.Models.kvsPlc.SV.KvStruct;

namespace CMP.Models.Resource
{
    public abstract class CollectDataChamber : BindableBase
    {
        string name;
        public readonly Dictionary<string, SV> ListSV;
        public KVLink kvLink;
        public byte[] readBuf;
        public int err;
        protected readonly object IoDataSync = new();
        internal object IoDataLock => IoDataSync;

        public string Name => name;

        /// <summary>
        /// 运行期键前缀（PolishA / RollerA / PenA / DryA …）。
        /// 采集端（SetSv）与日志端（DataLoggerHelper）必须通过 <see cref="BuildRuntimeKey"/> 使用同一个前缀，
        /// 否则 CSV 会出现“有表头但没有值”的列。
        /// </summary>
        public string KeyPrefix { get; protected set; } = string.Empty;

        /// <summary>过程量结构体字段（反射自各腔体的采集结构体），日志端就是按它遍历出列的。</summary>
        public FieldInfo[] collectDataFields = [];

        /// <summary>IO 结构体字段（反射自各腔体的 IO 结构体）。</summary>
        public FieldInfo[] ioDataFields = [];

        private DateTime _nextReadWarnUtc;

        public CollectDataChamber(string name, KVLink link)
        {
            this.name = name;
            this.kvLink = link;
            ListSV = new Dictionary<string, SV>();
            readBuf = new byte[2048];
            //_stCollectData = new KHST.stCollectData();
        }

        /// <summary>
        /// 构造运行期键。indexPrefix 是工位序号（"_1"/"_2"/"_3"），结构体字段名自带序号时传 null。
        /// SV 变量表里的名字（RollerA2 Process Step Act / PenA1 Pen Up Position Sensor …）就是按
        /// “腔体前缀 + 工位序号 + 字段名”归一化后匹配的，所以这个拼法必须唯一。
        /// </summary>
        public string BuildRuntimeKey(string? indexPrefix, string fieldName)
        {
            return string.Concat(KeyPrefix, indexPrefix, fieldName);
        }

        /// <summary>
        /// 结构体里的填充占位字段（Member*），PLC 上是保留位，没有对应的 SV 变量。
        /// 采集与日志必须用同一判据跳过，避免出现只有表头的垃圾列。
        /// </summary>
        public static bool IsPaddingField(string fieldName)
        {
            return fieldName.StartsWith("Member", StringComparison.Ordinal);
        }

        /// <summary>
        /// PLC 结构体读取失败时告警（10 秒节流）。以前这里是静默 return，
        /// 结果整段 IO 列没有数据却看不出原因。
        /// </summary>
        protected void WarnOnReadFailure(string variableName)
        {
            var now = DateTime.UtcNow;
            if (now < _nextReadWarnUtc)
            {
                return;
            }

            _nextReadWarnUtc = now.AddSeconds(10);
            CMP.Services.Log.Logger.For(CMP.Services.Log.Logger.Category.Logging)
                .Warn($"Chamber {name}: read PLC variable '{variableName}' failed (err={err}); its values will be missing from the CSV log.");
        }

        /// <summary>
        /// 伺服 ON 字段映射：字段名（不含工位前缀）顺序 = 设备位顺序。
        ///
        /// 各腔体用 nameof 写死，所以结构体“改名”会直接编译失败，不会静默错位；
        /// 结构体“新增/删除 …ServoOn 字段”由 <see cref="FindUnmappedMotorServoFields"/> 检查
        /// （构造时告警、单元测试里报错），提示必须同步更新本表。
        /// PLC 若在中间插入新的伺服位，只能人工同步 —— 位序无法从结构体推断。
        /// </summary>
        internal abstract IReadOnlyList<string> MotorServoSuffixes { get; }

        /// <summary>带工位号的腔体（Roller/Pen）返回 "_1"/"_2"/"_3"，否则返回空串。</summary>
        internal virtual IReadOnlyList<string> StationPrefixes => [string.Empty];

        /// <summary>映射表覆盖的所有结构体字段名（用于与结构体实际字段做一致性检查）。</summary>
        internal IReadOnlyCollection<string> MappedMotorServoFieldNames()
        {
            var names = new List<string>();
            foreach (var prefix in StationPrefixes)
            {
                foreach (var suffix in MotorServoSuffixes)
                {
                    names.Add(prefix + suffix);
                }
            }

            return names;
        }

        /// <summary>
        /// 结构体里有、映射表里没有的 …ServoOn 字段。结构体新增属性时会非空，
        /// 说明有位伺服状态不会被记录（而不是悄悄错位）。
        /// </summary>
        internal IReadOnlyList<string> FindUnmappedMotorServoFields()
        {
            var mapped = MappedMotorServoFieldNames().ToHashSet(StringComparer.Ordinal);
            return collectDataFields
                .Select(field => field.Name)
                .Where(fieldName => fieldName.EndsWith(MotorServoFieldSuffix, StringComparison.Ordinal)
                    && !mapped.Contains(fieldName))
                .ToList();
        }

        /// <summary>
        /// 把设备位读到的伺服 ON 写进日志/界面真正会读的运行期键
        /// （键名 = 采集结构体字段名，归一化后正好对上 SV 变量表里的同名变量）。
        /// </summary>
        /// <param name="motorServoBits">设备位，顺序与 <see cref="MotorServoSuffixes"/> 一致。</param>
        /// <param name="stationPrefix">工位前缀（"_1"/"_2"/"_3"），字段名自带工位号时必填。</param>
        internal void ApplyMotorServoStatus(IReadOnlyList<bool> motorServoBits, string stationPrefix = "")
        {
            ArgumentNullException.ThrowIfNull(motorServoBits);

            for (var bit = 0; bit < MotorServoSuffixes.Count && bit < motorServoBits.Count; bit++)
            {
                SetSv(BuildRuntimeKey(null, stationPrefix + MotorServoSuffixes[bit]), motorServoBits[bit]);
            }
        }

        /// <summary>结构体伺服字段与映射表不一致时告警（每个腔体实例一次），避免结构体改了没人发现。</summary>
        protected void WarnIfMotorServoMappingIncomplete()
        {
            var unmapped = FindUnmappedMotorServoFields();
            if (unmapped.Count == 0)
            {
                return;
            }

            CMP.Services.Log.Logger.For(CMP.Services.Log.Logger.Category.Logging)
                .Warn($"Chamber {name}: 采集结构体有 {unmapped.Count} 个伺服字段不在映射表里（结构体可能新增/改名），"
                    + $"这些伺服位不会被记录：{string.Join(", ", unmapped)}");
        }

        private const string MotorServoFieldSuffix = "MotorServoOn";

        public void SetSv(string key, object? value)
        {
            if (ListSV.TryGetValue(key, out var sv))
            {
                sv.Value = value;
            }
            else
            {
                ListSV[key] = new SV { Name = key, Value = value };
            }
        }

        public SV? GetSv(string key)
        {
            return ListSV.GetValueOrDefault(key);
        }

        /// <summary>
        /// 读取缓存中指定运行期字段的 bool 值。stationPrefix 对 Roller/Pen 使用
        /// "_1"/"_2"/"_3"，Polish/Dry 传空字符串。
        /// </summary>
        public abstract bool TryGetIoValue(string stationPrefix, string fieldName, out bool value);

        public abstract void CollectData();
    }

    public class Polish : CollectDataChamber
    {
        const uint MotorStatusAddress = 530;
        const uint MotorCount = 7;
        public string polishName = string.Empty;

        stPolishCollectData _stCollectData;
        KHST.stIOData _stIOData;
        private KHSTIO.stPolishIOData _ioData;

        /// <summary>最近一次成功读取的 Polish IO 结构体。</summary>
        public KHSTIO.stPolishIOData IoData
        {
            get { lock (IoDataSync) return _ioData; }
        }

        public Polish(string name, KVLink link) : base(name, link)
        {
            _stCollectData = new stPolishCollectData();
            _stIOData = new KHST.stIOData();
            _ioData = new KHSTIO.stPolishIOData();
            collectDataFields = typeof(KHSTSV.stPolishCollectData).GetFields();
            ioDataFields = typeof(KHSTIO.stPolishIOData).GetFields();
            polishName = name;
            KeyPrefix = name;
            WarnIfMotorServoMappingIncomplete();
        }

        /// <summary>M530 位顺序：Platen、Head Rotation、Head Sweep、Head UpDown、Condition Head、Condition Sweep、Slurry Sweep。</summary>
        internal override IReadOnlyList<string> MotorServoSuffixes =>
        [
            nameof(KHSTSV.stPolishCollectData.PlatenRotationMotorServoOn),
            nameof(KHSTSV.stPolishCollectData.HeadRotationMotorServoOn),
            nameof(KHSTSV.stPolishCollectData.HeadSweepMotorServoOn),
            nameof(KHSTSV.stPolishCollectData.HeadUpDownMotorServoOn),
            nameof(KHSTSV.stPolishCollectData.ConditionHeadRotationMotorServoOn),
            nameof(KHSTSV.stPolishCollectData.ConditionSweepMotorServoOn),
            nameof(KHSTSV.stPolishCollectData.SlurrySweepMotorServoOn)
        ];

        public override void CollectData()
        {
            //KHST.stCollectData stCollectData = kvsPlcComm.ReadVariable(IP);
            CollectProcessData1();
            CollectIOData1();
            CollectMotorServoStatus();
        }

        private void CollectProcessData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTSV.stPolishCollectData _stPolishCollectData = new KHSTSV.stPolishCollectData();
                KHSTSV.ByteTo_stPolishCollectData(ref _stPolishCollectData, _readBuf);
                if (_stPolishCollectData != null)
                {
                    CompiledFieldAccessor<stPolishCollectData>.ProcessAllFields(_stPolishCollectData, (name, value) =>
                    {
                        SetSv(BuildRuntimeKey(null, name), value);
                    });
                }
            }
        }

        private void CollectIOData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "structIOData", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTIO.stPolishIOData _stPolishIOData = new KHSTIO.stPolishIOData();
                KHSTIO.ByteTo_stPolishIOData(ref _stPolishIOData, _readBuf);
                if (_stPolishIOData != null)
                {
                    lock (IoDataSync)
                    {
                        _ioData = _stPolishIOData;
                    }
                    CompiledFieldAccessor<stPolishIOData>.ProcessAllFields(_stPolishIOData, (name, value) =>
                    {
                        SetSv(BuildRuntimeKey(null, name), value);
                    });
                }
            }
        }

        public override bool TryGetIoValue(string stationPrefix, string fieldName, out bool value)
        {
            lock (IoDataSync)
            {
                var field = typeof(KHSTIO.stPolishIOData).GetField(fieldName);
                if (field?.FieldType == typeof(bool) && field.GetValue(_ioData) is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }
            }

            value = false;
            return false;
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[MotorCount];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, MotorStatusAddress, 0, MotorCount, readBuf);
            if(err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus);
            }
        }
        
    }

    public enum PolishProcessType
    {
        None = 100,
        Polish = 101,
        Load = 103,
        Unload = 104,
        Dechuck = 105,
        ExsituCondition = 240
    }

    public class Roller:CollectDataChamber
    {
        KHSTRollerCollectData.stRollerCollectData _stRollerCollectData;
        private readonly KHSTRollerIOData.stRollerIOData[] _ioData =
            [new(), new(), new()];
        public string rollerName = string.Empty;

        public Roller(string name, KVLink link) : base(name, link)
        {
            _stRollerCollectData = new KHSTRollerCollectData.stRollerCollectData();
            collectDataFields = typeof(KHSTRollerCollectData.stRollerCollectData).GetFields();
            rollerName = name;
            KeyPrefix = name;
            ioDataFields = typeof(KHSTRollerIOData.stRollerIOData).GetFields();
            WarnIfMotorServoMappingIncomplete();
        }

        public KHSTRollerIOData.stRollerIOData GetIoData(int station)
        {
            lock (IoDataSync) return _ioData[station - 1];
        }

        public override bool TryGetIoValue(string stationPrefix, string fieldName, out bool value)
        {
            var station = ParseStation(stationPrefix);
            lock (IoDataSync)
            {
                var field = typeof(KHSTRollerIOData.stRollerIOData).GetField(fieldName);
                if (station is >= 1 and <= 3 && field?.FieldType == typeof(bool)
                    && field.GetValue(_ioData[station - 1]) is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }
            }
            value = false;
            return false;
        }

        private static int ParseStation(string stationPrefix) =>
            int.TryParse(stationPrefix.TrimStart('_'), out var result) ? result : 0;

        /// <summary>M534/M535 位顺序：WF 左柱、WF 右柱、上辊、下辊。</summary>
        internal override IReadOnlyList<string> MotorServoSuffixes =>
        [
            WithoutStation(nameof(KHSTRollerCollectData.stRollerCollectData._1WFLeftPillarMotorServoOn)),
            WithoutStation(nameof(KHSTRollerCollectData.stRollerCollectData._1WFRightPillarMotorServoOn)),
            WithoutStation(nameof(KHSTRollerCollectData.stRollerCollectData._1UpperRollerMotorServoOn)),
            WithoutStation(nameof(KHSTRollerCollectData.stRollerCollectData._1LowerRollerMotorServoOn))
        ];

        internal override IReadOnlyList<string> StationPrefixes => ["_1", "_2", "_3"];

        /// <summary>"_1WFLeftPillarMotorServoOn" -> "WFLeftPillarMotorServoOn"（去掉工位号）。</summary>
        private static string WithoutStation(string fieldName) => fieldName[2..];

        public override void CollectData()
        {
            CollectProcessData();
            CollectIOData();
            CollectMotorServoStatus();
        }

        private void CollectProcessData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTRollerCollectData.stRollerCollectData _stRollerCollectData = new KHSTRollerCollectData.stRollerCollectData();
                KHSTRollerCollectData.ByteTo_stRollerCollectData(ref _stRollerCollectData, _readBuf);
                if (_stRollerCollectData != null)
                {
                    CompiledFieldAccessor<KHSTRollerCollectData.stRollerCollectData>.ProcessAllFields(_stRollerCollectData, (name, value) =>
                    {
                        SetSv(BuildRuntimeKey(null, name), value);
                    });
                }
            }
        }

        private void CollectIOData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData1", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                ApplyIoData(_readBuf, 1);
            }
            else
            {
                WarnOnReadFailure("IOData1");
            }
            err = KHT.KHLReadVariable(kvLink.sock, "IOData2", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                ApplyIoData(_readBuf, 2);
            }
            else
            {
                WarnOnReadFailure("IOData2");
            }
            err = KHT.KHLReadVariable(kvLink.sock, "IOData3", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                ApplyIoData(_readBuf, 3);
            }
            else
            {
                WarnOnReadFailure("IOData3");
            }
        }

        /// <summary>
        /// 把一段 IOData 结构体字节流（PLC 变量 IOData1/IOData2/IOData3，每个工位一块）解析成运行期 SV。
        /// 键 = KeyPrefix + "_" + 工位号 + 字段名，与 DataLoggerHelper.LogRollerData 的查找键完全一致；
        /// 与传输层解耦，测试可直接喂缓冲区验证键与位序。
        /// </summary>
        internal void ApplyIoData(byte[] buffer, int rollerIndex)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            var indexPrefix = $"_{rollerIndex}";
            var ioData = new KHSTRollerIOData.stRollerIOData();
            KHSTRollerIOData.ByteTo_stRollerIOData(ref ioData, buffer);
            lock (IoDataSync)
            {
                _ioData[rollerIndex - 1] = ioData;
            }
            CompiledFieldAccessor<KHSTRollerIOData.stRollerIOData>.ProcessAllFields(ioData, (fieldName, value) =>
            {
                if (IsPaddingField(fieldName))
                {
                    return;
                }

                SetSv(BuildRuntimeKey(indexPrefix, fieldName), value);
            });
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[4];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 534, 0, 4, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus, "_1");
            }
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 534, 8, 4, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus, "_2");
            }
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 535, 0, 4, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus, "_3");
            }
        }

    }

    public class Pen:CollectDataChamber
    {
        KHSTPenCollectData.stPenCollectData _stPenCollectData;
        private KHSTPenIOData.stPenIOData _ioData = new();
        public string penName = string.Empty;

        public Pen(string name, KVLink link) : base(name, link)
        {
            _stPenCollectData = new KHSTPenCollectData.stPenCollectData();
            collectDataFields = typeof(KHSTPenCollectData.stPenCollectData).GetFields();
            penName = name;
            KeyPrefix = name;
            ioDataFields = typeof(KHSTPenIOData.stPenIOData).GetFields();
            WarnIfMotorServoMappingIncomplete();
        }

        public KHSTPenIOData.stPenIOData IoData
        {
            get { lock (IoDataSync) return _ioData; }
        }

        public override bool TryGetIoValue(string stationPrefix, string fieldName, out bool value)
        {
            if (!string.Equals(stationPrefix, "_1", StringComparison.Ordinal))
            {
                value = false;
                return false;
            }
            lock (IoDataSync)
            {
                var actualName = fieldName.TrimStart('_');
                var field = typeof(KHSTPenIOData.stPenIOData).GetField(actualName);
                if (field?.FieldType == typeof(bool) && field.GetValue(_ioData) is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }
            }
            value = false;
            return false;
        }

        /// <summary>M537 位顺序：Chuck、Pen Arm Sweep、Pen Rotation。</summary>
        internal override IReadOnlyList<string> MotorServoSuffixes =>
        [
            WithoutStation(nameof(KHSTPenCollectData.stPenCollectData._1ChuckMotorServoOn)),
            WithoutStation(nameof(KHSTPenCollectData.stPenCollectData._1PenArmSweepMotorServoOn)),
            WithoutStation(nameof(KHSTPenCollectData.stPenCollectData._1PenRotationMotorServoOn))
        ];

        internal override IReadOnlyList<string> StationPrefixes => ["_1", "_2"];

        /// <summary>"_1ChuckMotorServoOn" -> "ChuckMotorServoOn"（去掉工位号）。</summary>
        private static string WithoutStation(string fieldName) => fieldName[2..];

        public override void CollectData()
        {
            CollectProcessData();
            CollectIOData();
            CollectMotorServoStatus();
        }

        private void CollectProcessData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTPenCollectData.stPenCollectData _stPenCollectData = new KHSTPenCollectData.stPenCollectData();
                KHSTPenCollectData.ByteTo_stPenCollectData(ref _stPenCollectData, _readBuf);
                if (_stPenCollectData != null)
                {
                    CompiledFieldAccessor<KHSTPenCollectData.stPenCollectData>.ProcessAllFields(_stPenCollectData, (name, value) =>
                    {
                        SetSv(BuildRuntimeKey(null, name), value);
                    });
                }
            }
        }

        private void CollectIOData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                ApplyIoData(_readBuf, 1);
            }
            else
            {
                WarnOnReadFailure("IOData");
            }
        }

        /// <summary>
        /// 把 PenA/PenB 各自 PLC 的 IOData 结构体字节流解析成运行期 SV。
        /// 每台 PLC 只有一个 IOData 变量，当前对应各自的第一个 Pen 工位。
        /// 结构体是 8bc7821 拆出来的单支笔结构体 KHSTPenIOData.stPenIOData（字段名不带 _1/_2 序号），
        /// 所以站号必须由键补上：KeyPrefix + "_" + 工位号 + 字段名，正好对应 SV 变量表里的
        /// “PenA1 Pen Up Position Sensor” 这类名字，也与 DataLoggerHelper.LogPenData 的查找键一致。
        /// </summary>
        internal void ApplyIoData(byte[] buffer, int penIndex)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            var indexPrefix = $"_{penIndex}";
            var ioData = new KHSTPenIOData.stPenIOData();
            KHSTPenIOData.ByteTo_stPenIOData(ref ioData, buffer);
            lock (IoDataSync)
            {
                _ioData = ioData;
            }
            CompiledFieldAccessor<KHSTPenIOData.stPenIOData>.ProcessAllFields(ioData, (fieldName, value) =>
            {
                if (IsPaddingField(fieldName))
                {
                    return;
                }

                SetSv(BuildRuntimeKey(indexPrefix, fieldName), value);
            });
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[3];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 537, 0, 3, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus, "_1");
            }
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 537, 8, 3, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus, "_2");
            }
        }

    }

    public class Dry:CollectDataChamber
    {
        public string dryName = string.Empty;
        private KHSTDryIOData.stDryIOData _ioData = new();

        public Dry(string name, KVLink link) : base(name, link)
        {
            collectDataFields = typeof(KHSTDryCollectData.stDryCollectData).GetFields();
            dryName = name;
            KeyPrefix = name;
            ioDataFields = typeof(KHSTDryIOData.stDryIOData).GetFields();
            WarnIfMotorServoMappingIncomplete();
        }

        public KHSTDryIOData.stDryIOData IoData
        {
            get { lock (IoDataSync) return _ioData; }
        }

        public override bool TryGetIoValue(string stationPrefix, string fieldName, out bool value)
        {
            lock (IoDataSync)
            {
                var field = typeof(KHSTDryIOData.stDryIOData).GetField(fieldName);
                if (field?.FieldType == typeof(bool) && field.GetValue(_ioData) is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }
            }
            value = false;
            return false;
        }

        /// <summary>M539 位顺序：Chuck、Arm Sweep。</summary>
        internal override IReadOnlyList<string> MotorServoSuffixes =>
        [
            nameof(KHSTDryCollectData.stDryCollectData.ChuckMotorServoOn),
            nameof(KHSTDryCollectData.stDryCollectData.ArmSweepMotorServoOn)
        ];

        public override void CollectData()
        {
            CollectProcessData1();
            CollectIOData1();
            CollectMotorServoStatus();
        }

        private void CollectProcessData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTDryCollectData.stDryCollectData _stDryCollectData = new KHSTDryCollectData.stDryCollectData();
                KHSTDryCollectData.ByteTo_stDryCollectData(ref _stDryCollectData, _readBuf);
                if (_stDryCollectData != null)
                {
                    CompiledFieldAccessor<KHSTDryCollectData.stDryCollectData>.ProcessAllFields(_stDryCollectData, (name, value) =>
                    {
                        SetSv(BuildRuntimeKey(null, name), value);
                    });
                }
            }
        }

        private void CollectIOData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTDryIOData.stDryIOData _stDryIOData = new KHSTDryIOData.stDryIOData();
                KHSTDryIOData.ByteTo_stDryIOData(ref _stDryIOData, _readBuf);
                if (_stDryIOData != null)
                {
                    lock (IoDataSync)
                    {
                        _ioData = _stDryIOData;
                    }
                    CompiledFieldAccessor<KHSTDryIOData.stDryIOData>.ProcessAllFields(_stDryIOData, (name, value) =>
                    {
                        SetSv(BuildRuntimeKey(null, name), value);
                    });
                }
            }
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[2];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 539, 0, 2, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTDryCollectData.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                ApplyMotorServoStatus(motorServoStatus);
            }
        }

    }
}
