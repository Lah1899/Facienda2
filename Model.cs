using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Facienda2
{
    public class Root
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public Settings Settings { get; set; }
    }

    public class TaskItem : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private bool _isDone;
        private DateTime? _dueDate;
        private int _positionX;
        private int _positionY;

        public string Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsDone
        {
            get => _isDone;
            set
            {
                if (_isDone == value) return;
                _isDone = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                if (_dueDate == value) return;
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public int PositionX
        {
            get => _positionX;
            set
            {
                if (_positionX == value) return;
                _positionX = value;
                OnPropertyChanged();
            }
        }

        public int PositionY
        {
            get => _positionY;
            set
            {
                if (_positionY == value) return;
                _positionY = value;
                OnPropertyChanged();
            }
        }

        //この辺、何のために入れたんだっけ？
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Settings
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
