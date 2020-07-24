using CalibrationNewGUI.ViewModel.Setting;
using System.Windows.Controls;

namespace CalibrationNewGUI.UI.Setting
{
    /// <summary>
    /// OthersSettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OthersSettingPage : Page
    {
        public OthersSettingPage()
        {
            InitializeComponent();
            DataContext = new OthersSettingVM();
        }
    }
}
