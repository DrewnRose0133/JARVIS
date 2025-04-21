
using System;

namespace JARVIS.Modules
{
    public static class SceneAutomation
    {
        public static void ActivateScene(string scene)
        {
            Logger.Log("Activating scene: " + scene);
            Console.WriteLine($"Scene '{scene}' activated.");
        }
    }
}
