namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        public Dictionary<string, string> GetTagFileAsDict(string tagFilePath);
    }
}
