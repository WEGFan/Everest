﻿using System.IO;

namespace Celeste.Mod {
    /// <summary>
    /// Per-save-slot mod data, binary formatted.
    /// Everest loads / saves this for you as .bin by default.
    /// </summary>
    public abstract class EverestModuleBinarySaveData : EverestModuleSaveData {

        /// <summary>
        /// Read the save data from the given BinaryReader to the current object.
        /// </summary>
        public abstract void Read(BinaryReader reader);
        /// <summary>
        /// Write the save data from the current object to the given BinaryWriter.
        /// </summary>
        public abstract void Write(BinaryWriter writer);

    }
}
