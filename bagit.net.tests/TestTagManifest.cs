using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace bagit.net.tests
{
    public class TestTagManifest : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly Bagger _bagger;
        public TestTagManifest()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-bag");
            _bagger = new Bagger();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        public void Test_TagManifest_Exists()
        {
            _bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5);
            var tagManifestFile = Path.Join(_testDir, "tagmanifest-md5.txt");
            Assert.True(File.Exists(tagManifestFile));
        }

        [Fact]
        public void Test_TagManifest_Does_Not_Contain_Tag_Manifest()
        {
            _bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5);
            var tagManifestFile = Path.Combine(_testDir, "tagmanifest-md5.txt");
            var kvp = Manifest.GetManifestAsKeyValuePairs(tagManifestFile);
            Assert.DoesNotContain(kvp, pair =>
                pair.Value.Equals("tagmanifest-md5.txt", StringComparison.OrdinalIgnoreCase)
            );
        }

        [Fact]
        public void Test_TagManifest_Content_Is_Valid()
        {
            var algorithm = ChecksumAlgorithm.MD5;
            var algorithmCode = Checksum.GetAlgorithmCode(algorithm);
            _bagger.CreateBag(_tmpDir, algorithm);

            var kvp = Manifest.GetManifestAsKeyValuePairs(
                Path.Combine(_tmpDir, $"tagmanifest-{algorithmCode}.txt"));

            var dict = kvp.ToDictionary(k => k.Key, v => v.Value);

            var expected = new[]
            {
                ("bag-info.txt","86a0e026bc605fdf028a87d796deb098"),
                ("bagit.txt", "97f882dee1bde18065992d2d7b471f0e"),
                ("manifest-md5.txt", "0b8581813cd41d9efd767daf7e2feed7")
            };

            foreach (var (file, checksum) in expected)
            {
                Assert.True(dict.ContainsKey(checksum), $"Checksum {checksum} not found in tagmanifest");
                Assert.Equal(file, dict[checksum]);
            }
        }
    }
}
