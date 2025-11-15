using bagit.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.tests
{
    public class TestChecksums : IDisposable
    {
        private readonly string _tmpDir;

        public TestChecksums()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
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
        [InlineData(ChecksumAlgorithm.MD5, "8b1a9953c4611296a827abf8c47804d7")]
        [InlineData(ChecksumAlgorithm.SHA1, "f7ff9e8b7bb2e09b70935a5d785e0cc5d9d0abf0")]
        [InlineData(ChecksumAlgorithm.SHA256, "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969")]
        [InlineData(ChecksumAlgorithm.SHA384, "3519fe5ad2c596efe3e276a6f351b8fc0b03db861782490d45f7598ebd0ab5fd5520ed102f38c4a5ec834e98668035fc")]
        [InlineData(ChecksumAlgorithm.SHA512, "3615f80c9d293ed7402687f94b22d58e529b8cc7916f8fac7fddf7fbd5af4cf777d3d795a7a00a16bf7e7f3fb9561ee9baae480da9fe7a18769e71886b03f315")]
        public void Test_Calculate_Checksums_For_File(ChecksumAlgorithm algorithm, string checksum)
        {
            var file = Path.Combine(_tmpDir, "Dir", "hello.txt");
            AssertFileChecksum(file, checksum, algorithm);
        }

        [Fact]
        public void Test_Compare_Invalid_Checksum_For_File()
        {
                var file = Path.Combine(_tmpDir, "Dir", "hello.txt");
                Assert.False(Checksum.CompareChecksum(file, "d41d8cd98f00b204e9800998ecf8427e", ChecksumAlgorithm.MD5));
 
        }

        [Theory]
        [InlineData("8b1a9953c4611296a827abf8c47804d7")]
        [InlineData(" 8b1a9953c4611296a827abf8c47804d7 ")]
        [InlineData("8B1A9953C4611296A827ABF8C47804D7")]
        [InlineData("8b1a9953-c461-1296-a827-abf8c47804d7")]
        public void Test_Checksum_Variants(string variant)
        {
            var file = Path.Combine(_tmpDir, "Dir", "hello.txt");
            Assert.True(Checksum.CompareChecksum(file, variant, ChecksumAlgorithm.MD5));   
        }

        [Fact]
        public void Test_Checksum_Algorithm_Mismatch()
        {
            var file = Path.Combine(_tmpDir, "Dir", "hello.txt");
            Assert.Throws<ArgumentException>(() => Checksum.CompareChecksum(file, "8b1a9953c4611296a827abf8c47804d7", ChecksumAlgorithm.SHA1));
        }

        [Theory]
        [InlineData("")]
        [InlineData("     ")]
        [InlineData(null)]
        public void Test_Blank_Checksum(string? checksum)
        {
            var file = Path.Combine(_tmpDir, "Dir", "hello.txt");
            Assert.Throws<ArgumentNullException>(() => Checksum.CompareChecksum(file, checksum!, ChecksumAlgorithm.SHA512));
        }

    private void AssertFileChecksum(string filePath, string expected, ChecksumAlgorithm algorithm) =>
        Assert.True(Checksum.CompareChecksum(filePath, expected, algorithm), $"{algorithm} failed");
    }

}