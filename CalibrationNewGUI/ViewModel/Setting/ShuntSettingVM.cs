using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class ShuntSettingVM
    {
        public ShuntInfo ShuntInfos { get; set; }

        public RelayCommand SaveClick { get; set; }
        public RelayCommand<TextCompositionEventArgs> PositiveNumChecker { get; set; }
        public RelayCommand<TextChangedEventArgs> ActiveNumChecker { get; set; }

        public ShuntSettingVM()
        {
            ShuntInfos = ShuntInfo.GetObj();
            if (ShuntInfos.CorrectionMode == true)
                ShuntInfos.shuntReg = ShuntInfos.ShuntStandardCurr / ShuntInfos.ShuntNewCurr; //첫 실행 할때 션트 보정치를 계산
            else
            {
                if (ShuntInfos.ShuntNewReg == 0)
                {
                    ShuntInfos.shuntReg = 1;
                }
                else
                {
                    ShuntInfos.shuntReg = (1 / (ShuntInfos.ShuntNewReg * 0.001f)) * 1000;
                }
            }
                
            SaveClick = new RelayCommand(DataSave);

            PositiveNumChecker = new RelayCommand<TextCompositionEventArgs>(PositiveNumCheck);
            ActiveNumChecker = new RelayCommand<TextChangedEventArgs>(ActiveNumCheck);
        }

        private void DataSave()
        {
            ShuntInfos.Save();
            if (ShuntInfos.CorrectionMode == true)
            {
                if (ShuntInfos.ShuntStandardCurr == 0 || ShuntInfos.ShuntNewCurr == 0)
                    ShuntInfos.shuntReg = 1 * 1000;
                else
                    ShuntInfos.shuntReg = ShuntInfos.ShuntStandardCurr / ShuntInfos.ShuntNewCurr * 1000; //첫 실행 할때 션트 보정치를 계산
            }
            else
            {
                if (ShuntInfos.ShuntNewReg == 0)
                    ShuntInfos.shuntReg = 1;
                else
                    ShuntInfos.shuntReg = (1 / (ShuntInfos.ShuntNewReg * 0.001f)) * 1000;
            }
            MessageBox.Show(App.GetString("SaveOkMsg"));
        }

        private void PositiveNumCheck(TextCompositionEventArgs e)
        {
            //string positiveNum = @"^(-?[0-9]+)?$";            // 정수 판단용 정규식
            //string activeNum   = @"^(-?[0-9]+(.?[0-9]+)?)?$"; // 정수 및 실수 판단용 정규식
            
            Regex regex = new Regex(@"^(-?[0-9]*)?$"); // 정수 및 실수 판단용 정규식

            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ActiveNumCheck(TextChangedEventArgs e)
        {
            //string positiveNum = @"^(-?[0-9]+)?$";            // 정수 판단용 정규식
            //string activeNum   = @"^(-?[0-9]+(.?[0-9]+)?)?$"; // 정수 및 실수 판단용 정규식

            string inputText = ((TextBox)e.Source).Text;
            Regex regex = new Regex(@"^(-?[0-9]+)?$"); // 정수 및 실수 판단용 정규식
            //Regex regex = new Regex(@"^(-?[0-9]+(.?[0-9]+)?)+$"); // 정수 및 실수 판단용 정규식

            e.Handled = !regex.IsMatch(inputText);
        }
    }
}
