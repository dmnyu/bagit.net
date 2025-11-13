using Microsoft.VisualBasic;

namespace bagit.net.tests
{
    public class BagitTests
    {
        string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
        readonly static Bagit bagit = new Bagit();

        [Fact]
        public void CreateBag_Throws_On_Nonexistant_Directory()
        {
            Assert.Throws<DirectoryNotFoundException>(() =>
                bagit.CreateBag(Path.Combine(TestDataDir, "Foo")));
        }

        [Fact]
        public void CreateBag_Throws_On_Null_Value()
        {
            Assert.Throws<ArgumentNullException>(() =>
                bagit.CreateBag(null));
        }

        [Fact]
        public void CreateBag_DoesNotThrow_On_ValidDirectory()
        {
            var validDir = Path.Combine(TestDataDir, "Dir");
            var exception = Record.Exception(() => bagit.CreateBag(validDir));
            Assert.Null(exception);
        }

        [Fact]
        public void CreateBag_Created_Data_Dir()
        {
            Assert.True(Directory.Exists(Path.Combine(TestDataDir, "Dir", "data")));
        }

        [Fact]
        public void CreateBag_Moved_Files()
        {
            Assert.True(File.Exists(Path.Combine(TestDataDir, "Dir", "data", "hello.txt")));
            Assert.True(Directory.Exists(Path.Combine(TestDataDir, "Dir", "data", "subdir")));
            Assert.True(File.Exists(Path.Combine(TestDataDir, "Dir", "data", "subdir", "test.txt")));
        }
    }
}

