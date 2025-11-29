using bagit.net.domain;
using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.cli
{
    public class Bagger
    {
        readonly ICreationService _creationService;
        readonly ILogger _logger;
            
        public Bagger(ILogger<Validator> logger, ICreationService creationService)
        {
            _logger = logger;
            _creationService = creationService;
        }

        public void CreateBag(string dirLocation, ChecksumAlgorithm algorithm)
        {
            _creationService.CreateBag(dirLocation, algorithm);
        }
    }
}
