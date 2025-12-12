using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace bagit.net.tests.unit
{
    public class ValidationServiceTests : IDisposable
    {
        private string _testDir = string.Empty; 
        readonly ServiceProvider _serviceProvider;
        readonly IValidationService _validationService;
        readonly IMessageService _messageService;


        public ValidationServiceTests()
        {
            _serviceProvider = ValidationServiceConfigurator.BuildServiceProvider();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Path.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Bag_Has_Required_Files()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-valid");
            _validationService.HasRequiredFiles(_testDir);
            Assert.Empty(_messageService.GetAll());
        }


    }
}
