using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Hangups.Core
{
    public static class SettingsHelper
    {
        public static string GetStringSetting(string key){
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            Object retrievedValue = roamingSettings.Values[key];
            return retrievedValue == null ? String.Empty : (string)retrievedValue;
        }

        public static bool GetBooleanSetting(string key){
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            Object retrievedValue = roamingSettings.Values[key];
            return retrievedValue == null ? false : bool.Parse((string)retrievedValue);
        }

        public static void RemoveSetting(string key) {
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values.Remove(key);
        }

        public static void StoreSetting(string key, string value){
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values[key] = value;
        }

        public static void StoreSetting(string key, bool value){
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values[key] = value.ToString();
        }




    }
}
