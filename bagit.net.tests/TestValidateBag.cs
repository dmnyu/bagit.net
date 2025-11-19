using Microsoft.VisualStudio.TestPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bagit.net.tests
{
    public class TestValidateBag : IDisposable
    {
        private readonly string _tmpDir;
        private readonly string _bagDir;
        private readonly Validator _validator;
        
        public TestValidateBag()
        {
            _tmpDir = TestHelpers.PrepareTempTestData();
            _bagDir = Path.Combine(_tmpDir, "bagged-dir");
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
            Assert.True(Directory.Exists(_bagDir));
        }

        [Fact]
        public void Test_Has_Valid_BagitTXT()
        {
            var ex = Record.Exception(() => _validator.Has_Valid_BagitTXT(_bagDir));
            Assert.Null(ex);
        }

        [Fact]
        public void Test_Has_Valid_BaginfoTXT()
        {
            var ex = Record.Exception(() => _validator.Has_Valid_BaginfoTXT(_bagDir));
            Assert.Null(ex);
        }


    }
}
