using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structurizr;

namespace DemoAppModel.Core
{
    public interface IStructurizrService
    {
        void PublishModel(Workspace workspace);
        Workspace GetWorkspace();
    }
}
