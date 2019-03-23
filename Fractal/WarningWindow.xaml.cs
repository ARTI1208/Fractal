using System;
using System.Windows;

namespace Fractal
{
    public partial class WarningWindow
    {
        
        public static bool DoNotShowAgain;
        private readonly uint _mode;
        private static bool _construct;
        public const int ChangeDepthTo2 = 0;
        public const int LeaveOldDepthLevel = 1;

        public WarningWindow()
        {
            InitializeComponent();
            WarningLabel.Content = "АХТУНГ!!! WARNING!!!\nBy clicking OK button you agree that this app may do job for a long time" +
                                   " or do not work at all.\nIf you click Cancel, Fractal won't be constructed/updated.";
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            AgainCheckBox.ToolTip = "Current choice will be applied for all further requests";
        }

        private WarningWindow(uint type)
        {
            _mode = type;
            InitializeComponent();
            AgainCheckBox.Visibility = Visibility.Collapsed;
            WarningLabel.Content = "/*\n* Your warranty is now void.\n*\n* I am not responsible for bricked devices, " +
                                   "dead SD cards, thermonuclear war, \n* or you getting fired because the alarm app " +
                                   "failed.\n* YOU are choosing to make these modifications, and if\n* you point the finger " +
                                   "at me for messing up your device, I will laugh at you.\n*/";
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if (_mode == 0)
            {
                if (AgainCheckBox.IsChecked == true)
                    DoNotShowAgain = true;
                WarningWindow warranty = new WarningWindow(1);
                Hide();
                warranty.ShowDialog();
            }
            else
            {
                _construct = true;
                Hide();
            }
        }
        
        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (AgainCheckBox.IsChecked == true)
                DoNotShowAgain = true;
            _construct = false;
            Hide();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _construct = false;
        }
        
        public static bool ShouldConstruct()
        {
            return _construct;
        }
    }
}
