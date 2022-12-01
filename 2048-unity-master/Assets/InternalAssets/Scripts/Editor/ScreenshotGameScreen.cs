using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace RimuruDev.Editors
{
    public class ScreenshotGameScreen : Editor
    {
        [MenuItem("Tool/Take Screenshot")]
        public static void TakeSkreenshot()
        {
            var name = $"Screenshot-{GUID.Generate()}.png";

            Directory.CreateDirectory("Tool/Screenshot");

            ScreenCapture.CaptureScreenshot($"Tool/Screenshot/{name}", 10);
        }
    }
}