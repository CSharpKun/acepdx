namespace Acepdx.Core.Interfaces;

public interface IConfigService
{
    Task<T?> Get<T>(string path, T? defaultValue = default);
    Task Set<T>(string path, T value);
    Task Unset(string path);
}
