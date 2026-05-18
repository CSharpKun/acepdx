using Acepdx.Core.Models;

namespace Acepdx.Core.Interfaces;

public interface IConfigService
{    
    public Dictionary<string, SpdxRemote> Remotes { get; set; }
    public Dictionary<string, string> Settings { get; set; }
    public void Save();
    public void Load();
}