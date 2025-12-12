using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.unit
{
    public class ManifestServiceTests : IDisposable
    {
        private string _testDir = string.Empty;
        private readonly ServiceProvider _serviceProvider;
        private readonly IManifestService _manifestService;
        private readonly IMessageService _messageService;
        private readonly IChecksumService _checksumService;
        private readonly ITestOutputHelper _output;
        private readonly IEnumerable<ChecksumAlgorithm> _algorithms;
        private readonly int _processes = 1;
        public ManifestServiceTests(ITestOutputHelper output)
        {
            _serviceProvider = ManifestServiceConfigurator.BuildServiceProvider();
            _manifestService = _serviceProvider.GetRequiredService<IManifestService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
            _checksumService = _serviceProvider.GetRequiredService<IChecksumService>();
            _messageService.Clear();
            _output = output;
            _algorithms = new List<ChecksumAlgorithm>() { 
                ChecksumAlgorithm.MD5, 
                ChecksumAlgorithm.SHA1, 
                ChecksumAlgorithm.SHA256, 
                ChecksumAlgorithm.SHA384, 
                ChecksumAlgorithm.SHA512 };
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Theory]
        [InlineData(ChecksumAlgorithm.MD5, "manifest-md5.txt")]
        [InlineData(ChecksumAlgorithm.SHA1, "manifest-sha1.txt")]
        [InlineData(ChecksumAlgorithm.SHA256, "manifest-sha256.txt")]
        [InlineData(ChecksumAlgorithm.SHA384, "manifest-sha384.txt")]
        [InlineData(ChecksumAlgorithm.SHA512, "manifest-sha512.txt")]
        [Trait("Category", "Unit")]
        public async Task Test_Create_Payload_Manifest(ChecksumAlgorithm algorithm, string manifestName)
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("data-only");
            await _manifestService.CreatePayloadManifest(_testDir, new List<ChecksumAlgorithm> { algorithm }, _processes);
            Assert.True(File.Exists(Path.Combine(_testDir, manifestName)));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Test_Create_Multiple_Payload_Manifests()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("data-only");
            await _manifestService.CreatePayloadManifest(_testDir, _algorithms, _processes);
            foreach (var algorithm in _algorithms)
            {
                var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
                var manifestPath = Path.Combine(_testDir, $"manifest-{algorithmCode}.txt");
                Assert.True(File.Exists(manifestPath));
                var lines = File.ReadAllLines(manifestPath);
                Assert.True(lines.Length > 0, $"Manifest {manifestPath} should have at least one line.");
            }
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
            _testDir = TestHelpers.PrepareTempTestDataDir("no-tag-manifest");
            _manifestService.CreateTagManifestFile(_testDir, new List<ChecksumAlgorithm>() { algorithm });
            var tagManifestPath = Path.Combine(_testDir, manifestName);
            Assert.True(File.Exists(tagManifestPath));
            var lines = File.ReadAllLines(tagManifestPath);
            Assert.True(lines.Length > 0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Create_Multiple_Tag_Manifests()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-valid");
            _manifestService.CreateTagManifestFile(_testDir, _algorithms);
            foreach (var algorithm in _algorithms)
            {
                var algorithmCode = _checksumService.GetAlgorithmCode(algorithm);
                var tagmanifestPath = Path.Combine(_testDir, $"tagmanifest-{algorithmCode}.txt");
                Assert.True(File.Exists(tagmanifestPath));
                var lines = File.ReadAllLines(tagmanifestPath);
                Assert.True(lines.Length > 0);
            }
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("valid-bag")]
        [InlineData("valid-bag-bagitnet")]
        public void Test_Validate_Manifests(string bag) {
            _testDir = TestHelpers.PrepareTempTestDataDir(bag);
            var manifests = new List<string>() { "manifest-sha256.txt", "tagmanifest-sha256.txt"};
            
            foreach (var manifest in manifests)
            {
                var manifestFile = Path.Combine(_testDir, manifest);
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
            _testDir = TestHelpers.PrepareTempTestDataDir("valid-bag");
            var manifests = new List<string>() { "manifest-sha256.txt", "tagmanifest-sha256.txt" };
            foreach (var manifest in manifests)
            {
                var manifestFile = Path.Combine(_testDir, manifest);
                var kvp = _manifestService.GetManifestAsKeyValuePairs(manifestFile);
                Assert.True(kvp.Count > 0);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Validate_Bag_For_Completeness()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("valid-bag");
            _manifestService.ValidateManifestFilesCompleteness(_testDir);
            Assert.Empty(_messageService.GetAll());

        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Validate_Incomplete_Bag_For_Completeness()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-incomplete");
            _manifestService.ValidateManifestFilesCompleteness(_testDir);
            var messages = _messageService.GetAll();
            Assert.NotEmpty(messages);
            foreach(var message in messages)
            {
                _output.WriteLine($"{message}");
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Validate_Manifests_Multithread()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-incomplete");
            _manifestService.ValidateManifestFiles(_testDir, 6);
            var messages = _messageService.GetAll();
            Assert.Empty(messages);
        }

    }

}
