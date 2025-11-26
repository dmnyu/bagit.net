namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        Dictionary<string, string> GetTagFileAsDict(string tagFilePath);
        void CreateBagInfo(string bagDir);
        string GetOxum(string bagRoot);
        void CreateBagItTXT(string bagRoot);
        List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath);
        void ValidateBagitTXT(string bagRoot);
        bool HasBagInfo(string bagRoot);
        bool ValidateBagInfo(string bagRoot);
    }
}

