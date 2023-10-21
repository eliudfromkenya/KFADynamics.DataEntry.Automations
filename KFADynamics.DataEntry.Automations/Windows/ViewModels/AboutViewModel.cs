﻿using KFADynamics.DataEntry.Automations.ViewModels;

namespace KFADynamics.DataEntry.Automations.Windows.ViewModels
{
    public sealed class AboutViewModel : ViewModelBase
    {
        private bool _darkTheme;
        public bool DarkTheme
        {
            get { return _darkTheme; }
            set
            {
                _darkTheme = value;
                OnPropertyChanged(nameof(DarkTheme));
            }
        }

        public AboutViewModel(bool darkTheme)
        {
            DarkTheme = darkTheme;
        }
    }
}