namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        Dictionary<string, string> GetTagFileAsDict(string tagFilePath);

        void CreateBagInfo(string bagDir);
        string GetOxum(string bagRoot);

        List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath);

    }
}

