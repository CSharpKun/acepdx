using Licensify.Core.Interfaces;

namespace Licensify.Tests.Core;

internal class MockFileService(byte[]? fileBytes = null, string? fileString = null) : IFileService
{
    public byte[] FileBytes { get; private set; } = fileBytes ?? [0xDE, 0xAD, 0xBE, 0xEF];
    public string FileString { get; private set; } = fileString ?? "dummy";

    public bool FileExists => true;
    public DateTime GetLastWriteTime() => DateTime.UtcNow;

    public byte[] ReadFromFile() => FileBytes;
    public string ReadStringFromFile() => FileString;
    public void WriteToFile(byte[] data) => FileBytes = data;
    public void WriteToFile(string data) => FileString = data;
    
}