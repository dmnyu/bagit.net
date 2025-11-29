namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        //deprecate this
        Dictionary<string, string> GetTagFileAsDict(string tagFilePath);
        //deprecate this
        List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath);
        Dictionary<String, List<String>> GetTags(string tagFilePath);
       
        string CalculateOxum(string bagRoot);
        bool HasBagItTXT(string bagRoot);
        void CreateBagItTXT(string bagRoot);
        void ValidateBagitTXT(string bagRoot);
        bool HasBagInfo(string bagRoot);
        void CreateBagInfo(string bagDir);
        bool ValidateBagInfo(string bagRoot);
    }
}

