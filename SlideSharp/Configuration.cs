using Polenter.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SlideSharp
{
    public struct Config
    {
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
        public static Config Config;
        private static string filename;

        static Configuration()
        {
            filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\settings.xml";
        }
        public static void LoadDefaults()
        {
            Configuration.Config = new Config()
            {
                HIDDEN_OFFSET = (int)(WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width / 75),
                MMDRAG_DEADZONE = (int)((WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width / 100) * 15),
                WINDOW_ANIM_SPEED = 0.025,
            };
        }

        public static void Save()
        {
            try {
                SharpSerializer serializer = new SharpSerializer();
                serializer.Serialize(Config, "./settings.xml");
            } catch {
                MessageBox.Show("Config couldn't be saved.");
            }
        }

        public static void Load()
        {
            if(!File.Exists(filename)) {
                LoadDefaults();
            } else {
try {
                SharpSerializer serializer = new SharpSerializer();
                Config = (Config)serializer.Deserialize("./settings.xml");
            } catch {
                    MessageBox.Show("Config couldn't be loaded.");
            }
            }
            
        }
    }
}