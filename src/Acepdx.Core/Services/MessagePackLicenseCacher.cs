using System.IO.Abstractions;

using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Nerdbank.MessagePack;

namespace Acepdx.Core.Services;

public class MessagePackLicenseCacher(
    IFileSystem fileSystem,
    IFolders folders,
    ILogger<MessagePackLicenseCacher>? logger = null
) //: ILicenseCacheService
{
    public List<LicenseList> CachedLists { get; set; } = [];
    public List<License> CachedLicenses { get; set; } = [];

    private readonly IFolders _folders = folders;
    private readonly ILogger<MessagePackLicenseCacher> _logger =
        logger ?? NullLogger<MessagePackLicenseCacher>.Instance;
    private readonly MessagePackSerializer _serializer = new();
}
