using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zac_s_Minecraft_Patcher.Core;

namespace Zac_s_Minecraft_Patcher.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand DayCommand { get; set; }

        public DayViewModel DayVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            DayVM = new DayViewModel();
            CurrentView = DayVM;

            DayCommand = new RelayCommand(o =>
            {
                CurrentView = DayVM;
            });

        }
    }
}
