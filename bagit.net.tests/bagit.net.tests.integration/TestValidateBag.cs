using bagit.net.interfaces;
using bagit.net.domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace bagit.net.tests.integration
{
    public class TestValidateBag : IDisposable
    {
        private string _testDir = string.Empty;
        private readonly ServiceProvider _serviceProvider;
        private readonly IValidationService _validationService;
        private readonly ITestOutputHelper _output;
        private readonly IMessageService _messageService;

        public TestValidateBag(ITestOutputHelper output)
        {
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
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-valid");
            _validationService.ValidateBagFast(_testDir);
            var messages = _messageService.GetAll();
            Assert.False(MessageHelpers.HasError(messages));
            if (MessageHelpers.HasError(messages))
                foreach(var message in messages)
                    _output.WriteLine($"{message}");

        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Invalid_Bag_Fast()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-invalid-oxum");
            _validationService.ValidateBagFast(_testDir);
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
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-valid");
            _validationService.ValidateBag(_testDir);
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
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-valid");
            _validationService.ValidateBagCompleteness(_testDir);
            Assert.Empty(_messageService.GetAll());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Validate_Incomplete_Bag()
        {
            _testDir = TestHelpers.PrepareTempTestDataDir("bag-incomplete");
            _validationService.ValidateBagCompleteness(_testDir);
            var messages = _messageService.GetAll();
            Assert.NotEmpty(messages);
            foreach (var message in messages.ToList())
            {
                _output.WriteLine($"{message}");
            }
        }
    }
}
