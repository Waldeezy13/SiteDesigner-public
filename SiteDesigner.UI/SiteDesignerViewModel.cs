using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SiteDesigner.Core;

namespace SiteDesigner.UI
{
    public class SiteDesignerViewModel : INotifyPropertyChanged
    {
        double _front, _side, _rear, _stallW, _stallD, _aisleW; int _target;

        public double FrontSetback { get => _front; set { _front = value; OnPropertyChanged(); } }
        public double SideSetback { get => _side; set { _side = value; OnPropertyChanged(); } }
        public double RearSetback { get => _rear; set { _rear = value; OnPropertyChanged(); } }
        public double StallWidth { get => _stallW; set { _stallW = value; OnPropertyChanged(); } }
        public double StallDepth { get => _stallD; set { _stallD = value; OnPropertyChanged(); } }
        public double AisleWidth { get => _aisleW; set { _aisleW = value; OnPropertyChanged(); } }
        public int TargetStalls { get => _target; set { _target = value; OnPropertyChanged(); } }

        // The plugin assigns these so the buttons can trigger plugin actions:
        public Action? ApplyAction { get; set; }
        public Action? PlaceTestLayoutAction { get; set; }
        public Action? PickBoundaryAction { get; set; }
        public Action? DrawSetbackAction { get; set; }

        public ICommand ApplyCommand { get; }
        public ICommand PlaceTestLayoutCommand { get; }
        public ICommand PickBoundaryCommand { get; }
        public ICommand DrawSetbackCommand { get; }

        public SiteDesignerViewModel()
        {
            ApplyCommand = new RelayCommand(_ => ApplyAction?.Invoke());
            PlaceTestLayoutCommand = new RelayCommand(_ => PlaceTestLayoutAction?.Invoke());
            PickBoundaryCommand = new RelayCommand(_ => PickBoundaryAction?.Invoke());
            DrawSetbackCommand = new RelayCommand(_ => DrawSetbackAction?.Invoke());
        }

        public void LoadFrom(SiteConfig c)
        {
            FrontSetback = c.SetbackFront;
            SideSetback = c.SetbackSide;
            RearSetback = c.SetbackRear;
            StallWidth = c.StallWidth;
            StallDepth = c.StallDepth;
            AisleWidth = c.AisleWidth;
            TargetStalls = c.TargetStalls;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Lightweight ICommand helper
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        { _execute = execute; _canExecute = canExecute; }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
