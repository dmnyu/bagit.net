using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.unit
{
    
    public class ChecksumServiceUnitTests : IDisposable
    {
        readonly ServiceProvider _serviceProvider;
        readonly IChecksumService _checksumService;
        readonly string _tmpDir;

        public ChecksumServiceUnitTests()
        {
            _serviceProvider = ChecksumServiceConfigurator.BuildServiceProvider();
            _checksumService = _serviceProvider.GetRequiredService<IChecksumService>();
            _tmpDir = TestHelpers.PrepareTempTestData();
        }

        public void Dispose() {
            _serviceProvider.Dispose();
            if (_tmpDir != null)
            {
                Directory.Delete(_tmpDir, true);
            }
        }

        [Fact]
        public void Test_Calculate_Checksum()
        {
            var goldenFile = Path.Combine(_tmpDir, "golden-files", "golden-file.txt");
            var calculatedChecksum = _checksumService.CalculateChecksum(goldenFile, ChecksumAlgorithm.MD5);
            Assert.Equal("82715cc04f1900c87118d8780fc0b04a", calculatedChecksum);
        }

        [Theory]
        [InlineData(ChecksumAlgorithm.MD5, "d41d8cd98f00b204e9800998ecf8427e")]
        [InlineData(ChecksumAlgorithm.SHA1, "da39a3ee5e6b4b0d3255bfef95601890afd80709")]
        [InlineData(ChecksumAlgorithm.SHA256, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData(ChecksumAlgorithm.SHA384, "38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")]
        [InlineData(ChecksumAlgorithm.SHA512, "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")]

        public void Test_Compare_Checksums_For_Empty_File(ChecksumAlgorithm algorithm, string expectedChecksum)
        {
            var emptyFile = Path.Combine(_tmpDir, "empty.txt");
            File.WriteAllBytes(emptyFile, Array.Empty<byte>());
            AssertFileChecksum(emptyFile, expectedChecksum, algorithm);
        }
        
        [Theory]
        [InlineData(ChecksumAlgorithm.MD5, "82715cc04f1900c87118d8780fc0b04a")]
        [InlineData(ChecksumAlgorithm.SHA1, "f8c5bf7a5917d0ba6ab65732df5341475bbe6291")]
        [InlineData(ChecksumAlgorithm.SHA256, "8c219e899115cdfb4ec7c2f03353a55a54a3e48a952439f1f55d57d3a2146e69")]
        [InlineData(ChecksumAlgorithm.SHA384, "548a36e2e8de7a9a30bda5db1557ec3a4c9233a01a93c79e52bc2124f1e678ca0f3ee65abcd612d5fc58d7c6fc169498")]
        [InlineData(ChecksumAlgorithm.SHA512, "07893577f0b21751081feb2817bc817bdf7d8084aca7e40a8a727be89d731c05be418974d2ee4834bf6722775e9712a5d4dd2661cd666549eb571c8643443942")]
        public void Test_Compare_Checksums_For_Golden_File(ChecksumAlgorithm algorithm, string expectedChecksum)
        {
            var goldenFile = Path.Combine(_tmpDir, "golden-files", "golden-file.txt");
            AssertFileChecksum(goldenFile, expectedChecksum, algorithm);
        }

        [Theory]
        [InlineData(ChecksumAlgorithm.MD5, "md5")]
        [InlineData(ChecksumAlgorithm.SHA1, "sha1")]
        [InlineData(ChecksumAlgorithm.SHA256, "sha256")]
        [InlineData(ChecksumAlgorithm.SHA384, "sha384")]
        [InlineData(ChecksumAlgorithm.SHA512, "sha512")]
        public void Test_Get_Checksum_Codes(ChecksumAlgorithm algorithm, string checksumCode)
        {
            Assert.Equal(checksumCode, _checksumService.GetAlgorithmCode(algorithm));
        }

        private void AssertFileChecksum(string filePath, string expected, ChecksumAlgorithm algorithm) =>
            Assert.True(_checksumService.CompareChecksum(filePath, expected, algorithm), $"{algorithm} failed");

    }
}
