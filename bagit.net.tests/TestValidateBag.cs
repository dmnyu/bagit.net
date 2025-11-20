namespace bagit.net.tests
{
    public class TestValidateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _validBag;
        private readonly string _unsupportedBag;
        private readonly Validator _validator;
        
        public TestValidateBag()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _validBag = Path.Combine(_tmpDir, "bagged-dir");
            _unsupportedBag = Path.Combine(_tmpDir, "unsupported-algorithm");
            _validator = new Validator();
            Bagit.InitLogger();
        }

        public void Dispose()
        {
            if(Directory.Exists(_tmpDir))
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
            var ex = Record.Exception(() => _validator.Has_Valid_BaginfoTXT(_validBag));
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
            string unsupportedAlgorithmManifest =  Path.Combine(_unsupportedBag, "manifest-blake2s.txt");
            Assert.Throws<InvalidDataException>(() => Manifest.ValidateManifestFile(unsupportedAlgorithmManifest));
        }

        [Fact]
        public void Test_Unsupported_Algorithm_Bag()
        {
            Assert.Throws<InvalidDataException>(() => _validator.ValidateManifests(_unsupportedBag));
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
