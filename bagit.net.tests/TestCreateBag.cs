namespace bagit.net.tests
{
    public class CreateBagTests
    {
        private string TempTestDataDir => Path.Combine(Path.GetTempPath(), "BagitTest_" + Guid.NewGuid());

        [Fact]
        public void CreateBag_Throws_On_Invalid_Directories()
        {
            var tempDir = TempTestDataDir;
            CopyTestData(tempDir);

            
            var bagit = new Bagit();

            Assert.Throws<ArgumentNullException>(() =>
                bagit.CreateBag(null));

            Assert.Throws<DirectoryNotFoundException>(() =>
                bagit.CreateBag(Path.Combine(tempDir, "Foo")));

            Directory.Delete(tempDir, true);
        }


        [Fact]
        public void CreateBag_DoesNotThrow_On_Valid_Directory()
        {
            var tempDir = TempTestDataDir;
            CopyTestData(tempDir);

            var validDir = Path.Combine(tempDir, "dir");
            var bagit = new Bagit();

            var ex = Record.Exception(() => bagit.CreateBag(validDir));
            Assert.Null(ex);

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public void Test_Bag_Exists()
        {
            var tempDir = TempTestDataDir;
            CopyTestData(tempDir);

            var validDir = Path.Combine(tempDir, "dir");
            var dataDir = Path.Combine(validDir, "data");

            var bagit = new Bagit();
            bagit.CreateBag(validDir);

            
            Assert.True(Directory.Exists(validDir));
            Assert.True(Directory.Exists(dataDir));
            Assert.True(File.Exists(Path.Combine(dataDir, "hello.txt")));
            Assert.True(Directory.Exists(Path.Combine(dataDir, "subdir")));
            Assert.True(File.Exists(Path.Combine(dataDir, "subdir", "test.txt")));

            Directory.Delete(tempDir, true);
        }

        private void CopyTestData(string dest)
        {
            var originalDir = Path.Combine(AppContext.BaseDirectory, "TestData");
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.GetFiles(originalDir))
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));

            foreach (var dir in Directory.GetDirectories(originalDir))
                CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
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
