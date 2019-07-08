﻿using System;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class SettingWindow : Window
    {
        public SettingWindow() => InitializeComponent();

        private static SettingWindow _settingWindow = null;

        public static void OpenOrActivateExistingWindow(object dataContext)
        {
            if (_settingWindow == null)
            {
                _settingWindow = new SettingWindow()
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    DataContext = dataContext,
                };
                _settingWindow.Closed += OnSettingWindowClosed;
                _settingWindow.Show();
            }
            else
            {
                _settingWindow.Activate();
            }
        }

        private static void OnSettingWindowClosed(object sender, EventArgs e)
        {
            if (_settingWindow != null)
            {
                _settingWindow.Closed -= OnSettingWindowClosed;
                _settingWindow = null;
            }
        }
    }
}