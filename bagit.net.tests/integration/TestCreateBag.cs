using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.integration
{
    public class TestCreateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly ICreationService _creationService;

        public TestCreateBag()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-bag");
            _serviceProvider = CreateServiceConfigurator.BuildServiceProvider();
            _creationService = _serviceProvider.GetRequiredService<ICreationService>();
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
        public void CreateBag_Throws_On_Invalid_Directories()
        {
            Assert.Throws<DirectoryNotFoundException>(() => _creationService.CreateBag(Path.Combine(_tmpDir, "Foo"), ChecksumAlgorithm.MD5));
            Assert.Throws<ArgumentNullException>(() => _creationService.CreateBag(null!, ChecksumAlgorithm.MD5));
        }

    }
}
