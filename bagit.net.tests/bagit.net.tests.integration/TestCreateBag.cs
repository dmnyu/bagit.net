using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.integration
{
    public class TestCreateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly ICreationService _creationService;
        private readonly IMessageService _messageService;
        private readonly ITestOutputHelper _output;
        private readonly int _processes = 1;

        public TestCreateBag(ITestOutputHelper output)
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-bag");
            _serviceProvider = DefaultServiceConfigurator.BuildServiceProvider();
            _creationService = _serviceProvider.GetRequiredService<ICreationService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
            _output = output;
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Get_Version()
        {
            Assert.NotEmpty(Bagit.VERSION);
            Assert.Matches(@"^\d+\.\d+$", Bagit.BAGIT_VERSION);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Create_Bag()
        {
            _creationService.CreateBag(Path.Combine(_tmpDir, "dir"), new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, null, _processes);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            foreach (var message in messages)
                _output.WriteLine($"{message}");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Create_Bag_With_Metdata()
        {
            _creationService.CreateBag(Path.Combine(_tmpDir, "dir"), new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, Path.Combine(_tmpDir, "metadata.txt"), _processes);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            foreach (var message in messages)
                _output.WriteLine($"{message}");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CreateBag_Throws_On_Invalid_Directories()
        {
            _creationService.CreateBag(Path.Combine(_tmpDir, "Foo"), new List<ChecksumAlgorithm>() { ChecksumAlgorithm.MD5 }, null, _processes);
            var messages = _messageService.GetAll();
            Assert.True(MessageHelpers.HasError(messages));
            foreach (var message in messages)
                _output.WriteLine($"{message}");
        }



    }
}
