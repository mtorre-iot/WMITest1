#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using System.Management;
using LibreHardwareMonitor.Hardware;

#endregion

public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

public class WMIScanLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void Windows_General_Init()
    {
        //  ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2");
        // scope.Connect();

        // // Create a new query to get information about the operating system
        // ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");

        // // Create a new ManagementObjectSearcher to execute the query
        // ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

        // // Call the Get() method to retrieve the collection of WMI objects that match the query
        // ManagementObjectCollection results = searcher.Get();

        // // Loop through each object in the collection and print out its properties
        // foreach (ManagementObject obj in results)
        // {
        //     Log.Info("Operating System: {0}", obj["Caption"].ToString());
        //     Log.Info("Version: {0}", obj["Version"].ToString());
        //     Log.Info("Architecture: {0}", obj["OSArchitecture"].ToString());
        // }

        // Console.ReadLine();
    }

    [ExportMethod]
    public void Temperature_Init()
    {
        Monitor();
    }

    public void Monitor()
    {
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };

        computer.Open();
        computer.Accept(new UpdateVisitor());

        foreach (IHardware hardware in computer.Hardware)
        {
            Log.Info(string.Format("Hardware: {0}", hardware.Name));
            
            foreach (IHardware subhardware in hardware.SubHardware)
            {
                Log.Info(string.Format("\tSubhardware: {0}", subhardware.Name));
                
                foreach (ISensor sensor in subhardware.Sensors)
                {
                    Log.Info(string.Format("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value));
                }
            }

            foreach (ISensor sensor in hardware.Sensors)
            {
                Log.Info(string.Format("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value));
            }
        }
        computer.Close();
    }

    [ExportMethod]
    public void Alternative()
    {
        // Connect to the WMI provider
        ManagementScope scope = new ManagementScope("\\\\.\\root\\LibreHardwareMonitor");
        scope.Connect();

        // //ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT value FROM Sensor WHERE Name LIKE '%CPU Core%' AND SensorType = 'Temperature'"));
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM Sensor WHERE SensorType='Temperature' AND Name LIKE '%CPU%'"));
        
        
        ManagementObjectCollection sensors = searcher.Get();
        double[] temp = new double[sensors.Count];
        int i = 0;
        foreach (ManagementObject sensor in sensors)
        {
            // Read the CPU load
            temp[i] = Convert.ToDouble(sensor["Value"]);
            Log.Info(string.Format("CPU {0} - Temp: {1}", (i+1).ToString(), temp[i]));
            i++;
        }
    }
}
