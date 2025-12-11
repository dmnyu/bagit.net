using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.unit
{
    public class CreationServiceTests : IDisposable
    {
        private readonly string _testDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly ICreationService _creationService;
        private readonly IMessageService _messageService;
        private readonly ITestOutputHelper _output;
        private readonly int _processes = 1;

        public CreationServiceTests(ITestOutputHelper output)
        {
            _testDir = TestHelpers.PrepareTempTestData();
            _serviceProvider = DefaultServiceConfigurator.BuildServiceProvider();
            _creationService = _serviceProvider.GetRequiredService<ICreationService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
            _output = output;
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Fact]
        [Trait("Category", "unit")]
        public async Task Create_Bag()
        {
            await _creationService.CreateBag(Path.Combine(_testDir, "dir"), new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, null, _processes);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            foreach (var message in messages)
                _output.WriteLine($"{message}");
        }

        [Fact]
        [Trait("Category", "unit")]
        public async Task Create_Bag_With_Metdata()
        {
            await _creationService.CreateBag(Path.Combine(_testDir, "dir"), new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, Path.Combine(_testDir, "metadata.txt"), _processes);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            foreach (var message in messages)
                _output.WriteLine($"{message}");
            foreach(var line in File.ReadAllLines(Path.Combine(_testDir, "dir", "bag-info.txt"))) {
                _output.WriteLine(line);
            }
        }

        [Fact]
        [Trait("Category", "unit")]
        public async Task CreateBag_Throws_On_Invalid_Directories()
        {
            await _creationService.CreateBag(Path.Combine(_testDir, "Foo"), new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, null, _processes);
            var messages = _messageService.GetAll();
            Assert.True(MessageHelpers.HasError(messages));
            foreach (var message in messages)
                _output.WriteLine($"{message}");
        }

        [Fact]
        [Trait("Category", "unit")]
        public async Task Create_Bag_100_Files()
        {
            var hundredFilesDir = Path.Combine(_testDir, "dir-100-files");
            await _creationService.CreateBag(hundredFilesDir, new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, null, _processes);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            var payloadManifest = Path.Combine(_testDir, "dir-100-files", "manifest-md5.txt");
            var payloadLines = File.ReadAllLines(payloadManifest);
            Assert.Equal(100, payloadLines.Length);
        }

        [Fact]
        [Trait("Category", "unit")]
        public async Task Create_Bag_100_Files_Threaded()
        {
            var hundredFilesDir = Path.Combine(_testDir, "dir-100-files");
            await _creationService.CreateBag(hundredFilesDir, new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, null, 4);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            var payloadManifest = Path.Combine(_testDir, "dir-100-files", "manifest-md5.txt");
            var payloadLines = File.ReadAllLines(payloadManifest);
            Assert.Equal(100, payloadLines.Length);
        }
    }
}
