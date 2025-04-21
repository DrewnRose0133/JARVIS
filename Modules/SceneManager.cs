
using System;

namespace JARVIS.Modules
{
    public static class SceneManager
    {
        public static void LoadScene(string sceneName)
        {
            Logger.Log($"Scene '{sceneName}' activated.");
            Console.WriteLine($"Setting lights, music, and temperature for '{sceneName}' mode.");
        }
    }
}
