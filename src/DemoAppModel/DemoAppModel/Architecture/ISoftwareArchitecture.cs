using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppModel.Architecture
{
    public interface ISoftwareArchitecture
    {
        void SetWorkspace(string workspaceName, string workspaceDescription);
        void PublishC4Model();
    }
}
