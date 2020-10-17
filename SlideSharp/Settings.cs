using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SlideSharp
{
    public struct Settings
    {
        [DataMember]
        public int Update_Interval { get; set; }
        [DataMember]
        public int Window_Offscreen_Offset { get; set; }
        [DataMember]
        public int Middle_Button_DeadZone { get; set; }
        [DataMember]
        public int Window_Movement_Rate { get; set; }

        public static Settings SettingsInstance = Settings.Load();
        public static readonly string SettingsPath = "./Settings.json";

        public static Settings Defaults()
        {
            return new Settings() { Middle_Button_DeadZone = 25, Update_Interval = 16, Window_Movement_Rate = 4, Window_Offscreen_Offset = 30 };
        }

        public static void Save()
        {
            var jsonSettings = JsonConvert.SerializeObject(SettingsInstance, Formatting.Indented);
            if (jsonSettings.Length > 0) {
                try {
                    File.WriteAllText(SettingsPath, jsonSettings);
                } catch (Exception e) {
                    throw e;
                }
            } else {
                throw new Newtonsoft.Json.JsonException();
            }
        }

        public static Settings Load()
        {
            if (File.Exists(SettingsPath)) {
                return (Settings)JsonConvert.DeserializeObject(File.ReadAllText(SettingsPath));
            } else {
                return Defaults();
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(SettingsInstance);
        }
    }
}
