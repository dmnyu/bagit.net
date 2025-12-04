using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        //deprecate these
        Dictionary<string, string> GetTagFileAsDict(string tagFilePath);
        //deprecate this
        List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath);


        void CreateBagInfo(string bagDir);
        void CreateBagItTXT(string bagRoot);
        bool HasBagInfo(string bagRoot);
        bool HasBagItTXT(string bagRoot);
        IEnumerable<MessageRecord> ValidateBagInfo(string bagRoot);
        IEnumerable<MessageRecord> ValidateBagitTXT(string bagRoot);
        IEnumerable<MessageRecord> ScanFileForInvalidControlChars(string path);
        IEnumerable<MessageRecord> ValidateTags(string bagInfoPath);
        Dictionary<String, List<String>> GetTags(string tagFilePath);
        string CalculateOxum(string bagRoot);
    }
}

