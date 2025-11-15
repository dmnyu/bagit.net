using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.tests
{
    public class TestBagInfo : IDisposable
    {
        private readonly string _tmpDir;
        private readonly Bagger _bagger;

        public TestBagInfo()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _bagger = new Bagger();
        }

        public void Dispose()
        {
            if (Path.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        public void Test_Bag_Has_BagInfo_File()
        {
            _bagger.CreateBag(_tmpDir, ChecksumAlgorithm.MD5);
            var bagInfoFile = Path.Combine(_tmpDir, "bag-info.txt");
            Assert.True(File.Exists(bagInfoFile));
        }



        [Fact]

        public void Test_BagInfo_Content()
        {
            _bagger.CreateBag(_tmpDir, ChecksumAlgorithm.MD5);
            var expected = new Dictionary<string, Func<string, bool>>(StringComparer.OrdinalIgnoreCase)
            {
                // must match your generator output
                ["Bag-Software-Agent"] = v => v.StartsWith("bagit.net v"),
                ["Bagging-Date"] = v => DateTime.TryParse(v, out _),
                ["Payload-Oxum"] = v => v.Contains("."),
                ["BagIt-Version"] = v => !string.IsNullOrWhiteSpace(v)
            };


            var kvp = BagInfo.GetBagInfoAsKeyValuePairs(Path.Combine(_tmpDir, "bag-info.txt"));

            foreach (var (key, validator) in expected)
            {
                var entry = kvp.SingleOrDefault(e =>
                    string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));

                Assert.False(entry.Equals(default(KeyValuePair<string, string>)),
                    $"Missing key '{key}' in bag-info.txt");

                Assert.True(validator(entry.Value),
                    $"Value for '{key}' is invalid: {entry.Value}");
            }
        }
            
        
    }

       
}
