namespace bagit.net.tests
{
    public class TestLogging : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _testDir;
        private readonly string _logFile;
        private readonly Bagger _bagger;

        public TestLogging()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _testDir = Path.Combine(_tmpDir, "test-data");
            _logFile = Path.Combine(_tmpDir, "bagit.net.log");
            Directory.CreateDirectory(_testDir);
            Bagit.InitLogger(_logFile);
            _bagger = new Bagger();
        }

        public void Dispose() {
            Serilog.Log.CloseAndFlush();
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
            
        }

        [Fact]
        public void Test_Logfile_Exists()
        {
            _bagger.CreateBag(_testDir, ChecksumAlgorithm.MD5);
            Assert.True(File.Exists(_logFile));

        }
    }
}
