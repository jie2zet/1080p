using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace CMP.Services.Log;

/// <summary>
/// 一列待写入的数据。Key 用于稳定标识列，Header 是写入 CSV 的显示表头。
/// </summary>
public readonly record struct DataLogField(string Key, string Header, string? Value);

/// <summary>
/// CSV 数据记录器，用于记录设备运行数据
/// </summary>
public class DataLogger
{
    private sealed record DataLogColumn(string Key, string Header);

    private readonly string _basePath;
    private readonly string _category;
    private readonly object _lockObj = new();
    
    // 记录每个文件的列结构。Key 用于数据对齐，Header 允许重复。
    private readonly Dictionary<string, List<DataLogColumn>> _fileColumns = new();
    
    // 记录创建时间（Key: waferName 或 "Exsitu", Value: 创建时间）
    private readonly Dictionary<string, DateTime> _createTimes = new();
    
    /// <summary>
    /// CSV 分隔符（默认为逗号）
    /// </summary>
    private const string Delimiter = ",";
    
    /// <summary>
    /// 初始化数据记录器（绑定到特定类别）
    /// </summary>
    /// <param name="category">类别 (如 PolishA, RollerA1, DryA)</param>
    /// <param name="basePath">基础路径，默认为 D:\CMP\Log\</param>
    public DataLogger(string category, string basePath = @"D:\CMP\Log\")
    {
        _basePath = basePath;
        _category = category;
    }
    
    /// <summary>
    /// 创建CSV文件并写入表头
    /// </summary>
    /// <param name="columns">CSV 列定义</param>
    /// <param name="waferName">Wafer名称（Exsitu数据可为null）</param>
    /// <param name="subcategory">子类别 (可选，用于Polish的Exsitu)</param>
    /// <returns>创建的CSV文件完整路径</returns>
    private string CreateCsvFile(IReadOnlyList<DataLogColumn> columns, string waferName, string? subcategory = null)
    {
        lock (_lockObj)
        {
            try
            {
                var filePath = BuildFilePath(waferName, subcategory);
                var key = GetFileKey(waferName, subcategory);
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 如果文件已存在，抛出异常
                if (File.Exists(filePath))
                {
                    throw new InvalidOperationException($"CSV文件已存在，无法重复创建: {filePath}");
                }
                
                // 记录列结构
                _fileColumns[key] = new List<DataLogColumn>(columns);
                
                // 创建CSV文件并写入表头
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = Delimiter,
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = true
                };
                
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, config))
                {
                    // 写入表头
                    foreach (var column in columns)
                    {
                        csv.WriteField(column.Header);
                    }
                    csv.NextRecord();
                }
                
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建CSV文件失败: {ex.Message}", ex);
            }
        }
    }
    
    /// <summary>
    /// 追加一行数据到CSV文件
    /// 如果发现新列，会自动重建文件以包含新列
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="data">数据字典</param>
    /// <param name="subcategory">子类别 (可选，用于Polish的Exsitu)</param>
    public void AppendData(string waferName, Dictionary<string, string?> data, string? subcategory = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        AppendData(waferName, data.Select(kvp => new DataLogField(kvp.Key, kvp.Key, kvp.Value)).ToList(), subcategory);
    }

    /// <summary>
    /// 追加一行带独立列 Key 和显示表头的数据。
    /// </summary>
    public void AppendData(string waferName, IReadOnlyList<DataLogField> fields, string? subcategory = null)
    {
        AppendDataInternal(waferName, fields, subcategory);
    }

    private void AppendDataInternal(string waferName, IReadOnlyList<DataLogField> fields, string? subcategory)
    {
        lock (_lockObj)
        {
            try
            {
                var data = CreateFieldLookup(fields);
                var incomingColumns = fields.Select(field => new DataLogColumn(field.Key, field.Header)).ToList();
                var filePath = BuildFilePath(waferName, subcategory);
                var key = GetFileKey(waferName, subcategory);
                
                // 如果文件不存在，创建文件
                if (!File.Exists(filePath))
                {
                    CreateCsvFile(incomingColumns, waferName, subcategory);
                }
                
                // 检查是否有新列
                var currentColumns = _fileColumns.GetValueOrDefault(key);
                if (currentColumns == null)
                {
                    // 读取现有文件的列结构
                    currentColumns = ReadColumnsFromFile(filePath);
                    _fileColumns[key] = currentColumns;
                }
                
                var currentKeys = currentColumns.Select(column => column.Key).ToHashSet(StringComparer.Ordinal);
                var newColumns = incomingColumns.Where(column => !currentKeys.Contains(column.Key)).ToList();
                
                if (newColumns.Any())
                {
                    // 有新列，需要重建文件
                    RebuildFileWithNewColumns(filePath, incomingColumns, waferName, subcategory);
                    currentColumns = _fileColumns[key];
                }
                
                // 写入数据（按照当前列顺序）
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = Delimiter,
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = false
                };
                
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, config))
                {
                    foreach (var column in currentColumns)
                    {
                        csv.WriteField(data.GetValueOrDefault(column.Key, ""));
                    }
                    csv.NextRecord();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"追加数据到CSV文件失败: {ex.Message}", ex);
            }
        }
    }
    
    /// <summary>
    /// 批量追加多行数据
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="dataList">数据列表</param>
    /// <param name="subcategory">子类别 (可选，用于Polish的Exsitu)</param>
    public void AppendDataBatch(string waferName, List<Dictionary<string, string>>? dataList, string? subcategory = null)
    {
        if (dataList == null || dataList.Count == 0)
            return;
        
        // 逐条写入（自动处理列扩展）
        foreach (var data in dataList)
        {
            AppendData(waferName, data, subcategory);
        }
    }
    
    /// <summary>
    /// Polish 专用：只写入 Exsitu 数据
    /// </summary>
    /// <param name="ExsituData">Exsitu 数据</param>
    public void AppendExsituData(string waferName, Dictionary<string, string?> ExsituData)
    {
        ArgumentNullException.ThrowIfNull(ExsituData);
        AppendExsituData(waferName,
            ExsituData.Select(kvp => new DataLogField(kvp.Key, kvp.Key, kvp.Value)).ToList());
    }

    /// <summary>
    /// Polish 专用：写入带独立列 Key 和显示表头的 Exsitu 数据。
    /// </summary>
    public void AppendExsituData(string waferName, IReadOnlyList<DataLogField> fields)
    {
        AppendExsituDataInternal(waferName, fields);
    }
    
    /// <summary>
    /// Polish 专用：批量写入 Exsitu 数据
    /// </summary>
    /// <param name="ExsituDataList">Exsitu 数据列表</param>
    public void AppendExsituDataBatch(string waferName, List<Dictionary<string, string?>> ExsituDataList)
    {
        if (ExsituDataList == null || ExsituDataList.Count == 0)
            return;
        
        foreach (var data in ExsituDataList)
        {
            AppendExsituData(waferName, data);
        }
    }
    
    /// <summary>
    /// 内部方法：写入 Exsitu 数据
    /// </summary>
    private void AppendExsituDataInternal(string waferName, IReadOnlyList<DataLogField> fields)
    {
        lock (_lockObj)
        {
            try
            {
                var data = CreateFieldLookup(fields);
                var incomingColumns = fields.Select(field => new DataLogColumn(field.Key, field.Header)).ToList();
                var filePath = BuildExsituFilePath(waferName);
                var key = GetFileKey(waferName, "Exsitu");
                
                // 如果文件不存在，创建文件
                if (!File.Exists(filePath))
                {
                    CreateCsvFile(incomingColumns, waferName, "Exsitu");
                }
                
                // 检查是否有新列
                var currentColumns = _fileColumns.GetValueOrDefault(key);
                if (currentColumns == null)
                {
                    // 读取现有文件的列结构
                    currentColumns = ReadColumnsFromFile(filePath);
                    _fileColumns[key] = currentColumns;
                }
                
                var currentKeys = currentColumns.Select(column => column.Key).ToHashSet(StringComparer.Ordinal);
                var newColumns = incomingColumns.Where(column => !currentKeys.Contains(column.Key)).ToList();
                
                if (newColumns.Any())
                {
                    // 有新列，需要重建文件
                    RebuildFileWithNewColumns(filePath, incomingColumns, waferName, "Exsitu");
                    currentColumns = _fileColumns[key];
                }
                
                // 写入数据（按照当前列顺序）
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = Delimiter,
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = false
                };
                
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, config))
                {
                    foreach (var column in currentColumns)
                    {
                        csv.WriteField(data.GetValueOrDefault(column.Key, ""));
                    }
                    csv.NextRecord();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"追加Exsitu数据到CSV文件失败: {ex.Message}", ex);
            }
        }
    }
    
    /// <summary>
    /// Polish 专用：同时写入 Wafer 和 Exsitu 数据
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="waferData">Wafer 数据</param>
    /// <param name="ExsituData">Exsitu 数据</param>
    public void AppendPolishData(string waferName, Dictionary<string, string?> waferData, Dictionary<string, string?> ExsituData)
    {
        // 写入 Wafer 数据
        AppendData(waferName, waferData);
        
        // 写入 Exsitu 数据
        AppendExsituData(waferName, ExsituData);
    }
    
    /// <summary>
    /// Polish 专用：批量同时写入 Wafer 和 Exsitu 数据
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="waferDataList">Wafer 数据列表</param>
    /// <param name="ExsituDataList">Exsitu 数据列表</param>
    public void AppendPolishDataBatch(string waferName, List<Dictionary<string, string?>> waferDataList, 
        List<Dictionary<string, string?>> ExsituDataList)
    {
        // 批量写入 Wafer 数据
        AppendDataBatch(waferName, waferDataList);
        
        // 批量写入 Exsitu 数据
        AppendExsituDataBatch(waferName, ExsituDataList);
    }
    
    /// <summary>
    /// 生成文件存储键
    /// </summary>
    private static string GetFileKey(string? waferName, string? subcategory)
    {
        if (string.Equals(subcategory, "Exsitu", StringComparison.OrdinalIgnoreCase))
        {
            return ":Exsitu";
        }

        // Exsitu: ":Exsitu"
        // Wafer:  "Wafer01:" 或 "Wafer01:Polish"
        return $"{waferName ?? ""}:{subcategory ?? ""}";
    }
    
    /// <summary>
    /// 获取或创建时间戳
    /// </summary>
    private DateTime GetCreateTime(string? waferName, string? subcategory)
    {
        if (waferName is null) waferName = "";
        // Exsitu 使用独立的时间戳
        var timeKey = string.IsNullOrEmpty(waferName) && subcategory == "Exsitu" 
            ? "Exsitu"  // Exsitu 专用键
            : waferName!; // Wafer 使用 waferName
        
        if (!_createTimes.ContainsKey(timeKey))
        {
            _createTimes[timeKey] = DateTime.Now;
        }
        return _createTimes[timeKey];
    }
    
    /// <summary>
    /// 读取文件的表头
    /// </summary>
    private List<DataLogColumn> ReadColumnsFromFile(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = Delimiter,
            Encoding = Encoding.UTF8
        };
        
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? [];
        var usedKeys = new HashSet<string>(StringComparer.Ordinal);
        var columns = new List<DataLogColumn>(headers.Length);

        for (var index = 0; index < headers.Length; index++)
        {
            var header = headers[index] ?? string.Empty;
            var key = header;
            var suffix = 1;
            while (!usedKeys.Add(key))
            {
                key = $"{header}#{suffix++}";
            }

            columns.Add(new DataLogColumn(key, header));
        }

        return columns;
    }
    
    /// <summary>
    /// 重建文件以包含新列
    /// </summary>
    private void RebuildFileWithNewColumns(
        string filePath,
        IReadOnlyList<DataLogColumn> allColumns,
        string waferName,
        string? subcategory)
    {
        var key = GetFileKey(waferName, subcategory);
        var oldColumns = _fileColumns[key];
        
        // 合并列（保留旧列顺序，新列添加到末尾）
        var newColumnsList = new List<DataLogColumn>(oldColumns);
        var existingKeys = oldColumns.Select(column => column.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var col in allColumns)
        {
            if (existingKeys.Add(col.Key))
            {
                newColumnsList.Add(col);
            }
        }
        
        // 读取所有现有数据
        var existingData = new List<string[]>();
        if (File.Exists(filePath))
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Delimiter,
                Encoding = Encoding.UTF8
            };
            
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                
                while (csv.Read())
                {
                    var row = new string[oldColumns.Count];
                    for (var columnIndex = 0; columnIndex < oldColumns.Count; columnIndex++)
                    {
                        row[columnIndex] = csv.GetField(columnIndex) ?? "";
                    }
                    existingData.Add(row);
                }
            }
        }
        
        // 删除旧文件
        File.Delete(filePath);
        
        // 更新列结构
        _fileColumns[key] = newColumnsList;
        
        // 创建新文件
        CreateCsvFile(newColumnsList, waferName, subcategory);
        
        // 写回所有数据
        if (existingData.Any())
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Delimiter,
                Encoding = Encoding.UTF8,
                HasHeaderRecord = false
            };
            
            using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, config);
            
            foreach (var row in existingData)
            {
                for (var columnIndex = 0; columnIndex < newColumnsList.Count; columnIndex++)
                {
                    csv.WriteField(columnIndex < row.Length ? row[columnIndex] : "");
                }
                csv.NextRecord();
            }
        }
    }

    private static Dictionary<string, string?> CreateFieldLookup(IReadOnlyList<DataLogField> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        var data = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field.Key))
            {
                throw new ArgumentException("Data log field keys cannot be null or empty.", nameof(fields));
            }

            if (!data.TryAdd(field.Key, field.Value))
            {
                throw new ArgumentException($"Duplicate data log field key: {field.Key}", nameof(fields));
            }
        }

        return data;
    }
    
    /// <summary>
    /// 构建 Exsitu 文件路径
    /// </summary>
    private string BuildExsituFilePath(string waferName, DateTime? createTimeOverride = null)
    {
        // Exsitu 使用独立的时间戳
        var createTime = createTimeOverride ?? GetCreateTime(null, "Exsitu");
        var timestamp = createTime.ToString("yyyy-MM-dd_HH.mm.ss.fff");
        
        // 基础路径: D:\CMP\Log\Year\Month\Day\
        var basePath = Path.Combine(_basePath, createTime.Year.ToString(), 
            createTime.ToString("yyyy-MM"), createTime.ToString("yyyy-MM-dd"));
        
        // Exsitu 路径: D:\CMP\Log\2025\01\10\PolishA\Exsitu\2025-01-10_09.55.23.257.csv
        var fullPath = Path.Combine(basePath, _category, "Exsitu");
        var fileName = $"{waferName}_{timestamp}.csv";
        
        return Path.Combine(fullPath, fileName);
    }

    private static string GetAvailableFilePath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return filePath;
        }

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var suffix = 1;
        string candidatePath;

        do
        {
            candidatePath = Path.Combine(directory, $"{fileName}_{suffix++}{extension}");
        }
        while (File.Exists(candidatePath));

        return candidatePath;
    }
    
    /// <summary>
    /// 构建文件路径
    /// </summary>
    private string BuildFilePath(string waferName, string? subcategory = null, DateTime? createTimeOverride = null)
    {
        // Exsitu 使用专用方法
        if (subcategory == "Exsitu")
        {
            return BuildExsituFilePath(waferName, createTimeOverride);
        }
        
        // 获取或创建该 Wafer 的时间戳
        var createTime = createTimeOverride ?? GetCreateTime(waferName, subcategory);
        var timestamp = createTime.ToString("yyyy-MM-dd_HH.mm.ss.fff");
        
        // 基础路径: D:\CMP\Log\Year\Month\Day\
        var basePath = Path.Combine(_basePath, createTime.Year.ToString(), 
            createTime.ToString("yyyy-MM"), createTime.ToString("yyyy-MM-dd"));
        
        // 判断是否是 Polish 类别
        bool isPolish = _category.StartsWith("Polish", StringComparison.OrdinalIgnoreCase);
        
        string fileName;
        string fullPath;
        
        if (isPolish)
        {
            // Polish 正常情况
            // 路径: D:\CMP\Log\2025\01\10\PolishA\Polish\Wafer01_2025-01-10_09.55.23.257.csv
            fullPath = Path.Combine(basePath, _category, "Polish");
            fileName = $"{waferName}_{timestamp}.csv";
        }
        else
        {
            // 其他类别带 Wafer
            // 路径: D:\CMP\Log\2025\01\10\RollerA1\Wafer02_2025-01-10_09.55.23.257.csv
            fullPath = Path.Combine(basePath, _category);
            fileName = $"{waferName}_{timestamp}.csv";
        }
        
        return Path.Combine(fullPath, fileName);
    }
    
    /// <summary>
    /// 获取文件路径（不创建文件）
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="subcategory">子类别 (可选)</param>
    public string GetFilePath(string waferName, string? subcategory = null)
    {
        return BuildFilePath(waferName, subcategory);
    }
    
    /// <summary>
    /// 获取 Exsitu 文件路径
    /// </summary>
    public string GetExsituFilePath(string waferName)
    {
        return BuildExsituFilePath(waferName);
    }

    /// <summary>
    /// 将一次记录会话使用的临时 Wafer 文件名替换为最终 Wafer 名称。
    /// </summary>
    public bool RenameWaferFile(string temporaryWaferName, string waferName, string? subcategory = null)
    {
        lock (_lockObj)
        {
            var sourcePath = BuildFilePath(temporaryWaferName, subcategory);
            if (!File.Exists(sourcePath))
            {
                return false;
            }

            var isExsitu = string.Equals(subcategory, "Exsitu", StringComparison.OrdinalIgnoreCase);
            var createTime = isExsitu
                ? GetCreateTime(null, "Exsitu")
                : GetCreateTime(temporaryWaferName, subcategory);
            var destinationPath = BuildFilePath(waferName, subcategory, createTime);
            destinationPath = GetAvailableFilePath(destinationPath);

            File.Move(sourcePath, destinationPath);

            if (!isExsitu)
            {
                var oldKey = GetFileKey(temporaryWaferName, subcategory);
                var newKey = GetFileKey(waferName, subcategory);
                if (_fileColumns.Remove(oldKey, out var columns))
                {
                    _fileColumns[newKey] = columns;
                }

                _createTimes.Remove(temporaryWaferName);
                _createTimes[waferName] = createTime;
            }

            return true;
        }
    }
    
    /// <summary>
    /// 检查文件是否已创建
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="subcategory">子类别 (可选)</param>
    public bool FileExists(string waferName, string? subcategory = null)
    {
        var filePath = BuildFilePath(waferName, subcategory);
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// 检查 Exsitu 文件是否已创建
    /// </summary>
    public bool ExsituFileExists(string waferName)
    {
        var filePath = BuildExsituFilePath(waferName);
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// 删除指定文件
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    /// <param name="subcategory">子类别 (可选)</param>
    public bool DeleteFile(string waferName, string? subcategory = null)
    {
        lock (_lockObj)
        {
            try
            {
                var filePath = BuildFilePath(waferName, subcategory);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    var key = GetFileKey(waferName, subcategory);
                    _fileColumns.Remove(key);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// 删除 Exsitu 文件
    /// </summary>
    public bool DeleteExsituFile(string waferName)
    {
        lock (_lockObj)
        {
            try
            {
                var filePath = BuildExsituFilePath(waferName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _fileColumns.Remove(":Exsitu");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// 清理指定 Wafer 的所有缓存信息
    /// </summary>
    /// <param name="waferName">Wafer名称</param>
    public void ClearWaferCache(string waferName)
    {
        lock (_lockObj)
        {
            _createTimes.Remove(waferName);
            
            var keysToRemove = _fileColumns.Keys
                .Where(k => k.StartsWith($"{waferName}:"))
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _fileColumns.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// 清理 Exsitu 缓存信息
    /// </summary>
    public void ClearExsituCache()
    {
        lock (_lockObj)
        {
            _createTimes.Remove("Exsitu");
            _fileColumns.Remove(":Exsitu");
        }
    }
    
    /// <summary>
    /// 清理指定天数之前的日志文件
    /// </summary>
    /// <param name="days">保留天数</param>
    public void CleanOldLogs(int days = 30)
    {
        try
        {
            if (!Directory.Exists(_basePath))
                return;
            
            var cutoffDate = DateTime.Now.AddDays(-days);
            var directories = Directory.GetDirectories(_basePath, "*", SearchOption.AllDirectories);
            
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                if (dirInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                        // 忽略删除失败的情况
                    }
                }
            }
        }
        catch
        {
            // 忽略清理失败
        }
    }
}
