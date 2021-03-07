﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Celeste.Mod {
    /// <summary>
    /// Allows custom decals to have properties that are otherwise hardcoded, such as reflections, parallax, etc.
    /// </summary>
    public static class DecalRegistry {

        // string is propertyName
        public static Dictionary<string, Action<Decal, XmlAttributeCollection>> PropertyHandlers = new Dictionary<string, Action<Decal, XmlAttributeCollection>>() {
            { "parallax", delegate(Decal decal, XmlAttributeCollection attrs) {
                ((patch_Decal)decal).MakeParallax(float.Parse(attrs["amount"].Value));
            }},
            { "smoke", delegate(Decal decal, XmlAttributeCollection attrs) {
                float offx = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                float offy = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                Vector2 offset = new Vector2(offx, offy);
                bool inbg = attrs["inbg"] != null ? bool.Parse(attrs["inbg"].Value) : false;
                ((patch_Decal)decal).CreateSmoke(offset, inbg);
            }},
            { "depth", delegate(Decal decal, XmlAttributeCollection attrs) {
                if (attrs["value"] != null)
                    decal.Depth = int.Parse(attrs["value"].Value);
            }},
            { "animationSpeed", delegate(Decal decal, XmlAttributeCollection attrs) {
                if (attrs["value"] != null)
                    decal.AnimationSpeed = int.Parse(attrs["value"].Value);
            }},
            { "floaty", delegate(Decal decal, XmlAttributeCollection attrs) {
                ((patch_Decal)decal).MakeFloaty();
            }},
            { "sound", delegate(Decal decal, XmlAttributeCollection attrs) {
                if (attrs["event"] != null)
                decal.Add(new SoundSource(attrs["event"].Value));
            }},
            { "bloom", delegate(Decal decal, XmlAttributeCollection attrs) {
                float offx = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                float offy = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                Vector2 offset = new Vector2(offx, offy);
                float alpha = attrs["alpha"] != null ? float.Parse(attrs["alpha"].Value) : 1f;
                float radius = attrs["radius"] != null ? float.Parse(attrs["radius"].Value) : 1f;
                decal.Add(new BloomPoint(offset, alpha, radius));
            }},
            { "coreSwap", delegate(Decal decal, XmlAttributeCollection attrs) {
                if (attrs["coldPath"] != null && attrs["hotPath"] != null) {
                    ((patch_Decal)decal).MakeCoreSwap(attrs["coldPath"].Value, attrs["hotPath"].Value);
                }
            }},
            { "mirror", delegate(Decal decal, XmlAttributeCollection attrs) {
                string text = decal.Name.ToLower();
                if (text.StartsWith("decals/"))
                {
                    text = text.Substring(7);
                }
                bool keepOffsetsClose = attrs["keepOffsetsClose"] != null ? bool.Parse(attrs["keepOffsetsClose"].Value) : false;
                ((patch_Decal)decal).MakeMirror(text,keepOffsetsClose );
            }},
            { "banner", delegate(Decal decal, XmlAttributeCollection attrs) {
                float offset = 0f;
                if (attrs["offset"] != null)
                    offset = float.Parse(attrs["offset"].Value);
                float speed = 1f;
                if (attrs["speed"] != null)
                    speed = float.Parse(attrs["speed"].Value);
                float amplitude = 1f;
                if (attrs["amplitude"] != null)
                    amplitude = float.Parse(attrs["amplitude"].Value);
                int sliceSize = 1;
                if (attrs["sliceSize"] != null)
                    sliceSize = int.Parse(attrs["sliceSize"].Value);
                float sliceSinIncrement = 1f;
                if (attrs["sliceSinIncrement"] != null)
                    sliceSinIncrement = float.Parse(attrs["sliceSinIncrement"].Value);
                bool easeDown = attrs["easeDown"] != null ? bool.Parse(attrs["easeDown"].Value) : false;
                bool onlyIfWindy = attrs["onlyIfWindy"] != null ? bool.Parse(attrs["onlyIfWindy"].Value): false;
                ((patch_Decal)decal).MakeBanner(speed, amplitude, sliceSize, sliceSinIncrement, easeDown, offset, onlyIfWindy);
            }},
            { "solid", delegate(Decal decal, XmlAttributeCollection attrs) {
                float x = 0;
                if (attrs["x"] != null)
                    x = float.Parse(attrs["x"].Value);
                float y = 0;
                if (attrs["y"] != null)
                    y = float.Parse(attrs["y"].Value);
                float width = 16;
                if (attrs["width"] != null)
                    width = float.Parse(attrs["width"].Value);
                float height = 16;
                if (attrs["height"] != null)
                    height = float.Parse(attrs["height"].Value);
                int index = SurfaceIndex.ResortRoof;
                if (attrs["index"] != null)
                    index = int.Parse(attrs["index"].Value);
                bool blockWaterfalls = attrs["blockWaterfalls"] != null ? bool.Parse(attrs["blockWaterfalls"].Value) : true;
                ((patch_Decal)decal).MakeSolid(x, y, width, height, index, blockWaterfalls);
            }},
            { "staticMover", delegate(Decal decal, XmlAttributeCollection attrs) {
                int x = 0;
                if (attrs["x"] != null)
                    x = int.Parse(attrs["x"].Value);
                int y = 0;
                if (attrs["y"] != null)
                    y = int.Parse(attrs["y"].Value);
                int width = 16;
                if (attrs["width"] != null)
                    width = int.Parse(attrs["width"].Value);
                int height = 16;
                if (attrs["height"] != null)
                    height = int.Parse(attrs["height"].Value);
                ((patch_Decal)decal).MakeStaticMover(x, y, width, height);
            }},
            { "scared", delegate(Decal decal, XmlAttributeCollection attrs) {
                int hideRange = 32;
                int showRange = 48;
                if (attrs["range"] != null)
                    hideRange = showRange = int.Parse(attrs["range"].Value);
                if (attrs["hideRange"] != null)
                    hideRange = int.Parse(attrs["hideRange"].Value);
                if (attrs["showRange"] != null)
                    showRange = int.Parse(attrs["showRange"].Value);
                int[] hideFrames = Calc.ReadCSVIntWithTricks(attrs["hideFrames"]?.Value ?? "0");
                int[] showFrames = Calc.ReadCSVIntWithTricks(attrs["showFrames"]?.Value ?? "0");
                int[] idleFrames = Calc.ReadCSVIntWithTricks(attrs["idleFrames"]?.Value ?? "0");
                int[] hiddenFrames = Calc.ReadCSVIntWithTricks(attrs["hiddenFrames"]?.Value ?? "0");
                ((patch_Decal)decal).MakeScaredAnimation(hideRange, showRange, idleFrames, hiddenFrames, showFrames, hideFrames);
            }},
            { "light", delegate(Decal decal, XmlAttributeCollection attrs) {
                float offx = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                float offy = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                Vector2 offset = new Vector2(offx, offy);
                Color color = attrs["color"] != null ? Calc.HexToColor(attrs["color"].Value) : Color.White;
                float alpha = attrs["alpha"] != null ? float.Parse(attrs["alpha"].Value) : 1f;
                int startFade = attrs["startFade"] != null ? int.Parse(attrs["startFade"].Value) : 16;
                int endFade = attrs["endFade"] != null ? int.Parse(attrs["endFade"].Value) : 24;
                decal.Add(new VertexLight(offset, color, alpha, startFade, endFade));
            }},
        };

        public static Dictionary<string, DecalInfo> RegisteredDecals = new Dictionary<string, DecalInfo>();

        public static void AddPropertyHandler(string propertyName, Action<Decal, XmlAttributeCollection> action) {
            if (PropertyHandlers.ContainsKey(propertyName)) {
                Logger.Log(LogLevel.Warn, "Decal Registry", $"Property handler for {propertyName} already exists! Replacing...");
                PropertyHandlers[propertyName] = action;
            } else {
                PropertyHandlers.Add(propertyName, action);
            }
        }

        /// <summary>
        /// Reads a DecalRegistry.xml file's contents
        /// </summary>
        public static void ReadDecalRegistryXml(string fileContents) {
            // XmlElement file = Calc.LoadXML(path)["decals"];
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fileContents);
            XmlElement file = doc["decals"];
            foreach (XmlNode node in file) {
                if (node is XmlElement decal) {
                    string decalPath = decal.Attr("path", null).ToLower();
                    if (decalPath == null) {
                        Logger.Log(LogLevel.Warn, "Decal Registry", "Decal didn't have a path attribute!");
                        continue;
                    }
                    DecalInfo info = new DecalInfo();
                    info.CustomProperties = new List<KeyValuePair<string, XmlAttributeCollection>>();
                    // Read all the properties
                    foreach (XmlNode node2 in decal.ChildNodes) {
                        if (node2 is XmlElement property) {
                            if (property.Attributes == null) {
                                property.SetAttribute("a", "only here to prevent crashes");
                            }
                            info.CustomProperties.Add(new KeyValuePair<string, XmlAttributeCollection>(property.Name, property.Attributes));
                        }
                    }

                    if (RegisteredDecals.ContainsKey(decalPath)) {
                        Logger.Log("Decal Registry", $"Replaced decal {decalPath}");
                        RegisteredDecals[decalPath] = info;
                    } else {
                        Logger.Log("Decal Registry", $"Registered decal {decalPath}");
                        RegisteredDecals.Add(decalPath, info);
                    }

                }
            }
        }

        public static void LoadDecalRegistry() {
            foreach (ModAsset asset in
                Everest.Content.Mods
                .Select(mod => mod.Map.TryGetValue("DecalRegistry", out ModAsset asset) ? asset : null)
                .Where(asset => asset != null && asset.Type == typeof(AssetTypeDecalRegistry))
            ) {
                string fileContents;
                using (StreamReader reader = new StreamReader(asset.Stream)) {
                    fileContents = reader.ReadToEnd();
                }
                ReadDecalRegistryXml(fileContents);
            }
        }

        public struct DecalInfo {
            /// <summary>
            /// PropertyName -> AttributeCollection
            /// </summary>
            public List<KeyValuePair<string, XmlAttributeCollection>> CustomProperties;
        }
    }
}
