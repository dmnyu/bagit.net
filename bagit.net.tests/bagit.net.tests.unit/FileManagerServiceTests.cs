using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.unit
{
    public class FileManagerServiceTests : IDisposable
    {
        private readonly string _tmpDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly IFileManagerService _fileManagerService;
        public FileManagerServiceTests()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _serviceProvider = FileManagerServiceConfigurator.BuildServiceProvider();
            _fileManagerService = _serviceProvider.GetRequiredService<IFileManagerService>();
        }
        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Path.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Create_Temp_Directory()
        {
            var _dir = Path.Combine(_tmpDir, "dir");
            var _tempDir = _fileManagerService.CreateTempDirectory(_dir);
            Assert.True(Directory.Exists(_tempDir));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Move_Contents_To_Temp_Directory()
        {
            var _dir = Path.Combine(_tmpDir, "dir");
            var _tempDir = _fileManagerService.CreateTempDirectory(_dir);
            _fileManagerService.MoveContentsOfDirectory(_dir, _tempDir);
            var _files = Directory.GetFiles(_dir);
            Assert.Empty(_files);
            var _directories = Directory.GetDirectories(_dir);
            Assert.Single(_directories);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Move_Temp_Dir_To_Data()
        {
            var _dir = Path.Combine(_tmpDir, "dir");
            var _tempDir = _fileManagerService.CreateTempDirectory(_dir);
            _fileManagerService.MoveContentsOfDirectory(_dir, _tempDir);
            var _dataDir = Path.Combine(_dir, "data");
            _fileManagerService.MoveDirectory(_tempDir, _dataDir);
            var _directories = Directory.GetDirectories(_dir);
            Assert.Single(_directories);
            Assert.Equal("data", Path.GetFileName(_directories[0]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Valid_UTF8()
        {
            var _validDir = Path.Combine(_tmpDir, "valid-bag");
            var _validFile = Path.Combine(_validDir, "bag-info.txt");
            Assert.True(_fileManagerService.IsValidUTF8(_validFile));
        }

    }
}
