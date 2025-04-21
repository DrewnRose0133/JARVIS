# J.A.R.V.I.S. - Just A Rather Very Intelligent System

A fully integrated, smart-home AI assistant modeled after Tony Stark’s JARVIS system.

## 🔧 Features
- Voiceprint authentication + permission tiers
- Smart home control (lights, AC, garage, sensors)
- Visualizer UI with alerts, logs, and ripple effect
- OpenAI integration for conversational intelligence
- Push notifications, mobile interface, REST API
- Dynamic personality engine (JARVIS-style sarcasm + wit)
- Real-time presence & environmental awareness
- Self-diagnostics, scene automation, and more

## 🚀 Getting Started
1. Clone this repo:
   ```bash
   git clone https://github.com/DrewnRose0133/JARVIS
   ```
2. Open `JARVIS.sln` in Visual Studio (2022+)
3. Install .NET 8 SDK
4. Run the backend + visualizer

## 💬 Voice Examples
- “Open garage” → checks for user approval
- “Personality sarcastic” → JARVIS mode activated
- “Remind me to check the oven in 10 minutes”

## 🧠 Personality
JARVIS can be:
- Friendly
- Professional
- Sarcastic
- Full Paul Bettany–style J.A.R.V.I.S. (default)

Enjoy your own AI butler.

## 🔊 Audio Notes
This build uses NAudio for MP3 playback. Install via:
```
dotnet add package NAudio
```
