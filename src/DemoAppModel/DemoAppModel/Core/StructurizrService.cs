using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Structurizr;
using Structurizr.Api;

namespace DemoAppModel.Core
{
    public class StructurizrService : IStructurizrService
    {
        private readonly ILogger<StructurizrService> logger;
        private readonly Secrets secrets;
        private readonly StructurizrClient client;

        public StructurizrService(ILogger<StructurizrService> logger, Secrets secrets)
        {
            this.logger = logger;
            this.secrets = secrets;
            client = new StructurizrClient(secrets.ApiKey, secrets.ApiSecret);

        }

        public void PublishModel(Workspace workspace)
        {
            client.PutWorkspace(secrets.WorkspaceId, workspace);
        }

        public Workspace GetWorkspace()
        {
            return client.GetWorkspace(secrets.WorkspaceId);
        }

    }
}
