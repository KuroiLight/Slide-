using Polenter.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SlideSharp
{
    public struct Config
    {
        public int HIDDEN_OFFSET
        {
            get
            {
                return HIDDEN_OFFSET;
            }
            set
            {
                HIDDEN_OFFSET = Math.Clamp(value, 1, 1000);
            }
        }
        public int MMDRAG_DEADZONE
        {
            get
            {
                return MMDRAG_DEADZONE;

            }
            set
            {
                MMDRAG_DEADZONE = Math.Clamp(value, 1, 1000);
            }
        }
        public double WINDOW_ANIM_SPEED
        {
            get
            {
                return WINDOW_ANIM_SPEED;
            }
            set
            {
                WINDOW_ANIM_SPEED = Math.Clamp(value, 0.001, 1);
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