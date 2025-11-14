using System;
using System.IO;
using System.Text;
using Xunit;

namespace bagit.net.tests
{
    public class TestCreateBag
    {
        [Fact]
        public void Get_Version()
        {
            Assert.NotEmpty(Bagit.VERSION);
            Assert.Matches(@"^\d+\.\d+$", Bagit.BAGIT_VERSION);
        }


        [Fact]
        public void CreateBag_Throws_On_Invalid_Directories()
        {
            var tempDir = PrepareTempTestData();
            var bagger = new Bagger();

            try
            {
                Assert.Throws<ArgumentNullException>(() => bagger.CreateBag(null!));
                Assert.Throws<DirectoryNotFoundException>(() => bagger.CreateBag(Path.Combine(tempDir, "Foo")));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Test_Bag_Exists()
        {
            var tempDir = PrepareTempTestData();
            var validDir = Path.Combine(tempDir, "dir");
            var dataDir = Path.Combine(validDir, "data");

            var bagger = new Bagger();
            try
            {
                var ex = Record.Exception(() => bagger.CreateBag(validDir));

                Assert.Null(ex);
                Assert.True(Directory.Exists(validDir));
                Assert.True(Directory.Exists(dataDir));
                Assert.True(File.Exists(Path.Combine(dataDir, "hello.txt")));
                Assert.True(Directory.Exists(Path.Combine(dataDir, "subdir")));
                Assert.True(File.Exists(Path.Combine(dataDir, "subdir", "test.txt")));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Test_Bag_Has_Valid_BagitTxt_File()
        {
            var tempDir = PrepareTempTestData();
            try
            {
                var bagger = new Bagger();
                bagger.CreateBag(tempDir);
                var bagitTxt = Path.Combine(tempDir, "bagit.txt");
                Assert.True(File.Exists(bagitTxt));

                var content = File.ReadAllText(bagitTxt, Encoding.UTF8);
                Assert.Contains($"BagIt-Version: {Bagit.BAGIT_VERSION}", content);
                Assert.Contains("Tag-File-Character-Encoding: UTF-8", content);

                var lines = File.ReadAllLines(bagitTxt, Encoding.UTF8);
                Assert.Equal(2, lines.Length);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        //helper functions
        private string PrepareTempTestData()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "BagitTest_" + Guid.NewGuid());
            var originalDir = Path.Combine(AppContext.BaseDirectory, "TestData");

            CopyDirectory(originalDir, tempDir);
            return tempDir;
        }

       
        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)));

            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }
}
