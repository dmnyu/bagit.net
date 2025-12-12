using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.unit
{
    public class FileManagerServiceTests : IDisposable
    {
        private string _testDir = string.Empty;
        private readonly ServiceProvider _serviceProvider;
        private readonly IFileManagerService _fileManagerService;
        public FileManagerServiceTests()
        {
            _serviceProvider = FileManagerServiceConfigurator.BuildServiceProvider();
            _fileManagerService = _serviceProvider.GetRequiredService<IFileManagerService>();
        }
        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Path.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Create_Temp_Directory()
        {
            var testDir = TestHelpers.PrepareTempTestDataDir("dir");
            var tempDir = _fileManagerService.CreateTempDirectory(testDir);
            Assert.True(Directory.Exists(tempDir));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Move_Contents_To_Temp_Directory()
        {
            var _testDir = TestHelpers.PrepareTempTestDataDir("dir");
            var tmpDir = _fileManagerService.CreateTempDirectory(_testDir);
            _fileManagerService.MoveContentsOfDirectory(_testDir, tmpDir);
            var files = Directory.GetFiles(_testDir);
            Assert.Empty(files);
            var directories = Directory.GetDirectories(tmpDir);
            Assert.Single(directories);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Move_Temp_Dir_To_Data()
        {
            var _testDir = TestHelpers.PrepareTempTestDataDir ("dir");
            var tmpDir = _fileManagerService.CreateTempDirectory(_testDir );
            _fileManagerService.MoveContentsOfDirectory(_testDir, tmpDir);
            var dataDir = Path.Combine(_testDir, "data");
            _fileManagerService.MoveDirectory(tmpDir, dataDir);
            var directories = Directory.GetDirectories(_testDir);
            Assert.Single(directories);
            Assert.Equal("data", Path.GetFileName(directories[0]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Valid_UTF8()
        {
            var _testDir = TestHelpers.PrepareTempTestDataDir("valid-bag");
            var validFile = Path.Combine(_testDir, "bag-info.txt");
            Assert.True(_fileManagerService.IsValidUTF8(validFile));
        }

    }
}
