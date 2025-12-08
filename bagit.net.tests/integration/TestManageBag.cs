using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.integration
{
    public class TestManageBag : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _testDir;
        private readonly ServiceProvider _serviceProvider;
        private readonly ITagFileService _tagFileService;
        private readonly IValidationService _validationService;
        private readonly IMessageService _messageService;
        public TestManageBag(ITestOutputHelper output) { 
            _output = output;
            _testDir = TestHelpers.PrepareTempTestData();
            _serviceProvider = DefaultServiceConfigurator.BuildServiceProvider();
            _tagFileService = _serviceProvider.GetRequiredService<ITagFileService>();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
        }

        public void Dispose() { 
            _serviceProvider.Dispose();
            if(Directory.Exists(_testDir)) 
                Directory.Delete(_testDir, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Add_Tag_To_Bag()
        {
            var validBag = Path.Combine(_testDir, "valid-bag");
            _tagFileService.AddTag("test-tag", "test-value", validBag);
            _validationService.ValidateBag(validBag);
            Assert.False(MessageHelpers.HasError(_messageService.GetAll()));
        }
    }
}
