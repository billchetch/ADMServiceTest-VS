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
            List<TestDevice01> tdevs = new List<TestDevice01>();
            Dictionary<String, int> missing = new Dictionary<String, int>();
            Dictionary<String, int> received = new Dictionary<String, int>();
            System.Timers.Timer _timer;

            public TestGroup(String id = "tg", String name = "TG") : base(id, name)
            {

                for(int i = 0; i < 6; i++)
                {
                    var td = new TestDevice01("t" + i);
                    td.ReportInterval = 200;
                    tdevs.Add(td);
                    AddDevice(td);
                }

                //Report via onsole
                _timer = new System.Timers.Timer();
                _timer.Elapsed += OnTimer;
                _timer.Interval = 10*1000;
                _timer.AutoReset = true;
                _timer.Start();
            }

            private void OnTimer(Object sender, EventArgs eargs)
            {
                //Console.WriteLine("{0} sent value {1}", td.ID, td.TestValue);
                if (!ADM.IsReady) return;

                if (received.Count == tdevs.Count  && missing.Count == tdevs.Count)
                {
                    String s = "";
                    foreach(TestDevice01 td in tdevs){
                        s += String.Format("{0} rec/mis = {1}/{2}, ", td.ID,  received[td.ID], missing[td.ID]);
                    }
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
                        if (Math.Abs(td.TestValue - td.PrevTestValue) != 1)
                        {
                            missing[device.ID]++;
                        }
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

        public class GensetGovernor : ArduinoDeviceGroup
        {
            Chetch.Arduino2.Devices.Electricity.ZMPT101B zmpt;
            Chetch.Arduino2.Devices.Motors.ServoController servo;

            public GensetGovernor(String id = "gov", String name = "GOV") : base(id, name)
            {
                zmpt = new Chetch.Arduino2.Devices.Electricity.ZMPT101B("z1", 14);
                zmpt.ReportInterval = 2000;
                zmpt.SetTargetParameterse(Chetch.Arduino2.Devices.Electricity.ZMPT101B.Target.VOLTAGE, 223, 2, 200, 250);
                AddDevice(zmpt);
                
                /*servo = new Chetch.Arduino2.Devices.Motors.ServoController("srv1", 7);
                servo.Position = 90; // Chetch.Arduino2.Devices.Motors.ServoController.SERVER_POSITION_NONE;
                AddDevice(servo);*/
            }


            protected override void OnDeviceReady(ArduinoDevice device)
            {
                base.OnDeviceReady(device);

                if(device == servo)
                {
                    Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(500);
                        servo.RotateBy(50);
                        System.Threading.Thread.Sleep(500);
                        servo.RotateBy(-100);
                        System.Threading.Thread.Sleep(500);
                        servo.RotateBy(50);
                        //servo.MoveTo(35);
                    });
                }
            }

            protected override void HandleDevicePropertyChange(ArduinoDevice device, PropertyInfo property)
            {
                if (property.Name == "Voltage")
                {
                    Console.WriteLine("{0} Voltage: {1}, Hz: {2}", zmpt.ID, zmpt.Voltage, zmpt.Hz);
                }
                if (property.Name == "Adjustment")
                {
                    Console.WriteLine("{0} Adjust {1} by: {2}", zmpt.ID, zmpt.Targeting, zmpt.Adjustment);
                }
            }
        }

        public ADMServiceTest() : base(SERVICE_CMNAME, "ADMSTClient", "ADMServiceTest", "ADMServiceTestLog")
        {
            Chetch.Arduino2.ArduinoDeviceManager ADM;
            int localUartSize = 256;
            int remoteUartSize = 256;
            String serviceName = "crayfish9";
            //String serviceName = "oblong3";
            //String networkServiceURL = "http://192.168.2.100:8001/api";
            //String networkServiceURL = "http://192.168.1.188:8001/api";
            String networkServiceURL = "http://192.168.2.180:8001/api";

            bool useSerial = false;
            /*if (useSerial)
            {
                ADM = ArduinoDeviceManager.Create(ArduinoSerialConnection.BOARD_CH340, 115200, localUartSize, remoteUartSize);
            }
            else
            {
                ADM = ArduinoDeviceManager.Create(serviceName, networkServiceURL, localUartSize, remoteUartSize);
            }

            ADM.AddDeviceGroup(new SwitchGroup());
            ADM.AddDeviceGroup(new TestGroup());
            AddADM(ADM);*/

            if (useSerial)
            {
                ADM = ArduinoDeviceManager.Create(ArduinoSerialConnection.BOARD_UNO, 115200, localUartSize, remoteUartSize);
                //ADM = ArduinoDeviceManager.Create(ArduinoSerialConnection.BOARD_CH340, 115200, localUartSize, remoteUartSize);
            }
            else
            {
                //serviceName = "oblong3";
                ADM = ArduinoDeviceManager.Create(serviceName, networkServiceURL, localUartSize, remoteUartSize);
            }
            
            //ADM.AddDeviceGroup(new SwitchGroup());
            ADM.AddDeviceGroup(new TestGroup());

            //ADM.AddDeviceGroup(new GensetGovernor());


            AddADM(ADM);

            Settings = Properties.Settings.Default;
            ServiceDB = ADMServiceDB.Create<ADMServiceDB>(Settings, "ADMServiceDBName");
        }

    }
}
