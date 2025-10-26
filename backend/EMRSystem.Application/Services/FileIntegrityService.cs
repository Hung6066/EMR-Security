// Application/Services/FileIntegrityService.cs
public class FileIntegrityService
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context;

    public FileIntegrityService(IWebHostEnvironment env, ApplicationDbContext context) { _env = env; _context = context; }

    public async Task CreateBaselineAsync()
    {
        // Xóa baseline cũ
        await _context.FileIntegrityRecords.ExecuteDeleteAsync();

        var filesToMonitor = GetMonitoredFiles();
        foreach (var file in filesToMonitor)
        {
            var record = new FileIntegrityRecord
            {
                FilePath = GetRelativePath(file),
                Hash = await CalculateFileHashAsync(file),
                LastChecked = DateTime.UtcNow,
                Status = "Baseline"
            };
            _context.FileIntegrityRecords.Add(record);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<FileIntegrityRecord>> ScanForChangesAsync()
    {
        var baseline = await _context.FileIntegrityRecords.ToListAsync();
        var baselineDict = baseline.ToDictionary(f => f.FilePath, f => f);
        var currentFiles = GetMonitoredFiles().ToDictionary(f => GetRelativePath(f), f => f);
        var changes = new List<FileIntegrityRecord>();

        // Kiểm tra file thay đổi hoặc bị xóa
        foreach (var baselineRecord in baseline)
        {
            if (currentFiles.TryGetValue(baselineRecord.FilePath, out var currentFilePath))
            {
                var currentHash = await CalculateFileHashAsync(currentFilePath);
                if (baselineRecord.Hash != currentHash)
                {
                    baselineRecord.Status = "Changed";
                    baselineRecord.Hash = currentHash; // Cập nhật hash mới
                    changes.Add(baselineRecord);
                }
                currentFiles.Remove(baselineRecord.FilePath); // Đã kiểm tra
            }
            else
            {
                baselineRecord.Status = "Deleted";
                changes.Add(baselineRecord);
            }
            baselineRecord.LastChecked = DateTime.UtcNow;
        }

        // Kiểm tra file mới
        foreach (var newFile in currentFiles)
        {
            var newRecord = new FileIntegrityRecord
            {
                FilePath = newFile.Key,
                Hash = await CalculateFileHashAsync(newFile.Value),
                LastChecked = DateTime.UtcNow,
                Status = "New"
            };
            _context.FileIntegrityRecords.Add(newRecord);
            changes.Add(newRecord);
        }

        if (changes.Any())
        {
            // Gửi cảnh báo
            // ...
            await _context.SaveChangesAsync();
        }
        return changes;
    }

    private IEnumerable<string> GetMonitoredFiles()
    {
        var root = _env.ContentRootPath;
        var files = new List<string>
        {
            Path.Combine(root, "appsettings.json"),
            Path.Combine(root, "appsettings.Production.json")
        };
        // Thêm tất cả các file DLL
        files.AddRange(Directory.GetFiles(root, "*.dll", SearchOption.TopDirectoryOnly));
        // Thêm các file trong wwwroot
        files.AddRange(Directory.GetFiles(Path.Combine(root, "wwwroot"), "*.*", SearchOption.AllDirectories));
        return files;
    }
    
    // Helper methods: CalculateFileHashAsync, GetRelativePath
}