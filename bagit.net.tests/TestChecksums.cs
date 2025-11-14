using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.tests
{
    public class TestChecksums
    {
        //Test that all checksum algorithm's calculate correctly 
        [Fact]
        public void Test_Compare_Checksums_For_Empty_File()
        {
            var tmpDir = TestHelpers.PrepareTempTestData();
            var expectedChecksums = new Dictionary<ChecksumAlgorithm, string> {
                { ChecksumAlgorithm.MD5,  "d41d8cd98f00b204e9800998ecf8427e" },
                { ChecksumAlgorithm.SHA1,  "da39a3ee5e6b4b0d3255bfef95601890afd80709" },
                { ChecksumAlgorithm.SHA256,"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" },
                { ChecksumAlgorithm.SHA384,"38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b" },
                { ChecksumAlgorithm.SHA512,"cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e" }
            };
            try
            {
                var emptyFile = Path.Combine(tmpDir, "empty.txt");
                File.WriteAllBytes(emptyFile, Array.Empty<byte>());
                foreach (var kv in expectedChecksums) {
                    Assert.True(Checksum.CompareChecksum(emptyFile, kv.Value, kv.Key));
                }
            } finally
            {
                Directory.Delete(tmpDir, true);
            }
        }
    }
}
