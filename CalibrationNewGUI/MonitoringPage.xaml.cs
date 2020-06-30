using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CalibrationNewGUI
{
    /// <summary>
    /// MonitoringPage.xaml에 대한 상호 작용 논리
    /// </summary>
    [ImplementPropertyChanged]
    public partial class MonitoringPage : Page
    {
        public SettingData AllSetData { get; set; }
        //public DataTable VoltageCalTable { get; set; }  //CAL 전압 CalPoint입력용
        //public DataTable CurrentCalTable { get; set; }  //CAL 전류 CalPoint입력용
        //public DataTable VoltageMeaTable { get; set; }  //실측 전압 CalPoint입력용
        //public DataTable CurrentMeaTable { get; set; }  //실측 전류 CalPoint입력용
        public MonitoringPage()
        {
            InitializeComponent();
            DataContext = SettingData.GetObj();
        }
        private void MonitoringPageLoaded(object sender, RoutedEventArgs e)
        {
            AllSetData = SettingData.GetObj();
            AllSetData.VoltageCalTable = new DataTable();
            AllSetData.CurrentCalTable = new DataTable();
            AllSetData.VoltageMeaTable = new DataTable();
            AllSetData.CurrentMeaTable = new DataTable();

            AllSetData.VoltageCalTable.Columns.Add("CALNO");
            AllSetData.VoltageCalTable.Columns.Add("CALSetVolt");
            AllSetData.VoltageCalTable.Columns.Add("CALSetCurr");
            AllSetData.VoltageCalTable.Columns.Add("CALOutVolt");
            AllSetData.VoltageCalTable.Columns.Add("CALOutCurr");
            AllSetData.VoltageCalTable.Columns.Add("CALDMMOut");

            AllSetData.CurrentCalTable.Columns.Add("CALNO");
            AllSetData.CurrentCalTable.Columns.Add("CALSetVolt");
            AllSetData.CurrentCalTable.Columns.Add("CALSetCurr");
            AllSetData.CurrentCalTable.Columns.Add("CALOutVolt");
            AllSetData.CurrentCalTable.Columns.Add("CALOutCurr");
            AllSetData.CurrentCalTable.Columns.Add("CALDMMOut");

            AllSetData.VoltageMeaTable.Columns.Add("MEANO");
            AllSetData.VoltageMeaTable.Columns.Add("MEASetVolt");
            AllSetData.VoltageMeaTable.Columns.Add("MEASetCurr");
            AllSetData.VoltageMeaTable.Columns.Add("MEAOutVolt");
            AllSetData.VoltageMeaTable.Columns.Add("MEAOutCurr");
            AllSetData.VoltageMeaTable.Columns.Add("MEADMMOut");

            AllSetData.CurrentMeaTable.Columns.Add("MEANO");
            AllSetData.CurrentMeaTable.Columns.Add("MEASetVolt");
            AllSetData.CurrentMeaTable.Columns.Add("MEASetCurr");
            AllSetData.CurrentMeaTable.Columns.Add("MEAOutVolt");
            AllSetData.CurrentMeaTable.Columns.Add("MEAOutCurr");
            AllSetData.CurrentMeaTable.Columns.Add("MEADMMOut");

            VoltageCurrentChange(AllSetData.VoltCurrSelect); //초기 값에 의한 텍스트, 변수 변경
            //CAL 자동 버튼구역으로 초기화
            AutoCalPanel.Visibility = Visibility.Visible;
            ActiveCalPanel.Visibility = Visibility.Collapsed;
        }
        /// CAL 할때 자동, 수동 선택에 따른 UI 변화
        private void AutoCalBtn_Checked(object sender, RoutedEventArgs e)
        {
            AutoCalPanel.Visibility = Visibility.Visible;
            ActiveCalPanel.Visibility = Visibility.Collapsed;
        }

        private void ActCalBtn_Checked(object sender, RoutedEventArgs e)
        {
            AutoCalPanel.Visibility = Visibility.Collapsed;
            ActiveCalPanel.Visibility = Visibility.Visible;
        }

        /// 전압, 전류 선택에 따른 UI 변화 및 데이터 그리드 변화할것
        private void VoltageBtn_Checked(object sender, RoutedEventArgs e)
        {
            AllSetData.VoltCurrSelect = 0;
            VoltageCurrentChange(AllSetData.VoltCurrSelect);
        }
        private void CurrentBtn_Checked(object sender, RoutedEventArgs e)
        {
            AllSetData.VoltCurrSelect = 1;
            VoltageCurrentChange(AllSetData.VoltCurrSelect);
        }
        //채널 선택에 따른 UI변화
        //채널선택 - 1
        private void CHSelectCH1Btn_Checked(object sender, RoutedEventArgs e)
        {
            AllSetData.ChannelSelect = 1;
            VoltageCurrentChange(AllSetData.VoltCurrSelect);
        }
        //채널선택 - 2
        private void CHSelectCH2Btn_Checked(object sender, RoutedEventArgs e)
        {
            AllSetData.ChannelSelect = 2;
            VoltageCurrentChange(AllSetData.VoltCurrSelect);
        }
        //채널선택 - 3
        private void CHSelectCH3Btn_Checked(object sender, RoutedEventArgs e)
        {
            AllSetData.ChannelSelect = 3;
            VoltageCurrentChange(AllSetData.VoltCurrSelect);
        }

        //전압 전류 선택에 따른 텍스트 변경 필요
        private void VoltageCurrentChange(int VoltCurrSelect)
        {
            if (VoltCurrSelect == 0)//전압
            {
                DMMOutputText.Text = "Volt(mV)"; //DMM 실측 텍스트 블럭
                SelectVoltageCurrunt.Text = "Volt"; //CAL 수동 출력 텍스트 블럭
                AllSetData.CalErrRange = AllSetData.CalErrRangeVolt; //Cal에러 레인지
                CalErrorRangeUnit.Text = "mV";//에러 레인지 단위
                AllSetData.MeaErrRange = AllSetData.MeaErrRangeVolt; //Mea에러 레인지
                MeaErrorRangeUnit.Text = "mV";//에러 레인지 단위
                //채널표시를 출력
                AllSetData.CHSelectString = "Volt";
                if (AllSetData.ChannelSelect == 1) AllSetData.CHSelectString = "CH1 " + AllSetData.CHSelectString;
                else if (AllSetData.ChannelSelect == 2) AllSetData.CHSelectString = "CH2 " + AllSetData.CHSelectString;
                else if (AllSetData.ChannelSelect == 3) AllSetData.CHSelectString = "DUAL " + AllSetData.CHSelectString;
                //전압, 전류에 맞는 데이터 테이블 출력
                CalDataGrid.ItemsSource = AllSetData.VoltageCalTable.DefaultView;
                MeaDataGrid.ItemsSource = AllSetData.VoltageMeaTable.DefaultView;
            }
            else if (VoltCurrSelect == 1)//전류
            {
                DMMOutputText.Text = "Curr(mA)"; //DMM 실측 텍스트 블럭
                SelectVoltageCurrunt.Text = "Curr"; //CAL 수동 출력 텍스트 블럭
                AllSetData.CalErrRange = AllSetData.CalErrRangeCurr; //Cal에러 레인지
                CalErrorRangeUnit.Text = "mA";//에러 레인지 단위
                AllSetData.MeaErrRange = AllSetData.MeaErrRangeCurr; //Mea에러 레인지
                MeaErrorRangeUnit.Text = "mA";//에러 레인지 단위
                //채널표시를 출력
                AllSetData.CHSelectString = "Curr";
                if (AllSetData.ChannelSelect == 1) AllSetData.CHSelectString = "CH1 " + AllSetData.CHSelectString;
                else if (AllSetData.ChannelSelect == 2) AllSetData.CHSelectString = "CH2 " + AllSetData.CHSelectString;
                else if (AllSetData.ChannelSelect == 3) AllSetData.CHSelectString = "DUAL " + AllSetData.CHSelectString;
                //전압, 전류에 맞는 데이터 테이블 출력
                CalDataGrid.ItemsSource = AllSetData.CurrentCalTable.DefaultView;
                MeaDataGrid.ItemsSource = AllSetData.CurrentMeaTable.DefaultView;
            }
            //AutoSave 변화
            if (AllSetData.AutoSaveFlag == 1)
            {
                AutoSaveTextBlock.Text = "ON";
                AutoSaveCheckBox.Background = Brushes.GreenYellow;
            }
            else
            {
                AutoSaveTextBlock.Text = "OFF";
                AutoSaveCheckBox.Background = Brushes.Yellow;
            }
        }
        //Cal Point 추가버튼
        private void AddCalDataGridBtn_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(CalDataGrid);
            if (AllSetData.VoltCurrSelect == 0)
            {
                AllSetData.VoltageCalTable.Rows.Add(new String[] { (AllSetData.VoltageCalTable.Rows.Count + 1).ToString(), AllSetData.InputRangeVoltMin.ToString(), "0", "", "", "" });
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
            else if (AllSetData.VoltCurrSelect == 1)
            {
                AllSetData.CurrentCalTable.Rows.Add(new String[] { (AllSetData.CurrentCalTable.Rows.Count + 1).ToString(), AllSetData.InputRangeVoltMin.ToString(), "0", "", "", "" });
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
        //Cal Point 삭제버튼
        private void DelCalDataGridBtn_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(CalDataGrid);
            if (AllSetData.VoltCurrSelect == 0)
            {
                if (AllSetData.VoltageCalTable.Rows.Count > 0) AllSetData.VoltageCalTable.Rows.RemoveAt(AllSetData.VoltageCalTable.Rows.Count - 1);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
            else if (AllSetData.VoltCurrSelect == 1)
            {
                if (AllSetData.CurrentCalTable.Rows.Count > 0) AllSetData.CurrentCalTable.Rows.RemoveAt(AllSetData.CurrentCalTable.Rows.Count - 1);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
        //실측 Point 추가버튼
        private void AddMeaDataGridBtn_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(MeaDataGrid);
            if (AllSetData.VoltCurrSelect == 0)
            {
                AllSetData.VoltageMeaTable.Rows.Add(new String[] { (AllSetData.VoltageMeaTable.Rows.Count + 1).ToString(), AllSetData.InputRangeVoltMin.ToString(), "0", "", "", "" });
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
            else if (AllSetData.VoltCurrSelect == 1)
            {
                AllSetData.CurrentMeaTable.Rows.Add(new String[] { (AllSetData.CurrentMeaTable.Rows.Count + 1).ToString(), AllSetData.InputRangeVoltMin.ToString(), "0", "", "", "" });
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
        //실측 Point 삭제버튼
        private void DelMeaDataGridBtn_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(MeaDataGrid);
            if (AllSetData.VoltCurrSelect == 0)
            {
                if (AllSetData.VoltageMeaTable.Rows.Count > 0) AllSetData.VoltageMeaTable.Rows.RemoveAt(AllSetData.VoltageMeaTable.Rows.Count - 1);
                MeaDataGrid.ItemsSource = AllSetData.VoltageMeaTable.DefaultView;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
            else if (AllSetData.VoltCurrSelect == 1)
            {
                if (AllSetData.CurrentMeaTable.Rows.Count > 0) AllSetData.CurrentMeaTable.Rows.RemoveAt(AllSetData.CurrentMeaTable.Rows.Count - 1);
                MeaDataGrid.ItemsSource = AllSetData.CurrentMeaTable.DefaultView;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
        //스크롤 뷰어 포커스용 함수
        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        //Cal 데이터 그리드 편집 시 나오는 이벤트

        //Cal Point 입력시 예외처리
        private void CalDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string selectedVolt = "";
            string selectedCurr = "";
            for (int i = 0; i < CalDataGrid.Items.Count; i++)
            {
                if (AllSetData.VoltCurrSelect == 0)
                {
                    selectedVolt = AllSetData.VoltageCalTable.Rows[i][1].ToString();//전압
                    selectedCurr = AllSetData.VoltageCalTable.Rows[i][2].ToString();//전류
                }
                else if (AllSetData.VoltCurrSelect == 1)
                {
                    selectedVolt = AllSetData.CurrentCalTable.Rows[i][1].ToString();
                    selectedCurr = AllSetData.CurrentCalTable.Rows[i][2].ToString();
                }
                if (selectedVolt != "")
                {
                    try
                    {
                        Convert.ToInt32(selectedVolt);
                        if (Convert.ToInt32(selectedVolt) < AllSetData.InputRangeVoltMin || Convert.ToInt32(selectedVolt) > AllSetData.InputRangeVoltMax)
                        {
                            string msg = "전압은 " + AllSetData.InputRangeVoltMin.ToString() + "에서 " + AllSetData.InputRangeVoltMax.ToString() + "사이 정수값으로 입력하세요.";
                            System.Windows.MessageBox.Show(msg);
                            if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageCalTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                            else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentCalTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = "전압은 " + AllSetData.InputRangeVoltMin.ToString() + "에서 " + AllSetData.InputRangeVoltMax.ToString() + "사이 정수값으로 입력하세요.";
                        System.Windows.MessageBox.Show(msg);
                        if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageCalTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                        else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentCalTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                    }
                }
                
                if (selectedCurr != "")
                {
                    try
                    {
                        Convert.ToInt32(selectedCurr);
                        if (Convert.ToInt32(selectedCurr) < AllSetData.InputRangeCurrentMin || Convert.ToInt32(selectedCurr) > AllSetData.InputRangeCurrentMax)
                        {
                            string msg = "전류는 " + AllSetData.InputRangeCurrentMin.ToString() + "에서 " + AllSetData.InputRangeCurrentMax.ToString() + "사이 정수값으로 입력하세요.";
                            System.Windows.MessageBox.Show(msg);
                            if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageCalTable.Rows[i][2] = "0";
                            else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentCalTable.Rows[i][2] = "0";
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = "전류는 " + AllSetData.InputRangeCurrentMin.ToString() + "에서 " + AllSetData.InputRangeCurrentMax.ToString() + "사이 정수값으로 입력하세요.";
                        System.Windows.MessageBox.Show(msg);
                        if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageCalTable.Rows[i][2] = "0";
                        else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentCalTable.Rows[i][2] = "0";
                    }
                }
            }
        }
        //실측 포인트 입력시 예외처리
        private void MeaDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string selectedVolt = "";
            string selectedCurr = "";
            for (int i = 0; i < MeaDataGrid.Items.Count; i++)
            {
                if (AllSetData.VoltCurrSelect == 0)
                {
                    selectedVolt = AllSetData.VoltageMeaTable.Rows[i][1].ToString();//전압
                    selectedCurr = AllSetData.VoltageMeaTable.Rows[i][2].ToString();//전류
                }
                else if (AllSetData.VoltCurrSelect == 1)
                {
                    selectedVolt = AllSetData.CurrentMeaTable.Rows[i][1].ToString();
                    selectedCurr = AllSetData.CurrentMeaTable.Rows[i][2].ToString();
                }
                if (selectedVolt != "")
                {
                    try
                    {
                        Convert.ToInt32(selectedVolt);
                        if (Convert.ToInt32(selectedVolt) < AllSetData.InputRangeVoltMin || Convert.ToInt32(selectedVolt) > AllSetData.InputRangeVoltMax)
                        {
                            string msg = "전압은 " + AllSetData.InputRangeVoltMin.ToString() + "에서 " + AllSetData.InputRangeVoltMax.ToString() + "사이 정수값으로 입력하세요.";
                            System.Windows.MessageBox.Show(msg);
                            if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageMeaTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                            else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentMeaTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = "전압은 " + AllSetData.InputRangeVoltMin.ToString() + "에서 " + AllSetData.InputRangeVoltMax.ToString() + "사이 정수값으로 입력하세요.";
                        System.Windows.MessageBox.Show(msg);
                        if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageMeaTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                        else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentMeaTable.Rows[i][1] = AllSetData.InputRangeVoltMin.ToString();
                    }
                }

                if (selectedCurr != "")
                {
                    try
                    {
                        Convert.ToInt32(selectedCurr);
                        if (Convert.ToInt32(selectedCurr) < AllSetData.InputRangeCurrentMin || Convert.ToInt32(selectedCurr) > AllSetData.InputRangeCurrentMax)
                        {
                            string msg = "전류는 " + AllSetData.InputRangeCurrentMin.ToString() + "에서 " + AllSetData.InputRangeCurrentMax.ToString() + "사이 정수값으로 입력하세요.";
                            System.Windows.MessageBox.Show(msg);
                            if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageMeaTable.Rows[i][2] = "0";
                            else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentMeaTable.Rows[i][2] = "0";
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = "전류는 " + AllSetData.InputRangeCurrentMin.ToString() + "에서 " + AllSetData.InputRangeCurrentMax.ToString() + "사이 정수값으로 입력하세요.";
                        System.Windows.MessageBox.Show(msg);
                        if (AllSetData.VoltCurrSelect == 0) AllSetData.VoltageMeaTable.Rows[i][2] = "0";
                        else if (AllSetData.VoltCurrSelect == 1) AllSetData.CurrentMeaTable.Rows[i][2] = "0";
                    }
                }
            }
        }
        //수동Cal출력 시작
        private void OutputCalStartBtn_Click(object sender, RoutedEventArgs e)
        {
            AllSetData.ActCalPointArray = new int[2];
            string selectedVolt = "";
            string selectedCurr = "";
            
            if (AllSetData.MCUConnectFlag == 0)//MCU포트 연결 확인
            {
                System.Windows.MessageBox.Show("MCU 포트를 확인하세요.");
                return;
            }
            if (AllSetData.DMMConnectFlag == 0)//DMM포트 연결 확인
            {
                System.Windows.MessageBox.Show("DMM 포트를 확인하세요.");
                return;
            }

            selectedVolt = CalInputVoltTextBox.Text;//전압
            selectedCurr = CalInputCurrTextBox.Text;//전류
            try
            {
                Convert.ToInt32(selectedVolt);
                if (Convert.ToInt32(selectedVolt) < AllSetData.InputRangeVoltMin || Convert.ToInt32(selectedVolt) > AllSetData.InputRangeVoltMax)
                {
                    string msg = "전압은 " + AllSetData.InputRangeVoltMin.ToString() + "에서 " + AllSetData.InputRangeVoltMax.ToString() + "사이 정수값으로 입력하세요.";
                    System.Windows.MessageBox.Show(msg);
                    CalInputVoltTextBox.Text = AllSetData.InputRangeVoltMin.ToString();
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = "전압은 " + AllSetData.InputRangeVoltMin.ToString() + "에서 " + AllSetData.InputRangeVoltMax.ToString() + "사이 정수값으로 입력하세요.";
                System.Windows.MessageBox.Show(msg);
                CalInputVoltTextBox.Text = AllSetData.InputRangeVoltMin.ToString();
                return;
            }

            try
            {
                Convert.ToInt32(selectedCurr);
                if (Convert.ToInt32(selectedCurr) < AllSetData.InputRangeCurrentMin || Convert.ToInt32(selectedCurr) > AllSetData.InputRangeCurrentMax)
                {
                    string msg = "전류는 " + AllSetData.InputRangeCurrentMin.ToString() + "에서 " + AllSetData.InputRangeCurrentMax.ToString() + "사이 정수값으로 입력하세요.";
                    System.Windows.MessageBox.Show(msg);
                    CalInputCurrTextBox.Text = "0";
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = "전류는 " + AllSetData.InputRangeCurrentMin.ToString() + "에서 " + AllSetData.InputRangeCurrentMax.ToString() + "사이 정수값으로 입력하세요.";
                System.Windows.MessageBox.Show(msg);
                CalInputCurrTextBox.Text = "0";
                return;
            }
            if (AllSetData.AutoCalOutStartFlag == 1 || AllSetData.MeaOutStartFlag == 1)
            {
                System.Windows.MessageBox.Show("출력 실행 중입니다.");
                return;
            }
            AllSetData.ActCalPointArray[0] = Convert.ToInt32(selectedVolt);//전압
            AllSetData.ActCalPointArray[1] = Convert.ToInt32(selectedCurr);//전류

            AllSetData.CalOutStartFlag = 1;
        }
        //수동Cal 실시
        private void CalStartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllSetData.MCUConnectFlag == 0)//MCU포트 연결 확인
            {
                System.Windows.MessageBox.Show("MCU 포트를 확인하세요.");
                return;
            }
            if (AllSetData.DMMConnectFlag == 0)//DMM포트 연결 확인
            {
                System.Windows.MessageBox.Show("DMM 포트를 확인하세요.");
                return;
            }
            AllSetData.CalOutRealStartFlag = 1;
        }
        //수동 Cal 출력 종료
        private void CalEndBtn_Click(object sender, RoutedEventArgs e)
        {
            AllSetData.CalOutEndFlag = 1;
        }
        //자동 Cal 출력 시작
        private void AutoCalStartBtn_Click(object sender, RoutedEventArgs e)
        {
            //예외처리
            if (AllSetData.MCUConnectFlag == 1)//MCU포트 연결 확인
            {
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    if (AllSetData.VoltageCalTable.Rows.Count < 1 || AllSetData.VoltageCalTable.Rows[0][1] == null) //아무 값이 없으면
                    {
                        System.Windows.MessageBox.Show("값을 입력해주세요.");
                        return;
                    }
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    if (AllSetData.CurrentCalTable.Rows.Count < 1 || AllSetData.CurrentCalTable.Rows[0][1] == null)//아무 값이 없으면
                    {
                        System.Windows.MessageBox.Show("값을 입력해주세요.");
                        return;
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("MCU 포트를 확인하세요.");
                return;
            }
            if (AllSetData.DMMConnectFlag == 0)//DMM포트 연결 확인
            {
                System.Windows.MessageBox.Show("DMM 포트를 확인하세요.");
                return;
            }
            //데이터 추출
            if (AllSetData.AutoCalOutStartFlag == 0)
            {
                AllSetData.CalPointArray = new int[CalDataGrid.Items.Count, 2];
                //입력된 포인트를 배열로 전환
                for (int i = 0; i < CalDataGrid.Items.Count; i++)
                {
                    if (AllSetData.VoltCurrSelect == 0)
                    {
                        AllSetData.CalPointArray[i, 0] = Convert.ToInt32(AllSetData.VoltageCalTable.Rows[i][1].ToString());//전압
                        AllSetData.CalPointArray[i, 1] = Convert.ToInt32(AllSetData.VoltageCalTable.Rows[i][2].ToString()); //전류
                        AllSetData.VoltageCalTable.Rows[i][3] = "";
                        AllSetData.VoltageCalTable.Rows[i][4] = "";
                        AllSetData.VoltageCalTable.Rows[i][5] = "";
                    }
                    else if (AllSetData.VoltCurrSelect == 1)
                    {
                        AllSetData.CalPointArray[i, 0] = Convert.ToInt32(AllSetData.CurrentCalTable.Rows[i][1].ToString());//전압
                        AllSetData.CalPointArray[i, 1] = Convert.ToInt32(AllSetData.CurrentCalTable.Rows[i][2].ToString()); //전류
                        AllSetData.CurrentCalTable.Rows[i][3] = "";
                        AllSetData.CurrentCalTable.Rows[i][4] = "";
                        AllSetData.CurrentCalTable.Rows[i][5] = "";
                    }
                }
                AllSetData.AutoCalOutStartFlag = 1;
            }
        }
        //자동 Cal 출력 종료
        private void AutoCalEndBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllSetData.CalSeqStartFlag == 1)//자동cal과 실측은 유지를 해야하기 때문에 플래그가 1인상태
            {
                AllSetData.AutoCalOutStartFlag = 0;
                AllSetData.AutoCalOutEndFlag = 1; //Cal 시작일 경우에만 종료할것
            }
        }
        //실측 출력 시작
        private void OutputMeaStartBtn_Click(object sender, RoutedEventArgs e)
        {
            //예외처리
            if (AllSetData.MCUConnectFlag == 1)//MCU포트 연결 확인
            {
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    if (AllSetData.VoltageMeaTable.Rows.Count < 1 || AllSetData.VoltageMeaTable.Rows[0][1] == null) //아무 값이 없으면
                    {
                        System.Windows.MessageBox.Show("값을 입력해주세요.");
                        return;
                    }
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    if (AllSetData.CurrentMeaTable.Rows.Count < 1 || AllSetData.CurrentMeaTable.Rows[0][1] == null)//아무 값이 없으면
                    {
                        System.Windows.MessageBox.Show("값을 입력해주세요.");
                        return;
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("MCU 포트를 확인하세요.");
                return;
            }
            if (AllSetData.DMMConnectFlag == 0)//DMM포트 연결 확인
            {
                System.Windows.MessageBox.Show("DMM 포트를 확인하세요.");
                return;
            }
            //데이터 추출
            if (AllSetData.MeaOutStartFlag == 0)
            {
                AllSetData.MeaPointArray = new int[MeaDataGrid.Items.Count, 2];
                //입력된 포인트를 배열로 전환
                for (int i = 0; i < MeaDataGrid.Items.Count; i++)
                {
                    if (AllSetData.VoltCurrSelect == 0)
                    {
                        AllSetData.MeaPointArray[i, 0] = Convert.ToInt32(AllSetData.VoltageMeaTable.Rows[i][1].ToString());//전압
                        AllSetData.MeaPointArray[i, 1] = Convert.ToInt32(AllSetData.VoltageMeaTable.Rows[i][2].ToString()); //전류
                        AllSetData.VoltageMeaTable.Rows[i][3] = "";
                        AllSetData.VoltageMeaTable.Rows[i][4] = "";
                        AllSetData.VoltageMeaTable.Rows[i][5] = "";
                    }
                    else if (AllSetData.VoltCurrSelect == 1)
                    {
                        AllSetData.MeaPointArray[i, 0] = Convert.ToInt32(AllSetData.CurrentMeaTable.Rows[i][1].ToString());//전압
                        AllSetData.MeaPointArray[i, 1] = Convert.ToInt32(AllSetData.CurrentMeaTable.Rows[i][2].ToString()); //전류
                        AllSetData.CurrentMeaTable.Rows[i][3] = "";
                        AllSetData.CurrentMeaTable.Rows[i][4] = "";
                        AllSetData.CurrentMeaTable.Rows[i][5] = "";
                    }
                }
                AllSetData.MeaOutStartFlag = 1;
            }
        }       
        //실측 출력 종료
        private void OutputMeaEndBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllSetData.MeaOutStartFlag == 1) AllSetData.MeaOutEndFlag = 1;
        }

    }
}
