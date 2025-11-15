using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.tests
{
    public class TestManifests : IDisposable
    {
        private readonly string _tmpDir;
        private readonly Bagger _bagger;
        public TestManifests() {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _bagger = new Bagger();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Theory]
        [InlineData(ChecksumAlgorithm.MD5)]
        [InlineData(ChecksumAlgorithm.SHA1)]
        [InlineData(ChecksumAlgorithm.SHA256)]
        [InlineData(ChecksumAlgorithm.SHA384)]
        [InlineData(ChecksumAlgorithm.SHA512)]
        public void Test_Manifest_Exists(ChecksumAlgorithm algorithm)
        {
            var tmpDir = TestHelpers.PrepareTempTestData();
            var bagger = new Bagger();
            var algorithmCode = Checksum.GetAlgorithmCode(algorithm);
            bagger.CreateBag(tmpDir, algorithm);
            Assert.True(File.Exists(Path.Combine(tmpDir, $"manifest-{algorithmCode}.txt")));
        }

        [Fact]
        public void Test_MD5Manifest_Content_Is_Valid() {
            var algorithm = ChecksumAlgorithm.MD5;
            var algorithmCode = Checksum.GetAlgorithmCode(ChecksumAlgorithm.MD5);
            _bagger.CreateBag(_tmpDir, algorithm);
            var kvp = Manifest.GetManifestAsKeyValuePairs(Path.Combine(_tmpDir, $"manifest-{algorithmCode}.txt"));


            var expected = new[]
            {
                ("data/Dir/hello.txt", "8b1a9953c4611296a827abf8c47804d7"),
                ("data/Dir/subdir/test.txt", "098f6bcd4621d373cade4e832627b4f6")
            };

            foreach (var (file, checksum) in expected)
            {
                var foundPair = kvp.FirstOrDefault(e => e.Key == checksum);
                Assert.NotNull(foundPair);
                Assert.Equal(file, foundPair.Value);
            }
        }
    }
}
