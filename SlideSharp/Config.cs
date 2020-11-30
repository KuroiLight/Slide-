using System;
using System.ComponentModel;
using System.IO;
using Tinyhand;

namespace SlideSharp
{
    [TinyhandObject]
    public sealed partial class Config
    {
        [Key(0)]
        [DefaultValue(50)]
        private int _mouseDragDeadzone = 50;

        [Key(1)]
        [DefaultValue(35)]
        private int _windowHiddenOffset = 35;

        [Key(2)]
        [DefaultValue(0.025)]
        private double _windowMovementSpeed = 0.025;

        [IgnoreMember]
        public int MouseDragDeadzone { get { return _mouseDragDeadzone; } set { _mouseDragDeadzone = Math.Clamp(value, 1, 1000); } }

        [IgnoreMember]
        public int WindowHiddenOffset { get { return _windowHiddenOffset; } set { _windowHiddenOffset = Math.Clamp(value, 1, 1000); } }

        [IgnoreMember]
        public double WindowMovementSpeed { get { return _windowMovementSpeed; } set { _windowMovementSpeed = Math.Clamp(value, 0.01, 1); } }

        [IgnoreMember]
        private static readonly string _filename = "./settings.ini";

        [IgnoreMember]
        private static Config _instance;

        [IgnoreMember]
        public static Config GetInstance
        {
            get
            {
                return _instance ??= new Config();
            }
        }

        static Config()
        {
            _instance = new Config();
        }

        public Config()
        {
            this.MemberNotNull();
        }

        public static bool SaveToDisk()
        {
            try
            {
                var serialziedObject = TinyhandSerializer.Serialize<Config>(_instance);
                if (serialziedObject is null) return false;

                using (FileStream fs = new(_filename, FileMode.Create))
                {
                    fs.Write(serialziedObject);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ReadFromDisk()
        {
            try
            {
                using (FileStream fs = new(_filename, FileMode.Open))
                {
                    byte[] rawData = new byte[fs.Length];
                    var nBytes = fs.Read(rawData, 0, (int)fs.Length);
                    if (nBytes == 0) return false;
                    var deserializedObject = TinyhandSerializer.Deserialize<Config>(rawData);
                    if (deserializedObject is null) return false;
                    _instance = deserializedObject;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}