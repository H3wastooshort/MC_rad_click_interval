using System;
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
                Console.WriteLine("Plug in Mobile-CASSY 2 now.");
                while (!cassy.Open()) ; ;
                
                if (cassy.CASSYType == CASSYTypes.Mobile2) Console.WriteLine("Mobile-CASSY 2 found.");
                else { Console.WriteLine("Mobile-CASSY 2 not found."); return -1; }

                Box524440 gmBox = new Box524440(cassy, 0);
                cassyios.Add(gmBox);
                cassyios.Scan(ScanMode.ScanOnly);
                if (!gmBox.SensorBoxValid) { Console.WriteLine("Missing GM Box on input A"); return -2; }
                CASSYQuantity clicksQ = gmBox.QuantityN;
                clicksQ.Selected = true;
                double lastN = 0;
                double lastT = 0;
                using (StreamWriter logfile = new StreamWriter("rad_intervals.csv")) while (true)
                    {
                        cassyios.DoSingleMeasurement();
                        if (clicksQ.Value > lastN)
                        {
                            double timeNow = cassyios.Times[0]; //hacky and bad
                            double deltaT = timeNow - lastT;
                            lastT = timeNow;
                            lastN = clicksQ.Value;
                            Console.WriteLine(deltaT);
                            logfile.WriteLine(deltaT);
                        }
                    }
            }
        }
    }
}
