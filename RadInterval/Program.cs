using System;
using System.Diagnostics;
using System.IO;
using LD.Api;
using LD.Api.IOs;

namespace RadInterval
{
    internal class Program
    {
        static int Main(string[] args)
        {
            //all this assumes one MC2 connected via USB, nothing else.
            using (CASSY cassy = new MobileCASSY2())
            using (CASSYIOs cassyios = new CASSYIOs())
            {
                cassy.Connection = Connection.Usb;
                if (!cassy.Open()) { Console.WriteLine("Could not open CASSY. Is it plugged in and not used by another program?"); return -1; }
                
                if (cassy.CASSYType == CASSYTypes.Mobile2) Console.WriteLine("Mobile-CASSY 2 found.");
                else { Console.WriteLine("Mobile-CASSY 2 not found."); return -2; }

                Box524440 gmBox = new Box524440(cassy, 0);
                cassyios.Add(gmBox);
                cassyios.Scan(ScanMode.ScanOnly);
                if (!gmBox.SensorBoxValid) { Console.WriteLine("Missing GM Box on input A."); return -3; }
                CASSYQuantity clicksQ = gmBox.QuantityN;
                clicksQ.Selected = true;

                double lastN = 0;
                Stopwatch clock = new Stopwatch(); //bad idea, bad and hacky, dont do this
                clock.Start();
                using (StreamWriter logfile = new StreamWriter("rad_intervals.csv")) while (true)
                    {
                        cassyios.DoSingleMeasurement();
                        if (clicksQ.Value > lastN)
                        {
                            clock.Stop();

                            lastN = clicksQ.Value;

                            double deltaT = clock.Elapsed.TotalMilliseconds;
                            Console.WriteLine(deltaT);
                            logfile.WriteLine(deltaT);
                            logfile.Flush();

                            clock.Restart();
                        }
                    }
            }
        }
    }
}
