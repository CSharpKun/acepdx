namespace Acepdx.Core.Services;

/*public class MessagePackLicenseCacher : ILicenseCacheService
{
    private readonly IFileService _fileService;
    private readonly ILogger<MessagePackLicenseCacher> _logger;
    private readonly MessagePackSerializer _serializer = new();

    public List<LicenseList> Lists { get; set; }
    public List<License> Licenses { get; set; }

    public MessagePackLicenseCacher(IFileService fileService, ILogger<MessagePackLicenseCacher>? logger = null) 
    {
        _fileService = fileService;
        _logger = logger ?? NullLogger<MessagePackLicenseCacher>.Instance;

        
    }

    private bool TryGetFromCache<T>(out T? result) where T : IShapeable<T>
    {
        var isFileOld = _fileService.GetLastWriteTime() < DateTime.Now - TimeSpan.FromHours(10);
        if (isFileOld)
        {
            result = default;
            return false;
        }

        var bytes = _fileService.ReadFromFile();

        result = _serializer.Deserialize<T>(bytes);
        if (result is null) _logger.LogError("Couldn't parse cache JSON");
        return true;
    }
}*/