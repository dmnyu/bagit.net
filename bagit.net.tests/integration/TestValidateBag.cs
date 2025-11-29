using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests.integration
{
    public class TestValidateBag : IDisposable
    {
        private readonly string _testDir;
        private readonly string _unsupportedBag;
        private readonly ServiceProvider _serviceProvider;
        private readonly IValidationService _validationService;
        
        public TestValidateBag()
        {
            _testDir = TestHelpers.PrepareTempTestData();
            _unsupportedBag = Path.Combine(_testDir, "unsupported-algorithm");
            _serviceProvider = ValidationServiceConfigurator.BuildServiceProvider();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Validate_Bag_Fast()
        {
            var _validBag = Path.Combine(_testDir, "valid-bag");
            var ex = Record.Exception(() =>  _validationService.ValidateBagFast(_validBag));
            Assert.Null(ex);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Validate_Bag()
        {
            var _validBag = Path.Combine(_testDir, "valid-bag");
            var ex = Record.Exception(() => _validationService.ValidateBag(_validBag));
            Assert.Null(ex);
        }








        /*




                [Fact]
                [Trait("Category", "Integration")]
                public void Test_Validate_Bag()
                {
                    var _validBag = Path.Combine(_tmpDir, "valid-bag");
                    var ex = Record.Exception(() => _validator.ValidateBag(_validBag, false));
                    Assert.Null(ex);
                }

                */

    }
}
