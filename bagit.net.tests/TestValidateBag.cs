using bagit.net.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace bagit.net.tests
{
    public class TestValidateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _validBag;
        private readonly string _unsupportedBag;
        private readonly ServiceProvider _serviceProvider;
        private readonly Validator _validator;
        
        public TestValidateBag()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _validBag = Path.Combine(_tmpDir, "bagged-dir");
            _unsupportedBag = Path.Combine(_tmpDir, "unsupported-algorithm");
            _serviceProvider = ServiceConfigurator.BuildServiceProvider<Validator>();
            _validator = _serviceProvider.GetRequiredService<Validator>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            if (Directory.Exists(_tmpDir))
                Directory.Delete(_tmpDir, true);
        }

        [Fact]
        public void Test_Bag_Exists() 
        {
            Assert.True(Directory.Exists(_validBag));
        }

        [Fact]
        public void Test_Has_Valid_BagitTXT()
        {
            var ex = Record.Exception(() => _validator.Has_Valid_BagitTXT(_validBag));
            Assert.Null(ex);
        }

        [Fact]
        public void Test_Has_Valid_BaginfoTXT()
        {
            var ex = Record.Exception(() => _validator.Has_Valid_BaginfoTXT(_validBag, false));
            Assert.Null(ex);
        }

        [Fact]
        public void Test_Validate_Manifest_Files()
        {
            var ex = Record.Exception(() => _validator.ValidateManifests(_validBag));
            Assert.Null(ex);
        }

        [Fact]
        public void Test_Unsupported_Algorithm()
        {
            var manifestService = _serviceProvider.GetRequiredService<IManifestService>();
            string unsupportedAlgorithmManifest =  Path.Combine(_unsupportedBag, "manifest-blake2s.txt");
            Assert.Throws<InvalidDataException>(() => manifestService.ValidateManifestFile(unsupportedAlgorithmManifest));
        }

        [Fact]
        public void Test_Unsupported_Algorithm_Bag()
        {
            Assert.Throws<InvalidDataException>(() => _validator.ValidateManifests(_unsupportedBag));
        }

        [Fact]
        public void Test_Invalid_Oxum()
        {
            var invalidOxum = Path.Combine(_tmpDir, "bag-invalid-oxum");
            Assert.Throws<InvalidDataException>(() => _validator.ValidateBag(invalidOxum, false));
        }

        [Fact]
        public void Test_Validate_Bag()
        {
            var ex = Record.Exception(() => _validator.ValidateBag(_validBag, false));
            Assert.Null(ex);
        }

        [Fact]
        public void Test_Validate_Bag_Fast()
        {
            var ex = Record.Exception(() => _validator.ValidateBag(_validBag, true));
            Assert.Null(ex);
        }

    }
}
