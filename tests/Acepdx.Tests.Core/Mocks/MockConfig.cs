using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

namespace Acepdx.Tests.Core.Mocks;

public class MockConfig : IConfigService
{
    public Task<T?> Get<T>(string path, T? defaultValue = default)
    {
        throw new NotImplementedException();
    }

    public Task Set<T>(string path, T value)
    {
        throw new NotImplementedException();
    }

    public Task Unset(string path)
    {
        throw new NotImplementedException();
    }
}
