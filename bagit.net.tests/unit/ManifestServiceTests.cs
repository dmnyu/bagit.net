using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.unit
{
    public class ManifestServiceTests : IDisposable
    {
        private readonly string _tmpDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly IManifestService _manifestService;
        public ManifestServiceTests()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _serviceProvider = ManifestServiceConfigurator.BuildServiceProvider();
            _manifestService = _serviceProvider.GetRequiredService<IManifestService>();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(ChecksumAlgorithm.MD5, "manifest-md5.txt")]
        [InlineData(ChecksumAlgorithm.SHA1, "manifest-sha1.txt")]
        [InlineData(ChecksumAlgorithm.SHA256, "manifest-sha256.txt")]
        [InlineData(ChecksumAlgorithm.SHA384, "manifest-sha384.txt")]
        [InlineData(ChecksumAlgorithm.SHA512, "manifest-sha512.txt")]
        public void Test_Create_Payload_Manifest(ChecksumAlgorithm algorithm, string manifestName)
        {
            var dataBag = Path.Combine(_tmpDir, "data-only");
            _manifestService.CreatePayloadManifest(dataBag, algorithm);
            Assert.True(File.Exists(Path.Combine(dataBag, manifestName)));
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(ChecksumAlgorithm.MD5, "tagmanifest-md5.txt")]
        [InlineData(ChecksumAlgorithm.SHA1, "tagmanifest-sha1.txt")]
        [InlineData(ChecksumAlgorithm.SHA256, "tagmanifest-sha256.txt")]
        [InlineData(ChecksumAlgorithm.SHA384, "tagmanifest-sha384.txt")]
        [InlineData(ChecksumAlgorithm.SHA512, "tagmanifest-sha512.txt")]
        public void Test_Create_Tag_Manifest(ChecksumAlgorithm algorithm, string manifestName)
        {
            var dataBag = Path.Combine(_tmpDir, "no-tag-manifest");
            _manifestService.CreateTagManifestFile(dataBag, algorithm);
            Assert.True(File.Exists(Path.Combine(dataBag, manifestName)));
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("valid-bag")]
        [InlineData("valid-bag-bagitnet")]
        public void Test_Validate_Manifests(string bag) {
            var validBag = Path.Combine(_tmpDir, bag);
            var manifests = new List<string>() { "manifest-sha256.txt", "tagmanifest-sha256.txt"};
            
            foreach (var manifest in manifests)
            {
                var manifestFile = Path.Combine(validBag, manifest);
                Console.WriteLine($"Checking file: {manifestFile}, exists: {File.Exists(manifestFile)}");
                Console.WriteLine($"Temp dir: {Path.GetTempPath()}");


                var ex = Record.Exception(() => _manifestService.ValidateManifestFile(manifestFile));
                Assert.Null(ex);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Manifest_KVP()
        {
            var validBag = Path.Combine(_tmpDir, "valid-bag");
            var manifests = new List<string>() { "manifest-sha256.txt", "tagmanifest-sha256.txt" };
            foreach (var manifest in manifests)
            {
                var manifestFile = Path.Combine(validBag, manifest);
                var kvp = _manifestService.GetManifestAsKeyValuePairs(manifestFile);
                Assert.True(kvp.Count > 0);
            }
        }
    }

}
