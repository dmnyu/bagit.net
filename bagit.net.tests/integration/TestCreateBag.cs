using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace bagit.net.tests.integration
{
    public class TestCreateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly Bagger _bagger;

        public TestCreateBag()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-bag");
            _serviceProvider = ServiceConfigurator.BuildServiceProvider<Bagger>();
            _bagger = _serviceProvider.GetRequiredService<Bagger>();
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
            Assert.Throws<ArgumentNullException>(() => _bagger.CreateBag(null, ChecksumAlgorithm.MD5));
            Assert.Throws<DirectoryNotFoundException>(() => _bagger.CreateBag(Path.Combine(_tmpDir, "Foo"), ChecksumAlgorithm.MD5));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Bag_Exists()
        {

            var dataDir = Path.Combine(_testDir, "data");
            var ex = Record.Exception(() => _bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5));
            Assert.Null(ex);
            Assert.True(Directory.Exists(_testDir));
            Assert.True(Directory.Exists(dataDir));
            Assert.True(File.Exists(Path.Combine(dataDir, "hello.txt")));
            Assert.True(Directory.Exists(Path.Combine(dataDir, "subdir")));
            Assert.True(File.Exists(Path.Combine(dataDir, "subdir", "test.txt")));
        }



        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Bag_Has_Valid_BagitTxt_File()
        {

            _bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5);
            var bagitTxt = Path.Combine(_testDir, "bagit.txt");
            Assert.True(File.Exists(bagitTxt));

            var content = File.ReadAllText(bagitTxt, Encoding.UTF8);
            Assert.Contains($"BagIt-Version: {Bagit.BAGIT_VERSION}", content);
            Assert.Contains("Tag-File-Character-Encoding: UTF-8", content);

            var lines = File.ReadAllLines(bagitTxt, Encoding.UTF8);
            Assert.Equal(2, lines.Length);

        }
    }
}
