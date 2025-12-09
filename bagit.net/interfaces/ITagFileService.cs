using bagit.net.domain;

namespace bagit.net.interfaces
{
    public interface ITagFileService
    {
        //deprecate these
        Dictionary<string, string> GetTagFileAsDict(string tagFilePath);
        //deprecate this
        List<KeyValuePair<string, string>> GetTagFileAsList(string baginfoPath);


        void CreateBagInfo(string bagDir, string? tagFileLocation);
        void CreateBagItTXT(string bagRoot);
        bool HasBagInfo(string bagRoot);
        bool HasBagItTXT(string bagRoot);
        void ValidateBagInfo(string bagRoot);
        void ValidateBagitTXT(string bagRoot);
        void ScanFileForInvalidControlChars(string path);
        void ValidateTags(string bagInfoPath);
        Dictionary<String, List<String>> GetTags(string tagFilePath);
        string CalculateOxum(string bagRoot);
        void AddTag(string key, string value, string bagRoot);
        void SetTag(string key, string value, string bagRoot);
        void DeleteTag(string key, string bagRoot);
        void ViewTagFile(string bagRoot);
    }
}

