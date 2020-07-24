using CalibrationNewGUI.ViewModel.Setting;
using System.Windows.Controls;

namespace CalibrationNewGUI.UI.Setting
{
    /// <summary>
    /// AutoSaveSettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AutoSaveSettingPage : Page
    {
        public AutoSaveSettingPage()
        {
            InitializeComponent();
            DataContext = new AutoSaveSettingVM();
        }
    }
}
