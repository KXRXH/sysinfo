using System;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace HardwareMonitor {
  internal class Program {
    static void Main(string[] args) {
      Computer pc = new Computer() { CPUEnabled = true, RAMEnabled = true, GPUEnabled = true,
                                     MainboardEnabled = true };
      pc.Open();
      foreach (var hwElement in pc.Hardware) {
        switch (hwElement.HardwareType) {
          case HardwareType.CPU:
            UpdateSensors(hwElement);
            ushort coresNumber = 0;
            Dictionary<String, ushort> values = GetCpuState(hwElement.Sensors, ref coresNumber);
            // values.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
            Console.WriteLine("=================================");
            Console.WriteLine("              CPU");
            Console.WriteLine("=================================");
            Console.WriteLine("CPU: " + hwElement.Name);
            Console.WriteLine("> CPU Physical cores: " + coresNumber);
            Console.WriteLine("> CPU Temperature: " + values["CPU Package_t"] + " °C");
            Console.WriteLine("> CPU Clock: " + values["CPU Core #1"] + " MHz");
            Console.WriteLine("> CPU Power: " + values["CPU Package"] + " W");

            Console.WriteLine("=================================");
            Console.WriteLine("            CPU LOAD");
            Console.WriteLine("=================================");
            Console.WriteLine("CPU Load: " + values["CPU Total_L"] + " %");

            for (int i = 1; i <= coresNumber; i++) {
              Console.WriteLine($"> Core #{i}: " + values[$"CPU Core #{i}_L"] + " %");
            }
            break;
          case HardwareType.RAM:
            UpdateSensors(hwElement);
            double usedMem, freeMem;
            (usedMem, freeMem) = GetRamState(hwElement.Sensors);
            Console.WriteLine("=================================");
            Console.WriteLine("              RAM");
            Console.WriteLine("=================================");
            Console.WriteLine("Total RAM " + Math.Round(freeMem + usedMem) + " GB");
            Console.WriteLine("> Used RAM: " + Math.Round(usedMem, 1) + " GB");
            Console.WriteLine("> Free RAM: " + Math.Round(freeMem, 1) + " GB");
            break;
        }
      }
    }
    static Dictionary<String, ushort> GetCpuState(ISensor[] elSensors, ref ushort coreCounter) {
      Dictionary<String, ushort> dict = new Dictionary<string, ushort>();
      foreach (ISensor sensor in elSensors) {
        switch (sensor.SensorType) {
          case SensorType.Temperature:
            dict[sensor.Name + "_t"] = (ushort)sensor.Value;
            break;
          case SensorType.Clock:
            dict[sensor.Name] = (ushort)sensor.Value;
            coreCounter++;
            break;
          case SensorType.Power:
            dict[sensor.Name] = (ushort)sensor.Value;
            break;
          case SensorType.Load:
            dict[sensor.Name + "_L"] = (ushort)sensor.Value;
            break;
        }
      }
      coreCounter--;
      return dict;
    }
    static (double, double) GetRamState(ISensor[] elSensors) {
      double usedMem, freeMem;
      usedMem = 0;
      freeMem = 0;
      foreach (ISensor sensor in elSensors) {
        if (SensorType.Data == sensor.SensorType) {
          if (sensor.Name == "Used Memory") {
            usedMem = (double)sensor.Value;
          } else {
            freeMem = (double)sensor.Value;
          }
        }
      }
      return (usedMem, freeMem);
    }
    static void UpdateSensors(IHardware hardwareElement) {
      hardwareElement.Update();
      foreach (IHardware subHardware in hardwareElement.SubHardware) {
        subHardware.Update();
      }
    }
  }
}
