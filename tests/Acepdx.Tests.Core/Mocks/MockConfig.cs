using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

namespace Acepdx.Tests.Core.Mocks;

public class MockConfig : IConfigService
{
    public Dictionary<string, SpdxRemote> Remotes { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];

    public void Save() {}
    public void Load() {}
}