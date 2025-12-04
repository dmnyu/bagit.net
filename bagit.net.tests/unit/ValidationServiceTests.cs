using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace bagit.net.tests.unit
{
    public class ValidationServiceTests : IDisposable
    {
        readonly String _testData; 
        readonly ServiceProvider _serviceProvider;
        readonly IValidationService _validationService;
        readonly IMessageService _messageService;


        public ValidationServiceTests()
        {
            _testData = TestHelpers.PrepareTempTestData();
            _serviceProvider = ValidationServiceConfigurator.BuildServiceProvider();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            if (Path.Exists(_testData))
                Directory.Delete(_testData, true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Test_Bag_Has_Required_Files()
        {
            var _validBag = Path.Join(_testData, "valid-bag");
            _validationService.HasRequiredFiles(_validBag);
            Assert.Empty(_messageService.GetAll());
        }


    }
}
