namespace bagit.net.interfaces
{
    public interface IBagInfoService
    {
        void CreateBagInfo(string bagDir);
        string GetOxum(string bagRoot);

        List<KeyValuePair<string, string>> GetBagInfoAsKeyValuePairs(string baginfoPath);

    }
}
