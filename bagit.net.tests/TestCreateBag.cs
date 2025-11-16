using System;
using System.IO;
using System.Text;
using Xunit;

namespace bagit.net.tests
{
    public class TestCreateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly Bagger bagger;

        public TestCreateBag()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-bag");
            bagger = new Bagger();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        public void Get_Version()
        {
            Assert.NotEmpty(Bagit.VERSION);
            Assert.Matches(@"^\d+\.\d+$", Bagit.BAGIT_VERSION);
        }


        [Fact]
        public void CreateBag_Throws_On_Invalid_Directories()
        {
            Assert.Throws<ArgumentNullException>(() => bagger.CreateBag(null, ChecksumAlgorithm.MD5));
            Assert.Throws<DirectoryNotFoundException>(() => bagger.CreateBag(Path.Combine(_tmpDir, "Foo"), ChecksumAlgorithm.MD5));
        }

        [Fact]
        public void Test_Bag_Exists()
        {

            var dataDir = Path.Combine(_testDir, "data");
            var ex = Record.Exception(() => bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5));
            Assert.Null(ex);
            Assert.True(Directory.Exists(_testDir));
            Assert.True(Directory.Exists(dataDir));
            Assert.True(File.Exists(Path.Combine(dataDir, "hello.txt")));
            Assert.True(Directory.Exists(Path.Combine(dataDir, "subdir")));
            Assert.True(File.Exists(Path.Combine(dataDir, "subdir", "test.txt")));
        }



        [Fact]
        public void Test_Bag_Has_Valid_BagitTxt_File()
        {

            bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5);
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
