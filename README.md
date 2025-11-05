# Honeywell BuildingIQ - Building Management System

A professional .NET-based Building Management System demo showcasing enterprise IoT architecture, smart HVAC control, and energy optimization.

![Honeywell BuildingIQ Demo](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/License-MIT-blue)
![Platform](https://img.shields.io/badge/Platform-Cross--platform-green)

## Overview

This project demonstrates a complete Building Management System (BMS) inspired by Honeywell's BuildingIQ platform. It features real-time environmental monitoring, smart HVAC control, energy optimization, and regulatory compliance checking.

**Perfect for demonstrating skills in:**
- Enterprise .NET architecture
- IoT and sensor integration  
- Building automation systems
- Energy management
- Real-time data processing

## Features

### Core Building Management
- **Real-time Environmental Monitoring** - Temperature, humidity, light levels
- **Smart HVAC Control** - Heating, cooling, ventilation with occupancy optimization
- **Energy Management** - Consumption tracking, cost analysis, carbon footprint
- **Occupancy Analytics** - People counting and space utilization

### Technical Excellence
- **Enterprise .NET 8** - Modern C# with dependency injection
- **Clean Architecture** - Interface-based design for testability
- **Background Services** - Continuous monitoring with hosted services
- **Data Persistence** - CSV and JSON logging with rotation

### Business Intelligence
- **UK Regulatory Compliance** - Workplace temperature and humidity regulations
- **Predictive Maintenance** - Component runtime monitoring and alerts
- **Efficiency Scoring** - Real-time system performance metrics
- **Cost Optimization** - Energy consumption and carbon tracking

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Installation
```bash
# Clone the repository
git clone https://github.com/yourusername/honeywell-buildingiq-demo.git
cd honeywell-buildingiq-demo

# Build and run
dotnet build
dotnet run
```

### Sample Output
```
=== 🏢 HONEYWELL BuildingIQ DEMO ===
🕒 14:21:30 | 📊 Readings: 5
🌡️  ENVIRONMENTAL DATA
Temperature: 21.1°C - 😊 Comfortable
Humidity: 51.3% - 💧 Humid
❄️  HVAC STATUS
Heating: ON 🔥 | Cooling: OFF
📈 BUSINESS INTELLIGENCE
Occupancy: 13 people | Efficiency: 91.6%
✅ COMPLIANCE: All regulations met
```

## Architecture

```
HoneywellBuildingIQ/
├── Program.cs                 # Main application host
├── Models/                    # Data models (SensorReadings, BuildingStatus)
├── Interfaces/               # Hardware abstractions (ITemperatureSensor, IHVACController)
├── Services/                 # Business logic (EnvironmentMonitor, BuildingManagement)
├── Hardware/                 # Simulated hardware implementations
└── Data/                     # Logging and persistence
```

### Key Design Patterns
- **Dependency Injection** - Microsoft.Extensions.Hosting
- **Interface Segregation** - Hardware abstraction layer
- **Background Services** - Continuous monitoring
- **Strategy Pattern** - Multiple sensor implementation

## Use Cases

### Commercial Buildings
- Office environment monitoring and optimization
- HVAC system control and energy management
- Regulatory compliance reporting

### Educational Demo
- .NET IoT and enterprise architecture patterns
- Building automation system concepts
- Real-time data processing examples

### Production Ready Features
- Error handling and resilience
- Configurable thresholds
- Extensible sensor framework
- Professional logging

## Technology Stack

- **.NET 8.0** - Cross-platform runtime
- **C# 12** - Modern language features
- **Microsoft.Extensions.Hosting** - Dependency injection
- **System.Text.Json** - Data serialization

## Project Evolution

### Version 2.1 (Current)
- ✅ Predictive maintenance capabilities
- ✅ Enhanced business intelligence
- ✅ UK regulatory compliance
- ✅ Energy cost optimization

### Version 2.0 
- ✅ Building management features
- ✅ HVAC control logic
- ✅ Occupancy analytics
- ✅ Carbon footprint tracking

### Version 1.0
- ✅ Core environmental monitoring
- ✅ Basic alert system
- ✅ Data logging
- ✅ Web dashboard simulation


**`LICENSE`** (MIT License)
MIT License
Copyright (c) 2024 [Your Name]
[Standard MIT license text]



