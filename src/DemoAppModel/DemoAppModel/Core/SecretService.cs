using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DemoAppModel.Core
{
    public class SecretService : ISecretService
    {
        private readonly Secrets secrets;
        private readonly ILogger<SecretService> logger;

        public SecretService(Secrets secrets, ILogger<SecretService> logger)
        {
            this.secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            this.logger = logger;
        }

        public void ShowSecrets()
        {
            logger.LogDebug($"==========================");
            logger.LogDebug($"Secret WorkspaceId: {secrets.WorkspaceId}");
            logger.LogDebug($"Secret ApiKey     : {secrets.ApiKey}");
            logger.LogDebug($"Secret ApiSecret  : {secrets.ApiSecret}");
            logger.LogDebug($"==========================");
        }
    }
}
