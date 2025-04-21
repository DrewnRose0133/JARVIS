
# J.A.R.V.I.S. AI Assistant Project Documentation

## Overview
This is a complete C# and XAML-powered smart home assistant modeled after Tony Stark's J.A.R.V.I.S. It integrates voice control, wake word detection, smart device APIs, and a role-based interface across desktop and mobile platforms.

---

## Project Structure

### JarvisAI.Core
- `AppState.cs`: Manages app-wide shared states including user, token, and session.

### JarvisAI.WPFUI
- `DashboardWindow.xaml`: Main dashboard interface using WPF.
- `DashboardWindow.xaml.cs`: Code-behind for dynamic control panel routing based on user role, includes voice output and interaction.

### JarvisAI.MobileApp
- `MainPage.xaml`: Xamarin.Forms UI for mobile interaction and command dispatch.

### JarvisAI.API
- `JarvisController.cs`: ASP.NET Core Web API controller for command processing and JWT auth.

### JarvisAI.Config
- `settings.json`: Wake word, scene, and device configuration.

### JarvisAI.Services
- `SmartThingsService.cs`: Interacts with SmartThings API for light and thermostat control.
- `RoombaService.cs`: Controls iRobot Roomba start/stop.
- `UbiquitiService.cs`: Network visibility and router commands.
- `GroceryService.cs`: Maintains and modifies the grocery list.

### JarvisAI.Voice
- `VoiceInputService.cs`: Captures and parses voice input.
- `WakeWordListener.cs`: Listens for "Hey Jarvis" using voice activation.
- `CommandProcessor.cs`: Maps commands to features or actions.

---

## Features
- 🔊 Wake Word Activation: Trigger AI by saying “Hey Jarvis”
- 🧠 Natural Command Routing: Commands like “turn off lights” or “start cleaning”
- 🔐 Role-Based Access: Admin, User, Guest dashboards
- 📱 Mobile App: Lightweight interface to send commands and check status
- 🌐 API Access: Secure HTTP API for remote and mobile use
- 🧾 Grocery List: Add/view items using voice or button
- 🎛️ Device Control: Smart lights, thermostat, Roomba, Ubiquiti router
- 🧬 Personality Engine: Customizable responses

---

## Requirements
- Visual Studio 2022+
- .NET 6 SDK or newer
- For voice: Windows speech recognition or 3rd-party voice SDK (Porcupine, Vosk)
- For mobile: Xamarin workload in Visual Studio

---

## Getting Started
1. Open the solution file (`JarvisAI.sln`)
2. Set `JarvisAI.WPFUI` as startup project
3. Run the app, log in, and interact using dashboard or voice
4. Mobile app and API can be launched independently

---

## Next Steps
- Integrate with real SmartThings and iRobot credentials
- Deploy API to Raspberry Pi or local network server
- Add logging and metrics dashboard
- Extend personality engine with GPT or ElevenLabs voice

Enjoy your JARVIS assistant! 😎
