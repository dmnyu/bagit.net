using bagit.net.interfaces;
using bagit.net.domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.integration
{
    public class TestValidateBag : IDisposable
    {
        private readonly string _testDir;
        private readonly string _unsupportedBag;
        private readonly ServiceProvider _serviceProvider;
        private readonly IValidationService _validationService;
        private readonly ITestOutputHelper _output;
        private readonly IMessageService _messageService;

        public TestValidateBag(ITestOutputHelper output)
        {
            _testDir = TestHelpers.PrepareTempTestData();
            _unsupportedBag = Path.Combine(_testDir, "unsupported-algorithm");
            _serviceProvider = ValidationServiceConfigurator.BuildServiceProvider();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _messageService = _serviceProvider.GetRequiredService<IMessageService>();
            _messageService.Clear();
            _output = output;
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
            Assert.Empty(_messageService.GetAll());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Invalid_Bag_Fast()
        {
            var invalidBag = Path.Combine(_testDir, "bag-invalid-oxum");
            _validationService.ValidateBagFast(invalidBag);
            Assert.NotEmpty(_messageService.GetAll());
            foreach(var message in  _messageService.GetAll())
            {
                _output.WriteLine($"{message}");
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Validate_Bag()
        {
            var _validBag = Path.Combine(_testDir, "valid-bag");
            _validationService.ValidateBag(_validBag);
            Assert.False(MessageHelpers.HasError(_messageService.GetAll()));
            foreach(var message in _messageService.GetAll())
            {
                _output.WriteLine($"{message}");
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Validate_Bag_Completeness() 
        {
            var validBag = Path.Combine(_testDir, "valid-bag");
            _validationService.ValidateBagCompleteness(validBag);
            Assert.Empty(_messageService.GetAll());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Validate_Incomplete_Bag()
        {
            var validBag = Path.Combine(_testDir, "bag-incomplete");
            _validationService.ValidateBagCompleteness(validBag);
            var messages = _messageService.GetAll();
            Assert.NotEmpty(messages);
            foreach (var message in messages.ToList())
            {
                _output.WriteLine($"{message}");
            }
        }
    }
}
