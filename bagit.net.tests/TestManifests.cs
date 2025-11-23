using bagit.net.interfaces;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests
{
    public class TestManifests : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly Bagger _bagger;
        private readonly IChecksumService _checksumService;
        public TestManifests() {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-bag");
            _serviceProvider = ServiceConfigurator.BuildServiceProvider<Bagger>();
            _bagger = _serviceProvider.GetRequiredService<Bagger>();
            _checksumService = _serviceProvider.GetRequiredService<IChecksumService>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
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
            var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
            _bagger.CreateBag(tmpDir, algorithm);
            Assert.True(File.Exists(Path.Combine(tmpDir, $"manifest-{algorithmCode}.txt")));
        }

        [Fact]
        public void Test_MD5Manifest_Content_Is_Valid() {
            var algorithm = ChecksumAlgorithm.MD5;
            var algorithmCode = _checksumService.GetAlgorithmCode(ChecksumAlgorithm.MD5);
            _bagger.CreateBag(_tmpDir, algorithm);
            var manifestService = _serviceProvider.GetRequiredService<IManifestService>();
            var kvp = manifestService.GetManifestAsKeyValuePairs(Path.Combine(_tmpDir, $"manifest-{algorithmCode}.txt"));
            Assert.Equal(2, kvp.Count);

            var expected = new[]
            {
                ("data/test-bag/hello.txt", "8b1a9953c4611296a827abf8c47804d7"),
                ("data/test-bag/subdir/test.txt", "098f6bcd4621d373cade4e832627b4f6")
            };

            foreach (var (file, checksum) in expected)
            {
                var foundPair = kvp.FirstOrDefault(e => e.Key == checksum);
                Assert.Equal(file, foundPair.Value);
            }
        }
    }
}
