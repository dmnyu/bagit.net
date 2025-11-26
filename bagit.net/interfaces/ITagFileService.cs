namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        Dictionary<string, string> GetTagFileAsDict(string tagFilePath);
        List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath);
        Dictionary<String, List<String>> GetTags(string tagFilePath);
        void CreateBagInfo(string bagDir);
        string GetOxum(string bagRoot);
        void CreateBagItTXT(string bagRoot);
        void ValidateBagitTXT(string bagRoot);
        bool HasBagInfo(string bagRoot);
        bool ValidateBagInfo(string bagRoot);
    }
}

