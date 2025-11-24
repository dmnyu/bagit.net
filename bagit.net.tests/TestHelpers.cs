namespace bagit.net.tests
{
    internal static class TestHelpers
    {
        public static string PrepareTempTestData()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"BagitTest_{Guid.NewGuid()}");
            var originalDir = Path.Combine(AppContext.BaseDirectory, "TestData");

            CopyDirectory(originalDir, tempDir);
            return tempDir;
        }


        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.EnumerateFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);

            foreach (var dir in Directory.EnumerateDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }

    }
}
