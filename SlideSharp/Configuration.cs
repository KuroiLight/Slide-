using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Screen_Drop_In;

namespace SlideSharp
{
    public struct Config
    {
        [JsonInclude]
        private int _HIDDEN_OFFSET;
        public int HIDDEN_OFFSET
        {
            get
            {
                return _HIDDEN_OFFSET;
            }
            set
            {
                _HIDDEN_OFFSET = Math.Clamp(value, 1, 1000);
            }
        }
        [JsonInclude]
        private int _MMDRAG_DEADZONE;
        public int MMDRAG_DEADZONE
        {
            get
            {
                return _MMDRAG_DEADZONE;
            }
            set
            {
                _MMDRAG_DEADZONE = Math.Clamp(value, 1, 1000);
            }
        }
        [JsonInclude]
        private double _WINDOW_ANIM_SPEED;
        public double WINDOW_ANIM_SPEED
        {
            get
            {
                return _WINDOW_ANIM_SPEED;
            }
            set
            {
                _WINDOW_ANIM_SPEED = Math.Clamp(value, 0.001, 1);
            }
        }
    }

    public static class Configuration
    {
        public static Config Config { get; set; }
        private static readonly string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\settings.xml";

        public static void LoadDefaults()
        {
            Configuration.Config = new Config()
            {
                HIDDEN_OFFSET = (int)(Screen.PrimaryScreen.Bounds.Width / 75),
                MMDRAG_DEADZONE = (int)((Screen.PrimaryScreen.Bounds.Width / 100) * 15),
                WINDOW_ANIM_SPEED = 0.025,
            };
        }

        public static void Save()
        {
            try {
                var serialized = JsonSerializer.Serialize((object)Config, Config.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.General) { IncludeFields = true });
                File.WriteAllText(filename, serialized);
            } catch {
                MessageBox.Show("Config couldn't be saved.");
            }
        }

        public static void Load()
        {
            if (!File.Exists(filename)) {
                LoadDefaults();
            } else {
                try {
                    var serialized = File.ReadAllText(filename);
                    Config? unserialized = (Config?)JsonSerializer.Deserialize(serialized, Config.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.General) { IncludeFields = true });
                    if (unserialized is not null) {
                        Config = ((Config)unserialized)!;
                    } else {
                        LoadDefaults();
                    }
                } catch {
                    MessageBox.Show("Config couldn't be loaded.");
                }
            }
        }
    }
}