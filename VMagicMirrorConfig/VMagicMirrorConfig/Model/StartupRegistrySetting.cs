using System;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    class StartupRegistrySetting
    {
        private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ProductName = @"Baku.VMagicMirror";

        private string UnityAppPath => SpecialFilePath.UnityAppPath;

        public bool CheckThisVersionRegistered()
        {
            try
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false))
                {
                    return (
                        regKey.GetValue(ProductName, "") is string s &&
                        s == UnityAppPath
                        );
                }
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool CheckOtherVersionRegistered()
        {
            try
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false))
                {
                    return (
                        regKey.GetValue(ProductName, UnityAppPath) is string s &&
                        s != UnityAppPath
                        );
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetThisVersionRegister(bool enable)
        {
            try
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true))
                {
                    if (enable)
                    {
                        regKey.SetValue(ProductName, UnityAppPath);
                    }
                    else
                    {
                        regKey.DeleteValue(ProductName);
                    }
                }
            }
            catch (Exception)
            {
                //諦める
            }
        }
    }
}
