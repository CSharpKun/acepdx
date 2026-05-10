using Licensify.Core.Interfaces;

namespace Licensify.Core.Services;

public class FileService : IFileService
{
    private readonly FileInfo _file;
    private readonly string _filePath;

    public FileService(FileInfo file)
    {
        if (file.DirectoryName is not null && !Directory.Exists(file.DirectoryName)) Directory.CreateDirectory(file.DirectoryName);
        _filePath = file.FullName;
        _file = file;
    }

    public void WriteToFile(byte[] data) => File.WriteAllBytes(_filePath, data);

    public void WriteToFile(string data) => File.WriteAllText(_filePath, data);

    public byte[] ReadFromFile() => File.ReadAllBytes(_filePath);

    public string ReadStringFromFile() => File.ReadAllText(_filePath);

    public DateTime GetLastWriteTime() => File.GetLastWriteTimeUtc(_filePath);

    public bool FileExists { get => _file.Exists; }
}