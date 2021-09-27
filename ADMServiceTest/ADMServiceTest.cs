using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chetch.Arduino2;
using Chetch.Arduino2.Devices;

namespace ADMServiceTest
{
    public class ADMServiceTest : ADMService
    {
        public const String SERVICE_CMNAME = "ADMServiceTest";

        public class SwitchGroup : Chetch.Arduino2.ArduinoDeviceGroup
        {
            SwitchDevice sw0;
            SwitchDevice sw1;

            public SwitchGroup(String id = "swg", String name = "SWG") : base(id, name)
            {
                sw0 = new SwitchDevice("sw0", SwitchDevice.SwitchMode.PASSIVE, 6, SwitchDevice.SwitchPosition.OFF);
                sw1 = new SwitchDevice("sw1", SwitchDevice.SwitchMode.ACTIVE, 7, SwitchDevice.SwitchPosition.OFF);
                AddDevice(sw0);
                AddDevice(sw1);
            }

            protected override void HandleDevicePropertyChange(Chetch.Arduino2.ArduinoDevice device, System.Reflection.PropertyInfo property)
            {
                SwitchDevice sw = (SwitchDevice)device;
                if (property.Name == "Position" && sw.IsPassive)
                {
                    sw1.SetPosition(sw.Position);

                }
            }
        }
        public ADMServiceTest() : base(SERVICE_CMNAME, "ADMSTClient", "ADMServiceTest", "ADMServiceTestLog")
        {
            Chetch.Arduino2.ArduinoDeviceManager ADM;
            /*if (useSerial)
            {
                String portName = "CH340";
                int localUartSize = 64;
                int remoteUartSize = 64;
                ADM = Chetch.Arduino2.ArduinoDeviceManager.Create(portName, 115200, localUartSize, remoteUartSize);
            }
            else
            {*/
            //String serviceName = "kaki5";
            String serviceName = "oblong3";
            String networkServiceURL = "http://192.168.1.188:8001/api";
            int localUartSize = 64;
            int remoteUartSize = 64;
            ADM = Chetch.Arduino2.ArduinoDeviceManager.Create(serviceName, networkServiceURL, localUartSize, remoteUartSize);
            //}


            SwitchGroup swg = new SwitchGroup();
            ADM.AddDeviceGroup(swg);
            AddADM(ADM);

            Settings = Properties.Settings.Default;
            ServiceDB = ADMServiceDB.Create<ADMServiceDB>(Settings, "ADMServiceDBName");
        }

    }
}
