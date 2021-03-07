﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using MonoMod;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Celeste {
    static class patch_UserIO {

        [MonoModIgnore]
        public static bool Saving { get; private set; }

        private static extern string orig_GetSavePath(string dir);
        [MonoModIfFlag("FNA")]
        private static string GetSavePath(string dir) {
            string env = Environment.GetEnvironmentVariable("EVEREST_SAVEPATH");
            if (!string.IsNullOrEmpty(env))
                return Path.Combine(env, dir);

            try {
                return orig_GetSavePath(dir);
            } catch (NotSupportedException) {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dir);
            }
        }

        [MonoModIgnore]
        private static extern string GetHandle(string name);

        public static string GetSaveFilePath(string name = null)
            => string.IsNullOrEmpty(name) ? Path.GetDirectoryName(GetSaveFilePath("dummy")) : GetHandle(name);

        [MonoModIgnore]
        [PatchSaveRoutine]
        private static extern IEnumerator SaveRoutine(bool file, bool settings);

        [MonoModLinkFrom("System.Collections.IEnumerator Celeste.UserIO::SaveHandler(System.Boolean,System.Boolean)")]
        public static IEnumerator SaveHandlerLegacy(bool file, bool settings) {
            if (Saving)
                return SaveNonHandler();
            Saving = true;
            // Note how we're calling SaveRoutine, not orig_SaveHandler.
            return new SafeRoutine(SaveRoutine(file, settings));
        }

        // Patch the Deserialize method so that it doesn't use BinaryFormatter (that causes an arbitrary code execution vulnerability).
        [MonoModReplace]
        private static T Deserialize<T>(Stream stream) where T : class {
            return (T) new XmlSerializer(typeof(T)).Deserialize(stream);
        }

        public static extern T orig_Load<T>(string path, bool backup = false);
        public static T Load<T>(string path, bool backup = false) {
            T result = orig_Load<T>(path, backup);

            // if we are loading a SaveData, fill out the FileSlot right away.
            if (typeof(T) == typeof(SaveData) && result != null) {
                if (path == "debug") {
                    (result as SaveData).FileSlot = -1;
                } else if (int.TryParse(path, out int slot)) {
                    (result as SaveData).FileSlot = slot;
                }
            }

            return result;
        }

        private static IEnumerator SaveNonHandler() {
            yield break;
        }

        public static T Load<T>(string path) where T : class
            => UserIO.Load<T>(path, false);

    }
}
