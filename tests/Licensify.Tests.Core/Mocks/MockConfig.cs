using Licensify.Core;
using Licensify.Core.Interfaces;

namespace Licensify.Tests.Core.Mocks;

public class MockConfig : IConfigService
{
    public Dictionary<string, SpdxRemote> Remotes { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];

    public void Save() {}
    public void Load() {}
}