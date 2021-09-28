using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                sw0 = new SwitchDevice("sw0", SwitchDevice.SwitchMode.PASSIVE, 6, SwitchDevice.SwitchPosition.OFF, 20);
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

        public class TestGroup : ArduinoDeviceGroup
        {
            TestDevice01 t0;
            TestDevice01 t1;
            Dictionary<String, int> missing = new Dictionary<String, int>();
            Dictionary<String, int> received = new Dictionary<String, int>();
            System.Timers.Timer _timer;

            public TestGroup(String id = "tg", String name = "TG") : base(id, name)
            {
                t0 = new TestDevice01("t0");
                t0.ReportInterval = 1;
                t1 = new TestDevice01("t1");
                t1.ReportInterval = 1;
                AddDevice(t0);
                AddDevice(t1);
                _timer = new System.Timers.Timer();
                _timer.Elapsed += OnTimer;
                _timer.Interval = 10000;
                _timer.AutoReset = true;
                _timer.Start();
            }

            private void OnTimer(Object sender, EventArgs eargs)
            {
                //Console.WriteLine("{0} sent value {1}", td.ID, td.TestValue);
                if (received.Count == 2 && missing.Count == 2)
                {
                    String s = String.Format("t0 recieved/missing = {0}/{1}, t1 received/missing = {2}/{3}", received["t0"], missing["t0"], received["t1"], missing["t1"]);
                    ADM.Tracing?.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, s);
                }
            }

            protected override void HandleDevicePropertyChange(ArduinoDevice device, PropertyInfo property)
            {
                //throw new NotImplementedException();
                TestDevice01 td = (TestDevice01)device;
                if(property.Name == "TestValue")
                {
                    if (!missing.ContainsKey(device.ID))
                    {
                        missing[device.ID] = 0;
                    } else
                    {
                        if (Math.Abs(td.TestValue - td.PrevTestValue) != 1) missing[device.ID]++;
                    }

                    if(!received.ContainsKey(device.ID))
                    {
                        received[device.ID] = 1;
                    } else
                    {
                        received[device.ID]++;
                    }
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
            String networkServiceURL = "http://192.168.2.100:8001/api";
            int localUartSize = 64;
            int remoteUartSize = 64;
            ADM = Chetch.Arduino2.ArduinoDeviceManager.Create(serviceName, networkServiceURL, localUartSize, remoteUartSize);
            //}


            SwitchGroup swg = new SwitchGroup();
            ADM.AddDeviceGroup(swg);
            
            TestGroup tg = new TestGroup();
            ADM.AddDeviceGroup(tg);

            AddADM(ADM);

            Settings = Properties.Settings.Default;
            ServiceDB = ADMServiceDB.Create<ADMServiceDB>(Settings, "ADMServiceDBName");
        }

    }
}
