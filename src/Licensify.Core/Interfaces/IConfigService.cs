namespace Licensify.Core.Interfaces;

public interface IConfigService
{
    public Dictionary<string, object> Settings { get; } 
    public Dictionary<string, SpdxRemote> SpdxRemotes { get; }
    void UpdateSettings();
}