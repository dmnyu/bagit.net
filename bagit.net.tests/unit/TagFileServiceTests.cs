using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.unit
{
    public class TagFileServiceTests : IDisposable
    {
        readonly ServiceProvider _serviceProvider;
        readonly ITagFileService _tagFileService;
        readonly IMessageService _messageService;
        private readonly ITestOutputHelper _output;
        readonly string _testDir;
        public TagFileServiceTests(ITestOutputHelper output)
        {
            _serviceProvider = TagFileServiceConfigurator.BuildServiceProvider();
            _tagFileService = _serviceProvider.GetRequiredService<ITagFileService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
            _output = output;
            _testDir = TestHelpers.PrepareTempTestData();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Path.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Tag_File_Dictionary()
        {
            var bagInfo = Path.Combine(_testDir, "bag-valid", "bag-info.txt");
            var tagDict = _tagFileService.GetTagFileAsDict(bagInfo);
            Assert.True(tagDict.Count > 1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Tag_File_List()
        {
            var bagInfo = Path.Combine(_testDir, "bag-valid", "bag-info.txt");
            var tagDict = _tagFileService.GetTagFileAsList(bagInfo);
            Assert.True(tagDict.Count > 1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Create_Bag_Info()
        {
            var dataOnly = Path.Combine(_testDir, "data-only");
            Exception ex = Record.Exception(() => _tagFileService.CreateBagInfo(dataOnly, null));
            Assert.True(ex == null);
            var bagInfo = Path.Combine(dataOnly, "bag-info.txt");
            Assert.True(File.Exists(bagInfo));
            var tagDict = _tagFileService.GetTags(bagInfo);
            Assert.NotNull(tagDict["Payload-Oxum"]);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Create_Bag_Info_With_Metadata()
        {
            var dataOnly = Path.Combine(_testDir, "data-only");
            var metadata = Path.Combine(_testDir, "metadata.txt");
            Exception ex = Record.Exception(() => _tagFileService.CreateBagInfo(dataOnly, metadata));
            Assert.True(ex == null);
            var bagInfo = Path.Combine(dataOnly, "bag-info.txt");
            Assert.True(File.Exists(bagInfo));
            var tags = _tagFileService.GetTags(bagInfo);
            Assert.NotNull(tags["Payload-Oxum"]);
            foreach (var tag in tags)
            {
                foreach (var val in tag.Value)
                {
                    _output.WriteLine($"{tag.Key}: {val}");
                }
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Oxum()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            var calculatedOxum = _tagFileService.CalculateOxum(validBag);
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tagDict = _tagFileService.GetTags(bagInfo);
            Assert.Equal(tagDict["Payload-Oxum"][0], calculatedOxum);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Validate_BagitTXT()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.ValidateBagitTXT(validBag);
            Assert.Empty(_messageService.GetAll());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Has_BagInfo()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            Assert.True(_tagFileService.HasBagInfo(validBag));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Validate_BagInfo()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.ValidateBagInfo(Path.Combine(validBag, "bag-info.txt"));
            Assert.Empty(_messageService.GetAll());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Get_Tags()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tags = _tagFileService.GetTags(bagInfo);
            Assert.True(tags.Count > 0);
        }

        [Theory]
        [InlineData("Source-Organization", "Example Digital Archives Division")]
        [InlineData("Organization-Address", "123 Preservation Way, Archive City, NY 10001")]
        [InlineData("Contact-Name", "Jane Archivist")]
        [InlineData("Contact-Phone", "+1-212-555-0123")]
        [InlineData("Contact-Email", "jane.archivist@example.org")]
        [InlineData("External-Description", "Digitized photographs from the 1940s, scanned at 600dpi.")]
        [InlineData("External-Identifier", "ARC-2025-000123")]
        [InlineData("Bag-Group-Identifier", "WWII-Photo-Collection")]
        [InlineData("Bag-Count", "1 of 3")]
        [InlineData("Internal-Sender-Identifier", "DIGI-SERVER-004")]
        [InlineData("Internal-Sender-Description", "Batch export from the Digitization Workflow System")]
        [Trait("Category", "Unit")]
        public void Test_Add_Tag(string key, string value)
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.AddTag(key, value, validBag);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tags = _tagFileService.GetTags(bagInfo);
            Assert.True(tags.ContainsKey(key));
            Assert.Equal(tags[key][0], value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Add_Multiple_Tags()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.AddTag("External-Identifier", "ARC-2025-000123", validBag);
            _tagFileService.AddTag("External-Identifier", "ARC-2025-000124", validBag);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tags = _tagFileService.GetTags(bagInfo);
            Assert.Equal("ARC-2025-000123", tags["External-Identifier"][0]);
            Assert.Equal("ARC-2025-000124", tags["External-Identifier"][1]);
        }

        [Theory]
        [InlineData("Bagging-Date", "2025-09-01")]
        [InlineData("Payload-Oxum", "10.2")] 
        [InlineData("Bag-Software-Agent", "fake agent")]
        [InlineData("BagIt-Version", "0.1")]
        [Trait("Category", "Unit")]
        public void Test_Add_Non_Repeatable_Tag(string key, string value)
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.AddTag(key, value, validBag);
            var messages = _messageService.GetAll();
            Assert.True(MessageHelpers.HasError(messages));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Set_Tag()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.AddTag("External-Identifier", "ARC-2025-000123", validBag);
            _tagFileService.SetTag("External-Identifier", "ARC-2025-000124", validBag);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tags = _tagFileService.GetTags(bagInfo);
            Assert.Equal("ARC-2025-000124", tags["External-Identifier"][0]);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Delete_Tag()
        {
            var validBag = Path.Combine(_testDir, "bag-valid");
            _tagFileService.DeleteTag("Payload-Oxum", validBag);
            Assert.False(MessageHelpers.HasError(_messageService.GetAll()));
            var bagInfo = Path.Combine(validBag, "bag-info.txt");
            var tags = _tagFileService.GetTags(bagInfo);
            Assert.False(tags.ContainsKey("Payload-Oxum"));
        }

    }
}