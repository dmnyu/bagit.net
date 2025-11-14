using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.tests
{
    internal static class TestHelpers
    {
        public static string PrepareTempTestData()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "BagitTest_" + Guid.NewGuid());
            var originalDir = Path.Combine(AppContext.BaseDirectory, "TestData");

            CopyDirectory(originalDir, tempDir);
            return tempDir;
        }


        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)));

            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }
}
