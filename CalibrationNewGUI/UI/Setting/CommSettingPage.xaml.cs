using CalibrationNewGUI.ViewModel.Setting;
using System.Windows.Controls;

namespace CalibrationNewGUI.UI.Setting
{
    /// <summary>
    /// CommSettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommSettingPage : Page
    {
        public CommSettingPage()
        {
            InitializeComponent();
            DataContext = new CommSettingVM();
        }
    }
}
