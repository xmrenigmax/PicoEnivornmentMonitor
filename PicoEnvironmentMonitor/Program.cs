using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HoneywellBuildingIQ
{
    // ==================== CORE MODELS & INTERFACES ====================
    public class SensorReadings
    {
        public double Temperature { get; set; }
        public double LightLevel { get; set; }
        public double Humidity { get; set; }
        public string LightCategory { get; set; } = string.Empty;
        public string TemperatureStatus { get; set; } = string.Empty;
        public string HumidityStatus { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string LedColor { get; set; } = "Off";
        public bool IsAlert { get; set; }
    }

    public class SystemStats
    {
        public int TotalReadings { get; set; }
        public int AlertsTriggered { get; set; }
        public double UptimeMinutes { get; set; }
        public double AverageReadIntervalMs { get; set; }
    }

    public class Alert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public AlertLevel Level { get; set; }
        public DateTime TriggeredAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public enum AlertLevel
    {
        Info,
        Warning,
        Critical
    }

    // ==================== CORE INTERFACES ====================
    public interface ITemperatureSensor
    {
        double ReadTemperature();
        bool TryReadTemperature(out double temperature);
    }

    public interface ILightSensor
    {
        double ReadLightLevel();
        string GetLightCategory();
    }

    public interface IHumiditySensor
    {
        double ReadHumidity();
        bool TryReadHumidity(out double humidity);
    }

    public interface ILedController
    {
        void SetColor(string color);
        string GetCurrentColor();
    }

    public interface IPerformanceMonitor
    {
        SystemStats GetSystemStats();
        void RecordReading();
        void RecordAlert();
    }

    // ==================== CORE HARDWARE SIMULATORS ====================
    public class SimulatedTemperatureSensor : ITemperatureSensor
    {
        private readonly Random _random;
        private double _currentTemp = 22.0;
        private const double MinTemp = 15.0;
        private const double MaxTemp = 35.0;

        public SimulatedTemperatureSensor() => _random = new Random();

        public double ReadTemperature()
        {
            double change = (_random.NextDouble() - 0.5) * 1.5;
            _currentTemp = Math.Max(MinTemp, Math.Min(MaxTemp, _currentTemp + change));
            return Math.Round(_currentTemp, 1);
        }

        public bool TryReadTemperature(out double temperature)
        {
            if (_random.NextDouble() < 0.05)
            {
                temperature = 0;
                return false;
            }
            temperature = ReadTemperature();
            return true;
        }
    }

    public class SimulatedLightSensor : ILightSensor
    {
        private readonly Random _random;
        private int _cycleCounter = 0;

        public SimulatedLightSensor() => _random = new Random();

        public double ReadLightLevel()
        {
            _cycleCounter++;
            double timeOfDay = Math.Sin(_cycleCounter * 0.1) * 0.5 + 0.5;
            double noise = (_random.NextDouble() - 0.5) * 0.2;
            double lightLevel = Math.Max(0.0, Math.Min(1.0, timeOfDay + noise));
            return Math.Round(lightLevel, 2);
        }

        public string GetLightCategory()
        {
            double level = ReadLightLevel();
            return level switch
            {
                < 0.2 => "🌙 Dark",
                < 0.4 => "💡 Dim",
                < 0.7 => "☀️ Normal",
                _ => "🔆 Bright"
            };
        }
    }

    public class SimulatedHumiditySensor : IHumiditySensor
    {
        private readonly Random _random;
        private double _currentHumidity = 45.0;

        public SimulatedHumiditySensor() => _random = new Random();

        public double ReadHumidity()
        {
            double change = (_random.NextDouble() - 0.5) * 10.0;
            _currentHumidity = Math.Max(20.0, Math.Min(80.0, _currentHumidity + change));
            return Math.Round(_currentHumidity, 1);
        }

        public bool TryReadHumidity(out double humidity)
        {
            humidity = ReadHumidity();
            return _random.NextDouble() > 0.02;
        }
    }

    public class SimulatedLedController : ILedController
    {
        private string _currentColor = "Off";

        public void SetColor(string color)
        {
            _currentColor = color;
            Console.WriteLine($"💡 [HARDWARE] LED changed to: {color}");
        }

        public string GetCurrentColor() => _currentColor;
    }

    // ==================== CORE BUSINESS SERVICES ====================
    public class EnvironmentMonitorService
    {
        private readonly ITemperatureSensor _tempSensor;
        private readonly ILightSensor _lightSensor;
        private readonly IHumiditySensor _humiditySensor;
        private readonly ILedController _ledController;
        private readonly AlertSystem _alertSystem;

        public EnvironmentMonitorService(
            ITemperatureSensor tempSensor,
            ILightSensor lightSensor,
            IHumiditySensor humiditySensor,
            ILedController ledController,
            AlertSystem alertSystem)
        {
            _tempSensor = tempSensor;
            _lightSensor = lightSensor;
            _humiditySensor = humiditySensor;
            _ledController = ledController;
            _alertSystem = alertSystem;
        }

        public SensorReadings GetComprehensiveReading()
        {
            var readings = new SensorReadings
            {
                Temperature = _tempSensor.ReadTemperature(),
                LightLevel = _lightSensor.ReadLightLevel(),
                Humidity = _humiditySensor.ReadHumidity(),
                LightCategory = _lightSensor.GetLightCategory(),
                Timestamp = DateTime.Now
            };

            readings.TemperatureStatus = GetEnhancedTemperatureStatus(readings.Temperature);
            readings.HumidityStatus = GetHumidityStatus(readings.Humidity);
            readings.LedColor = UpdateMultiFactorLed(readings);
            readings.IsAlert = _alertSystem.CheckForAlerts(readings);

            return readings;
        }

        private string GetEnhancedTemperatureStatus(double temperature)
        {
            return temperature switch
            {
                < 10 => "❄️ Freezing",
                < 18 => "🌬️ Chilly",
                < 24 => "😊 Comfortable",
                < 30 => "🌡️ Warm",
                _ => "🔥 Hot"
            };
        }

        private string GetHumidityStatus(double humidity)
        {
            return humidity switch
            {
                < 30 => "🏜️ Dry",
                < 50 => "😊 Comfortable",
                < 70 => "💧 Humid",
                _ => "🌊 Very Humid"
            };
        }

        private string UpdateMultiFactorLed(SensorReadings readings)
        {
            if (readings.Temperature > 32 || readings.Humidity > 75)
                return "Red";
            else if (readings.Temperature < 15 || readings.Humidity < 25)
                return "Blue";
            else if (readings.LightLevel < 0.2)
                return "Purple";
            else
                return "Green";
        }
    }

    public class AlertSystem
    {
        private readonly List<Alert> _activeAlerts = new();
        private readonly PerformanceMonitor _perfMonitor;

        public AlertSystem(PerformanceMonitor perfMonitor)
        {
            _perfMonitor = perfMonitor;
        }

        public bool CheckForAlerts(SensorReadings readings)
        {
            var newAlerts = new List<Alert>();

            if (readings.Temperature > 35)
                newAlerts.Add(new Alert { Message = "🚨 CRITICAL: High temperature!", Level = AlertLevel.Critical });
            else if (readings.Temperature > 30)
                newAlerts.Add(new Alert { Message = "⚠️ WARNING: Temperature rising", Level = AlertLevel.Warning });
            else if (readings.Temperature < 5)
                newAlerts.Add(new Alert { Message = "🚨 CRITICAL: Freezing temperature!", Level = AlertLevel.Critical });

            if (readings.Humidity > 85)
                newAlerts.Add(new Alert { Message = "💧 HIGH HUMIDITY: Risk of condensation", Level = AlertLevel.Warning });
            else if (readings.Humidity < 20)
                newAlerts.Add(new Alert { Message = "🏜️ LOW HUMIDITY: Dry conditions", Level = AlertLevel.Info });

            if (readings.LightLevel < 0.1)
                newAlerts.Add(new Alert { Message = "🌙 NIGHT MODE: Low light conditions", Level = AlertLevel.Info });

            foreach (var alert in newAlerts)
            {
                _activeAlerts.Add(alert);
                _perfMonitor.RecordAlert();
                Console.ForegroundColor = alert.Level == AlertLevel.Critical ? ConsoleColor.Red :
                                       alert.Level == AlertLevel.Warning ? ConsoleColor.Yellow : ConsoleColor.Blue;
                Console.WriteLine($"[ALERT] {alert.Message}");
                Console.ResetColor();
            }

            return newAlerts.Any();
        }

        public List<Alert> GetActiveAlerts() => _activeAlerts.Where(a => a.IsActive).ToList();
    }

    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly List<DateTime> _readings = new();
        private readonly List<DateTime> _alerts = new();
        private readonly DateTime _startTime = DateTime.Now;

        public SystemStats GetSystemStats()
        {
            var now = DateTime.Now;
            return new SystemStats
            {
                TotalReadings = _readings.Count,
                AlertsTriggered = _alerts.Count,
                UptimeMinutes = (now - _startTime).TotalMinutes,
                AverageReadIntervalMs = _readings.Count > 1 ?
                    (_readings.Last() - _readings.First()).TotalMilliseconds / _readings.Count : 0
            };
        }

        public void RecordReading() => _readings.Add(DateTime.Now);
        public void RecordAlert() => _alerts.Add(DateTime.Now);
    }

    // ==================== HONEYWELL-SPECIFIC MODELS ====================
    public class BuildingZone
    {
        public string ZoneId { get; set; } = "Zone-A";
        public string ZoneName { get; set; } = "Main Office Area";
        public ZoneType Type { get; set; } = ZoneType.Office;
        public double TargetTemperature { get; set; } = 22.0;
        public double TargetHumidity { get; set; } = 45.0;
    }

    public enum ZoneType
    {
        Office,
        ServerRoom,
        Laboratory,
        Manufacturing,
        Warehouse
    }

    public class HVACStatus
    {
        public bool IsHeating { get; set; }
        public bool IsCooling { get; set; }
        public bool IsVentilating { get; set; }
        public int FanSpeed { get; set; }
        public double EnergyConsumption { get; set; }
    }

    public class EnergyMetrics
    {
        public double TotalEnergyUsed { get; set; }
        public double CarbonFootprint { get; set; }
        public double EnergyCost { get; set; }
        public double EfficiencyScore { get; set; }
    }

    public class ComplianceAlert
    {
        public string Regulation { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public ComplianceLevel Level { get; set; }
        public DateTime DetectedAt { get; set; }
    }

    public enum ComplianceLevel
    {
        Compliant,
        Warning,
        Violation
    }

    // ==================== HONEYWELL-SPECIFIC INTERFACES ====================
    public interface IHVACController
    {
        HVACStatus GetStatus();
        void SetTemperature(double targetTemp);
        void OptimizeForOccupancy(int peopleCount);
        EnergyMetrics GetEnergyMetrics();
    }

    public interface IOccupancySensor
    {
        int GetPeopleCount();
        bool IsOccupied();
        DateTime LastMovement { get; }
    }

    public interface IComplianceChecker
    {
        List<ComplianceAlert> CheckRegulations(SensorReadings readings, HVACStatus hvacStatus);
    }

    // ==================== HONEYWELL-SPECIFIC SERVICES ====================
    public class SmartHVACController : IHVACController
    {
        private readonly Random _random = new();
        private double _currentEnergy = 0.0;
        
        public HVACStatus GetStatus()
        {
            return new HVACStatus
            {
                IsHeating = _random.NextDouble() > 0.7,
                IsCooling = _random.NextDouble() > 0.6,
                IsVentilating = true,
                FanSpeed = _random.Next(30, 80),
                EnergyConsumption = Math.Round(_random.NextDouble() * 5.0, 2)
            };
        }

        public void SetTemperature(double targetTemp)
        {
            Console.WriteLine($"🎯 [HVAC] Set point adjusted to {targetTemp}°C");
        }

        public void OptimizeForOccupancy(int peopleCount)
        {
            var fanSpeed = peopleCount > 10 ? 80 : 50;
            Console.WriteLine($"👥 [HVAC] Optimized for {peopleCount} occupants - Fan: {fanSpeed}%");
        }

        public EnergyMetrics GetEnergyMetrics()
        {
            _currentEnergy += 0.1;
            return new EnergyMetrics
            {
                TotalEnergyUsed = Math.Round(_currentEnergy, 1),
                CarbonFootprint = Math.Round(_currentEnergy * 0.233, 2),
                EnergyCost = Math.Round(_currentEnergy * 0.34, 2),
                EfficiencyScore = Math.Round(85 + _random.NextDouble() * 15, 1)
            };
        }
    }

    public class OccupancySensor : IOccupancySensor
    {
        private readonly Random _random = new();
        public DateTime LastMovement { get; private set; } = DateTime.Now;

        public int GetPeopleCount()
        {
            var hour = DateTime.Now.Hour;
            int baseCount = hour switch
            {
                >= 9 and <= 17 => 15,
                >= 7 and < 9 => 5,
                > 17 and <= 20 => 3,
                _ => 1
            };
            
            return Math.Max(0, baseCount + _random.Next(-3, 4));
        }

        public bool IsOccupied() => GetPeopleCount() > 0;
    }

    public class UKComplianceChecker : IComplianceChecker
    {
        public List<ComplianceAlert> CheckRegulations(SensorReadings readings, HVACStatus hvacStatus)
        {
            var alerts = new List<ComplianceAlert>();

            if (readings.Temperature < 16.0)
            {
                alerts.Add(new ComplianceAlert
                {
                    Regulation = "UK Workplace Regulations",
                    Requirement = "Minimum temperature 16°C",
                    Level = ComplianceLevel.Violation,
                    DetectedAt = DateTime.Now
                });
            }

            if (readings.Temperature > 30.0 && hvacStatus.IsCooling == false)
            {
                alerts.Add(new ComplianceAlert 
                {
                    Regulation = "HSE Guidelines", 
                    Requirement = "Cooling required above 30°C",
                    Level = ComplianceLevel.Warning,
                    DetectedAt = DateTime.Now
                });
            }

            if (readings.Humidity > 70.0)
            {
                alerts.Add(new ComplianceAlert
                {
                    Regulation = "Building Standards",
                    Requirement = "Humidity should be 40-70%",
                    Level = ComplianceLevel.Warning,
                    DetectedAt = DateTime.Now
                });
            }

            if (hvacStatus.EnergyConsumption > 4.0)
            {
                alerts.Add(new ComplianceAlert
                {
                    Regulation = "Energy Efficiency",
                    Requirement = "High energy consumption detected",
                    Level = ComplianceLevel.Warning,
                    DetectedAt = DateTime.Now
                });
            }

            return alerts;
        }
    }

    // ==================== ENHANCED BUILDING SERVICES ====================
    public class BuildingManagementService
    {
        private readonly EnvironmentMonitorService _envMonitor;
        private readonly IHVACController _hvacController;
        private readonly IOccupancySensor _occupancySensor;
        private readonly IComplianceChecker _complianceChecker;
        private readonly AlertSystem _alertSystem;

        public BuildingManagementService(
            EnvironmentMonitorService envMonitor,
            IHVACController hvacController,
            IOccupancySensor occupancySensor,
            IComplianceChecker complianceChecker,
            AlertSystem alertSystem)
        {
            _envMonitor = envMonitor;
            _hvacController = hvacController;
            _occupancySensor = occupancySensor;
            _complianceChecker = complianceChecker;
            _alertSystem = alertSystem;
        }

        public BuildingStatus GetCompleteBuildingStatus()
        {
            var envReadings = _envMonitor.GetComprehensiveReading();
            var hvacStatus = _hvacController.GetStatus();
            var peopleCount = _occupancySensor.GetPeopleCount();
            var energyMetrics = _hvacController.GetEnergyMetrics();
            var complianceAlerts = _complianceChecker.CheckRegulations(envReadings, hvacStatus);

            if (peopleCount > 0)
            {
                _hvacController.OptimizeForOccupancy(peopleCount);
            }

            if (envReadings.Temperature > 25.0 && !hvacStatus.IsCooling)
            {
                _hvacController.SetTemperature(22.0);
            }

            return new BuildingStatus
            {
                Environment = envReadings,
                HVAC = hvacStatus,
                Occupancy = peopleCount,
                Energy = energyMetrics,
                ComplianceAlerts = complianceAlerts,
                Timestamp = DateTime.Now
            };
        }
    }

    public class BuildingStatus
    {
        public SensorReadings Environment { get; set; } = new();
        public HVACStatus HVAC { get; set; } = new();
        public int Occupancy { get; set; }
        public EnergyMetrics Energy { get; set; } = new();
        public List<ComplianceAlert> ComplianceAlerts { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    // ==================== ENHANCED DATA LOGGER ====================
    public class BuildingDataLogger
    {
        private readonly string _csvFile = "building_metrics.csv";

        public BuildingDataLogger()
        {
            if (!File.Exists(_csvFile))
            {
                File.WriteAllText(_csvFile, "Timestamp,Temperature,Humidity,Light,Occupancy,EnergyUsed,EnergyCost,CarbonFootprint,Efficiency,ComplianceAlerts\n");
            }
        }

        public void LogBuildingData(BuildingStatus status)
        {
            var csvEntry = $"{status.Timestamp:yyyy-MM-dd HH:mm:ss},{status.Environment.Temperature},{status.Environment.Humidity},{status.Environment.LightLevel},{status.Occupancy},{status.Energy.TotalEnergyUsed},{status.Energy.EnergyCost},{status.Energy.CarbonFootprint},{status.Energy.EfficiencyScore},{status.ComplianceAlerts.Count}\n";
            File.AppendAllText(_csvFile, csvEntry);

            if (DateTime.Now.Second % 20 == 0)
            {
                var jsonEntry = JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("building_status.json", jsonEntry);
            }
        }
    }
    // Add this class to show predictive capabilities
    public class PredictiveMaintenance
    {
        private readonly Dictionary<string, int> _componentHours = new()
        {
            {"HVAC_Compressor", 2450},
            {"Air_Handler", 1800},
            {"Chiller", 3200},
            {"Boiler", 1500}
        };

        public void CheckMaintenanceNeeds()
        {
            var needsMaintenance = _componentHours.Where(x => x.Value > 2000).ToList();
            if (needsMaintenance.Any() && DateTime.Now.Minute % 5 == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"🔧 [PREDICTIVE] Maintenance due: {string.Join(", ", needsMaintenance.Select(x => x.Key))}");
                Console.ResetColor();
            }
        }
    }

    // ==================== ENHANCED DASHBOARD ====================
    public class HoneywellDashboard
    {
        public void DisplayBuildingOverview(BuildingStatus status)
        {
            if (DateTime.Now.Second % 15 == 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("🏢 [HONEYWELL BMS] Building Overview:");
                Console.WriteLine($"   👥 Occupancy: {status.Occupancy} people");
                Console.WriteLine($"   💡 Energy: {status.Energy.TotalEnergyUsed} kWh (£{status.Energy.EnergyCost})");
                Console.WriteLine($"   🌱 Carbon: {status.Energy.CarbonFootprint} kg CO2");
                Console.WriteLine($"   ⚡ Efficiency: {status.Energy.EfficiencyScore}%");
                Console.ResetColor();
            }
        }
    }

    // ==================== MAIN WORKER SERVICE ====================
    public class BuildingMonitoringWorker : BackgroundService
    {
        private readonly BuildingManagementService _buildingService;
        private readonly BuildingDataLogger _dataLogger;
        private readonly HoneywellDashboard _dashboard;
        private readonly PerformanceMonitor _perfMonitor;
        private readonly ILogger<BuildingMonitoringWorker> _logger;

        public BuildingMonitoringWorker(
            BuildingManagementService buildingService,
            BuildingDataLogger dataLogger,
            HoneywellDashboard dashboard,
            PerformanceMonitor perfMonitor,
            ILogger<BuildingMonitoringWorker> logger)
        {
            _buildingService = buildingService;
            _dataLogger = dataLogger;
            _dashboard = dashboard;
            _perfMonitor = perfMonitor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Honeywell BuildingIQ monitoring started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var buildingStatus = _buildingService.GetCompleteBuildingStatus();
                    _perfMonitor.RecordReading();
                    _dataLogger.LogBuildingData(buildingStatus);
                    _dashboard.DisplayBuildingOverview(buildingStatus);

                    DisplayHoneywellStatus(buildingStatus);
                    
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in building monitoring loop");
                }
            }
        }

        private void DisplayHoneywellStatus(BuildingStatus status)
        {
            Console.Clear();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== 🏢 HONEYWELL BuildingIQ DEMO ===");
            Console.ResetColor();
            
            Console.WriteLine($"🕒 {status.Timestamp:HH:mm:ss} | 📊 Readings: {_perfMonitor.GetSystemStats().TotalReadings}");
            Console.WriteLine("=========================================");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🌡️  ENVIRONMENTAL DATA");
            Console.ResetColor();
            Console.WriteLine($"Temperature: {status.Environment.Temperature}°C - {status.Environment.TemperatureStatus}");
            Console.WriteLine($"Humidity: {status.Environment.Humidity}% - {status.Environment.HumidityStatus}");
            Console.WriteLine($"Light: {status.Environment.LightLevel:P0} - {status.Environment.LightCategory}");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n❄️  HVAC STATUS");
            Console.ResetColor();
            Console.WriteLine($"Heating: {(status.HVAC.IsHeating ? "ON 🔥" : "OFF")}");
            Console.WriteLine($"Cooling: {(status.HVAC.IsCooling ? "ON ❄️" : "OFF")}");
            Console.WriteLine($"Ventilation: {(status.HVAC.IsVentilating ? "ON 💨" : "OFF")}");
            Console.WriteLine($"Fan Speed: {status.HVAC.FanSpeed}%");
            Console.WriteLine($"Energy Use: {status.HVAC.EnergyConsumption} kWh");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📈 BUSINESS INTELLIGENCE");
            Console.ResetColor();
            Console.WriteLine($"Occupancy: {status.Occupancy} people");
            Console.WriteLine($"Total Energy: {status.Energy.TotalEnergyUsed} kWh");
            Console.WriteLine($"Energy Cost: £{status.Energy.EnergyCost}");
            Console.WriteLine($"Carbon Footprint: {status.Energy.CarbonFootprint} kg CO2");
            Console.WriteLine($"Efficiency Score: {status.Energy.EfficiencyScore}%");
            
            if (status.ComplianceAlerts.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n⚖️  COMPLIANCE ALERTS: {status.ComplianceAlerts.Count}");
                Console.ResetColor();
                foreach (var alert in status.ComplianceAlerts.Take(2))
                {
                    var icon = alert.Level == ComplianceLevel.Violation ? "🚨" : "⚠️";
                    Console.WriteLine($"{icon} {alert.Regulation}: {alert.Requirement}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ COMPLIANCE: All regulations met");
                Console.ResetColor();
            }
            
            Console.WriteLine($"\n📍 Honeywell Poole Demo | BuildingIQ v2.0");
            Console.WriteLine("Press Ctrl+C to exit");
        }
    }

    // ==================== MAIN PROGRAM ====================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🏢 Honeywell BuildingIQ - Poole Interview Demo");
            Console.WriteLine("===============================================\n");

            var host = CreateHostBuilder(args).Build();
            DisplayHoneywellBanner();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Core sensors
                    services.AddSingleton<ITemperatureSensor, SimulatedTemperatureSensor>();
                    services.AddSingleton<ILightSensor, SimulatedLightSensor>();
                    services.AddSingleton<IHumiditySensor, SimulatedHumiditySensor>();
                    services.AddSingleton<ILedController, SimulatedLedController>();
                    
                    // Honeywell-specific services
                    services.AddSingleton<IHVACController, SmartHVACController>();
                    services.AddSingleton<IOccupancySensor, OccupancySensor>();
                    services.AddSingleton<IComplianceChecker, UKComplianceChecker>();
                    
                    // Business services
                    services.AddSingleton<PerformanceMonitor>();
                    services.AddSingleton<AlertSystem>();
                    services.AddSingleton<EnvironmentMonitorService>();
                    services.AddSingleton<BuildingManagementService>();
                    services.AddSingleton<BuildingDataLogger>();
                    services.AddSingleton<HoneywellDashboard>();
                    
                    // Main worker
                    services.AddHostedService<BuildingMonitoringWorker>();
                });

        static void DisplayHoneywellBanner()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
    ╔══════════════════════════════════════════════╗
    ║            HONEYWELL BuildingIQ              ║
    ║           Poole Interview Demo               ║
    ║                                              ║
    ║  🏢  Smart Building Management              ║
    ║  🌡️   HVAC Control & Optimization           ║
    ║  💡  Energy Efficiency Monitoring           ║
    ║  👥  Occupancy-Based Automation             ║
    ║  ⚖️   UK Regulatory Compliance              ║
    ║  📊  Real-time Business Intelligence        ║
    ╚══════════════════════════════════════════════╝
            ");
            Console.ResetColor();
            Console.WriteLine("Initializing Building Management System...\n");
            Console.WriteLine("📍 Demonstrating skills relevant to Honeywell Poole:");
            Console.WriteLine("   • Building Automation Systems (BAS)");
            Console.WriteLine("   • HVAC Control Logic");
            Console.WriteLine("   • Energy Management");
            Console.WriteLine("   • Regulatory Compliance");
            Console.WriteLine("   • Real-time Monitoring\n");
        }
    }
}