using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomCursors
{
    [HarmonyPatch]
    [BepInPlugin("com.steven.trombone.customcursors", "Custom Cursors", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        static Texture2D cursorTexture;
        static CustomCursor customCursor;

        int lastCursor = -1;

        void Awake()
        {
            var harmony = new Harmony("com.steven.trombone.customcursors");
            harmony.PatchAll();

            LoadCustomCursor();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                CycleCustomCursor();

                var methodInfo = customCursor.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                methodInfo?.Invoke(customCursor, null);
            }
        }

        void LoadCustomCursor()
        {
            var files = GetFiles();
            if (files.Length == 0) return;

            var nextCursor = Random.Range(0, files.Length);
            LoadCursorFromFile(files, nextCursor);
        }

        void CycleCustomCursor()
        {
            var files = GetFiles();
            if (files.Length <= 1) return;

            var nextCursor = lastCursor + 1;
            if (nextCursor >= files.Length) nextCursor = 0;

            LoadCursorFromFile(files, nextCursor);
        }

        string[] GetFiles()
        {
            var allowedFileTypes = new List<string>() { ".png", ".jpg" };
            var directory = Path.Combine(Paths.BepInExRootPath, "CustomCursors");
            Directory.CreateDirectory(directory);

            var files = Directory.GetFiles(directory).Where((x) => allowedFileTypes.Contains(Path.GetExtension(x).ToLower()));

            return files.ToArray();
        }

        private void LoadCursorFromFile(string[] files, int nextCursor)
        {
            var file = files[nextCursor];
            lastCursor = nextCursor;

            try
            {
                cursorTexture = LoadTextureRaw(File.ReadAllBytes(file));
                cursorTexture.wrapMode = TextureWrapMode.Clamp;
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}\n{e.StackTrace}");
            }
        }

        public static Texture2D LoadTextureRaw(byte[] bytes)
        {
            if (bytes.Count() > 0)
            {
                Texture2D Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(bytes))
                    return Tex2D;
            }
            return null;
        }

        [HarmonyPatch(typeof(CustomCursor), "Start")]
        static void Prefix(CustomCursor __instance)
        {
            customCursor ??= __instance;
            if (cursorTexture) __instance.cursorTexture = cursorTexture;
        }
    }
}
