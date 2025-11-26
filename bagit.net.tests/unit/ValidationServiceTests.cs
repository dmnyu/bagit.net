using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace bagit.net.tests.unit
{
    public class ValidationServiceTests : IDisposable
    {
        readonly String _testData; 
        readonly ServiceProvider _serviceProvider;
        readonly IValidationService _validationService;


        public ValidationServiceTests()
        {
            _testData = TestHelpers.PrepareTempTestData();
            _serviceProvider = ValidationServiceConfigurator.BuildServiceProvider();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
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
            var ex = Record.Exception(() => _validationService.HasRequiredFiles(_validBag));
            Assert.Null(ex);
        }

    }
}
