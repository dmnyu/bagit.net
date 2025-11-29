using bagit.net.interfaces;
using Microsoft.Extensions.Logging;

namespace bagit.net.cli
{
    public class Validator
    {
        private readonly ILogger _logger;
        private readonly IValidationService _validationService;
        public Validator(ILogger<Validator> logger, IValidationService validationService)
        {
            _logger = logger;
            _validationService = validationService;
        }
        public void ValidateBag(string bagPath, bool fast)
        {
            _logger.LogInformation($"Using bagit.net v{Bagit.VERSION}");
            try
            {   
                if(fast)
                {  
                    _validationService.ValidateBag(bagPath);
                    _logger.LogInformation($"{bagPath} is valid according to Payload-Oxum");
                    return;
                }

                _validationService.ValidateBag(bagPath);
                _logger.LogInformation($"{bagPath} is valid");
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Failed to validate bag at {Path}", bagPath);
                throw;
            }
        }

    }
}
