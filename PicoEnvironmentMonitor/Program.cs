using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProfessionalIoTMonitor
{
    // ==================== INTERFACES ====================
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

    // ==================== MODELS ====================
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

    // ==================== HARDWARE SIMULATORS ====================
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
            if (_random.NextDouble() < 0.05) // 5% failure rate
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
            return _random.NextDouble() > 0.02; // 2% failure rate
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

    // ==================== BUSINESS SERVICES ====================
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

    public class DataLogger
    {
        private readonly string _csvFile = "sensor_data.csv";
        private readonly string _jsonFile = "sensor_data.json";

        public DataLogger()
        {
            if (!File.Exists(_csvFile))
            {
                File.WriteAllText(_csvFile, "Timestamp,Temperature,LightLevel,Humidity,LightCategory,TemperatureStatus,LedColor,IsAlert\n");
            }
        }

        public void LogReading(SensorReadings reading)
        {
            var csvEntry = $"{reading.Timestamp:yyyy-MM-dd HH:mm:ss},{reading.Temperature},{reading.LightLevel},{reading.Humidity},{reading.LightCategory},{reading.TemperatureStatus},{reading.LedColor},{reading.IsAlert}\n";
            File.AppendAllText(_csvFile, csvEntry);

            if (DateTime.Now.Second % 20 == 0)
            {
                var jsonEntry = System.Text.Json.JsonSerializer.Serialize(reading, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_jsonFile, jsonEntry);
            }
        }
    }

    public class WebDashboard
    {
        private readonly List<SensorReadings> _recentReadings = new();
        private const int MAX_HISTORY = 50;

        public void UpdateDashboard(SensorReadings reading)
        {
            _recentReadings.Add(reading);
            if (_recentReadings.Count > MAX_HISTORY)
                _recentReadings.RemoveAt(0);

            if (DateTime.Now.Second % 30 == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🌐 [WEB] Dashboard updated - {_recentReadings.Count} readings in history");
                Console.ResetColor();
            }
        }

        public List<SensorReadings> GetRecentReadings() => _recentReadings;
    }

    // ==================== MAIN WORKER SERVICE ====================
    public class MonitoringWorker : BackgroundService
    {
        private readonly EnvironmentMonitorService _monitorService;
        private readonly AlertSystem _alertSystem;
        private readonly DataLogger _dataLogger;
        private readonly WebDashboard _webDashboard;
        private readonly PerformanceMonitor _perfMonitor;
        private readonly ILogger<MonitoringWorker> _logger;

        public MonitoringWorker(
            EnvironmentMonitorService monitorService,
            AlertSystem alertSystem,
            DataLogger dataLogger,
            WebDashboard webDashboard,
            PerformanceMonitor perfMonitor,
            ILogger<MonitoringWorker> logger)
        {
            _monitorService = monitorService;
            _alertSystem = alertSystem;
            _dataLogger = dataLogger;
            _webDashboard = webDashboard;
            _perfMonitor = perfMonitor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Monitoring worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var readings = _monitorService.GetComprehensiveReading();
                    _perfMonitor.RecordReading();
                    _dataLogger.LogReading(readings);
                    _webDashboard.UpdateDashboard(readings);

                    DisplayEnhancedStatus(readings, _perfMonitor.GetSystemStats());

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitoring loop");
                }
            }
        }

        private void DisplayEnhancedStatus(SensorReadings readings, SystemStats stats)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== 🚀 PROFESSIONAL IOT MONITOR ===");
            Console.ResetColor();

            Console.WriteLine($"🕒 {readings.Timestamp:HH:mm:ss} | 📊 Readings: {stats.TotalReadings} | ⚠️ Alerts: {stats.AlertsTriggered}");
            Console.WriteLine("----------------------------------------");

            Console.ForegroundColor = readings.Temperature > 30 ? ConsoleColor.Red :
                                    readings.Temperature < 15 ? ConsoleColor.Blue : ConsoleColor.Green;
            Console.WriteLine($"🌡️  Temperature: {readings.Temperature}°C - {readings.TemperatureStatus}");
            Console.ResetColor();

            Console.ForegroundColor = readings.Humidity > 70 ? ConsoleColor.Yellow : ConsoleColor.Green;
            Console.WriteLine($"💧 Humidity: {readings.Humidity}% - {readings.HumidityStatus}");
            Console.ResetColor();

            Console.WriteLine($"💡 Light: {readings.LightLevel:P0} - {readings.LightCategory}");
            Console.WriteLine($"🎨 LED: {readings.LedColor} | 🔔 Alerts: {(readings.IsAlert ? "ACTIVE" : "None")}");
            Console.WriteLine($"⏱️  Uptime: {stats.UptimeMinutes:F1}m | 📈 Avg Interval: {stats.AverageReadIntervalMs:F0}ms");

            var activeAlerts = _alertSystem.GetActiveAlerts();
            if (activeAlerts.Any())
            {
                Console.WriteLine("\n🚨 ACTIVE ALERTS:");
                foreach (var alert in activeAlerts.Take(3))
                {
                    Console.ForegroundColor = alert.Level == AlertLevel.Critical ? ConsoleColor.Red : ConsoleColor.Yellow;
                    Console.WriteLine($"   • {alert.Message}");
                }
                Console.ResetColor();
            }

            Console.WriteLine("\nPress Ctrl+C to exit | Professional IoT System v1.0");
        }
    }

    // ==================== MAIN PROGRAM ====================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Professional IoT Environment Monitor");
            Console.WriteLine("========================================\n");

            var host = CreateHostBuilder(args).Build();
            DisplayStartupBanner();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ITemperatureSensor, SimulatedTemperatureSensor>();
                    services.AddSingleton<ILightSensor, SimulatedLightSensor>();
                    services.AddSingleton<IHumiditySensor, SimulatedHumiditySensor>();
                    services.AddSingleton<ILedController, SimulatedLedController>();

                    services.AddSingleton<PerformanceMonitor>();
                    services.AddSingleton<AlertSystem>();
                    services.AddSingleton<DataLogger>();
                    services.AddSingleton<WebDashboard>();
                    services.AddSingleton<EnvironmentMonitorService>();

                    services.AddHostedService<MonitoringWorker>();
                });

        static void DisplayStartupBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
    ╔═══════════════════════════════════════════╗
    ║          PROFESSIONAL IoT MONITOR         ║
    ║           Hardware Interview Demo         ║
    ║                                           ║
    ║  🌡️  Temperature Monitoring              ║
    ║  💡 Light Level Detection                ║
    ║  💧 Humidity Tracking                    ║
    ║  🚨 Smart Alert System                   ║
    ║  📊 Web Dashboard                        ║
    ║  📈 Performance Metrics                  ║
    ╚═══════════════════════════════════════════╝
            ");
            Console.ResetColor();
            Console.WriteLine("Starting services...\n");
        }
    }
}