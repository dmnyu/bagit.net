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

/*
 [Fact]
        [Trait("Category", "Integration")]
        public void Test_Has_Valid_BaginfoTXT()
        {
            var _validBag = Path.Combine(_tmpDir, "valid-bag");
            var ex = Record.Exception(() => _validator.Has_Valid_BaginfoTXT(_validBag, false));
            Assert.Null(ex);
        }
[Fact]
[Trait("Category", "Integration")]
public void Test_Invalid_Oxum()
{
    var invalidOxum = Path.Combine(_tmpDir, "bag-invalid-oxum");
    Assert.Throws<InvalidDataException>(() => _validator.ValidateBag(invalidOxum, false));
}

        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Unsupported_Algorithm()
        {
            var manifestService = _serviceProvider.GetRequiredService<IManifestService>();
            string unsupportedAlgorithmManifest =  Path.Combine(_unsupportedBag, "manifest-blake2s.txt");
            Assert.Throws<InvalidDataException>(() => manifestService.ValidateManifestFile(unsupportedAlgorithmManifest));
        }

        /*
        [Fact]
        [Trait("Category", "Integration")]
        public void Test_Bag_Exists() 
        {
            var _validBag = Path.Combine(_tmpDir, "valid-bag");
            Assert.True(Directory.Exists(_validBag));
        }


                [Fact]
                [Trait("Category", "Integration")]
                public void Test_Unsupported_Algorithm_Bag()
                {
                    Assert.Throws<InvalidDataException>(() => _validator.ValidateManifests(_unsupportedBag));
                }
*/
