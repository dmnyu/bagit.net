using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.unit
{
    public class TagFileServiceTests: IDisposable
    {
        readonly ServiceProvider _serviceProvider;
        readonly ITagFileService _tagFileService;
        readonly string _tmpDir;
        public TagFileServiceTests()
        {
            _serviceProvider = TagFileServiceConfigurator.BuildServiceProvider();
            _tagFileService = _serviceProvider.GetRequiredService<ITagFileService>();
            _tmpDir = TestHelpers.PrepareTempTestData();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            if(Path.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Tag_File_Dictionary()
        {
            var bagInfo = Path.Combine(_tmpDir, "valid-bag", "bag-info.txt");
            var tagDict = _tagFileService.GetTagFileAsDict(bagInfo);
            Assert.True(tagDict.Count > 1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Tag_File_List()
        {
            var bagInfo = Path.Combine(_tmpDir, "valid-bag", "bag-info.txt");
            var tagDict = _tagFileService.GetTagFileAsList(bagInfo);
            Assert.True(tagDict.Count > 1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Create_Bag_Info()
        {
            var dataOnly = Path.Combine(_tmpDir, "data-only");
            Exception ex = Record.Exception(() => _tagFileService.CreateBagInfo(dataOnly));
            Assert.True(ex == null);
            var bagInfo = Path.Combine(dataOnly, "bag-info.txt");
            Assert.True(File.Exists(bagInfo));
            var tagDict = _tagFileService.GetTagFileAsDict(bagInfo);
            Assert.NotNull(tagDict["Payload-Oxum"]);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Oxum()
        {
            var validBag = Path.Combine(_tmpDir, "valid-bag");
            var calculatedOxum = _tagFileService.GetOxum(validBag);
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tagDict = _tagFileService.GetTagFileAsDict(bagInfo);
            Assert.Equal(tagDict["Payload-Oxum"], calculatedOxum);
        }
    }
}
