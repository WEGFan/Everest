﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;

namespace Celeste {
    static class patch_BinaryPacker {

        [MonoModIgnore] // We don't want to change anything about the method...
        [ProxyFileCalls] // ... except for proxying all System.IO.File.* calls to Celeste.Mod.FileProxy.*
        public static extern BinaryPacker.Element FromBinary(string filename);

        public class Element : BinaryPacker.Element {
            public extern bool orig_HasAttr(string name);
            public new bool HasAttr(string name)
                => orig_HasAttr(name) || orig_HasAttr(name.ToLowerInvariant());

            public extern string orig_Attr(string name, string defaultValue = "");
            public new string Attr(string name, string defaultValue = "")
                => orig_Attr(name, orig_Attr(name.ToLowerInvariant(), defaultValue));

            public extern bool orig_AttrBool(string name, bool defaultValue = false);
            public new bool AttrBool(string name, bool defaultValue = false)
                => orig_AttrBool(name, orig_AttrBool(name.ToLowerInvariant(), defaultValue));

            public extern float orig_AttrFloat(string name, float defaultValue = 0f);
            public new float AttrFloat(string name, float defaultValue = 0f)
                => orig_AttrFloat(name, orig_AttrFloat(name.ToLowerInvariant(), defaultValue));
        }

    }
}
