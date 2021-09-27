using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chetch.Services;

namespace ADMServiceTest
{
    [RunInstaller(true)]
    public class ADMServiceTestInstaller : ServiceInstaller
    {
        public ADMServiceTestInstaller() : base("ADMServiceTest",
                                    "ADM Test Service",
                                    "Runs an ADM service that can be used for testing")
        {
            //empty
        }
    }
}
