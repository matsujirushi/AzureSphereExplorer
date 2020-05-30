using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSphereExplorer
{
    class DeploymentModel
    {
        public AzureSphereDeployment Context;

        public DateTime CurrentDeploymentDate { get; set; }
        public int NumberOfImages { get; set; }
    }
}
