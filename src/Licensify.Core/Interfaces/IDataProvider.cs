using System.Text.RegularExpressions;

namespace Licensify.Core.Interfaces;

public interface IDataProvider 
{
    public string GetData(DataQueryType type, string? defaultValue = null, Regex? validation = null);
    public bool GetFlag(FlagQueryType type, string? textToChange = null);
}