namespace Licensify.Core.Interfaces;

public interface IFileService 
{
    public void WriteToFile(byte[] data);
    public void WriteToFile(string data);
    public byte[] ReadFromFile();
    public string ReadStringFromFile();
    public DateTime GetLastWriteTime();
    public bool FileExists { get; }
}