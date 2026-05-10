namespace Licensify.Core.Interfaces;

public interface ILicenseCacheService
{
    public Task<T?> GetData<T>(string name, CancellationToken token = default) where T : class;
}