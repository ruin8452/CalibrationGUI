using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
            AllSetData.ChannelSelect = 0;
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
                //파일 이름 변경
                AllSetData.CalFileName = AllSetData.CalFileNameVolt;
                AllSetData.MeaFileName = AllSetData.MeaFileNameVolt;
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
                //파일 이름 변경
                AllSetData.CalFileName = AllSetData.CalFileNameCurr;
                AllSetData.MeaFileName = AllSetData.MeaFileNameCurr;
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
            if (AllSetData.CalSeqStartFlag == 1 || AllSetData.MeaSeqStartFlag == 1)
            {
                System.Windows.MessageBox.Show("출력중입니다.");
                return;
            }
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
            if (AllSetData.CalSeqStartFlag == 1 || AllSetData.MeaSeqStartFlag == 1)
            {
                System.Windows.MessageBox.Show("출력중입니다.");
                return;
            }
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
            if (AllSetData.MeaSeqStartFlag == 1)//자동cal과 실측은 유지를 해야하기 때문에 플래그가 1인상태
            {
                AllSetData.MeaOutStartFlag = 0;
                AllSetData.MeaOutEndFlag = 1; //Cal 시작일 경우에만 종료할것
            }
        }

        //Cal 현재 결과 데이터 csv로 저장하기
        private void AutoCalSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int i = SaveResultFile(0);
            if (i == 1) System.Windows.MessageBox.Show("파일이 저장되었습니다.");
            else System.Windows.MessageBox.Show("저장하려는 데이터가 없습니다.");
        }
        //실측 현재 결과 데이터 csv로 저장하기
        private void MeaSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int i = SaveResultFile(1);
            if(i == -1) System.Windows.MessageBox.Show("저장하려는 데이터가 없습니다."); 
            else System.Windows.MessageBox.Show("파일이 저장되었습니다.");
        }
        //Cal 현재 포인트 열기
        private void CalFileOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenPointFile(0);
        }
        //Cal 현재 포인트 저장
        private void CalFileSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int i = SavePointFile(0);
            if (i == -1) System.Windows.MessageBox.Show("저장하려는 데이터가 없습니다.");
        }
        //실측 현재 포인트 열기
        private void MeaFileOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenPointFile(1);
        }
        //실측 현재 포인트 저장
        private void MeaFileSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int i = SavePointFile(1);
            if (i == -1) System.Windows.MessageBox.Show("저장하려는 데이터가 없습니다.");
        }
        //파일 열기 함수
        private void OpenPointFile(int CalMeaSelect)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Config"; //현재 프로그램 경로
            string folderName = "";
            DataTable dt = new DataTable();//포인트 저장을 위한 변수
            // 파일 열기, OpenFileDialog
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            if (CalMeaSelect == 0) //cal 포인트
            {
                dlg.DefaultExt = ".cpt";
                dlg.Filter = "(*.cpt)|*.cpt";
                dlg.Multiselect = false;
                folderName = "\\CalPoint";
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    dt = AllSetData.VoltageCalTable;
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    dt = AllSetData.CurrentCalTable;
                }
            }
            else if (CalMeaSelect == 1)
            {
                dlg.DefaultExt = ".mpt";
                dlg.Filter = "(*.mpt)|*.mpt";
                dlg.Multiselect = false;
                folderName = "\\MeasurePoint";
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    dt = AllSetData.VoltageMeaTable;
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    dt = AllSetData.CurrentMeaTable;
                }
            }
            
            //폴더의 경로 확인
            if (System.IO.Directory.Exists(basePath + folderName))
            {
                //폴더가 있으면 해당 경로 지정
                dlg.InitialDirectory = basePath + folderName;
            }
            else
            {
                //없으면 폴더 만들기
                DirectoryInfo difo = new DirectoryInfo(basePath);
                if (difo.Exists == false) difo.Create();
                DirectoryInfo difo2 = new DirectoryInfo(basePath + folderName);
                if (difo2.Exists == false) difo2.Create();

                dlg.InitialDirectory = basePath + folderName;
            }
            // Display OpenFileDialog by calling ShowDialog method 
            bool result = (bool)dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                dt.Rows.Clear();
                StreamReader sr = new StreamReader(dlg.FileName, Encoding.GetEncoding("UTF-8"));
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    string[] temp = s.Split(',');        // Split() 메서드를 이용하여 ',' 구분하여 잘라냄
                    dt.Rows.Add(temp[0], temp[1], temp[2], "", "", "");
                }
                if (CalMeaSelect == 0)
                {
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        AllSetData.CalFileNameVolt = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.CalFileName = AllSetData.CalFileNameVolt;
                    }
                    else if (AllSetData.VoltCurrSelect == 1)//전류
                    {
                        AllSetData.CalFileNameCurr = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.CalFileName = AllSetData.CalFileNameCurr;
                    }
                }
                else if (CalMeaSelect == 1)
                {
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        AllSetData.MeaFileNameVolt = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.MeaFileName = AllSetData.MeaFileNameVolt;
                    }
                    else if (AllSetData.VoltCurrSelect == 1)//전류
                    {
                        AllSetData.MeaFileNameCurr = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.MeaFileName = AllSetData.MeaFileNameCurr;
                    }
                }
            }
        }
        //파일 저장 함수
        private int SavePointFile(int CalMeaSelect)
        {
            StringBuilder sb = new StringBuilder();
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "Config"; //현재 프로그램 경로
            string folderName = "";
            DataTable dt = new DataTable();//포인트 저장을 위한 변수
            // 파일 열기, OpenFileDialog
            // Create OpenFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            if (CalMeaSelect == 0) //cal 포인트
            {
                dlg.DefaultExt = ".cpt";
                dlg.Filter = "(*.cpt)|*.cpt";
                dlg.Title = "Cal 포인트 저장";
                dlg.AddExtension = true;
                folderName = "\\CalPoint";
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    dt = AllSetData.VoltageCalTable;
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    dt = AllSetData.CurrentCalTable;
                }
            }
            else if (CalMeaSelect == 1)
            {
                dlg.DefaultExt = ".mpt";
                dlg.Filter = "(*.mpt)|*.mpt";
                dlg.Title = "Measure 포인트 저장";
                dlg.AddExtension = true;
                folderName = "\\MeasurePoint";
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    dt = AllSetData.VoltageMeaTable;
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    dt = AllSetData.CurrentMeaTable;
                }
            }
            if (dt.Rows.Count < 1)//포인트값이 없으면 실패
            {
                return -1;
            }
            //폴더의 경로 확인
            if (System.IO.Directory.Exists(basePath + folderName))
            {
                //폴더가 있으면 해당 경로 지정
                dlg.InitialDirectory = basePath + folderName;
            }
            else
            {
                //없으면 폴더 만들기
                DirectoryInfo difo = new DirectoryInfo(basePath);
                if (difo.Exists == false) difo.Create();
                DirectoryInfo difo2 = new DirectoryInfo(basePath + folderName);
                if (difo2.Exists == false) difo2.Create();

                dlg.InitialDirectory = basePath + folderName;
            }

            // Display OpenFileDialog by calling ShowDialog method 
            bool result = (bool)dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                //실제 파일 작성하기
                for (int i=0;i<dt.Rows.Count;i++)
                {
                    string strRow = dt.Rows[i][0].ToString() + "," + dt.Rows[i][1].ToString() + "," + dt.Rows[i][2].ToString();
                    sb.AppendLine(strRow);
                }
                File.WriteAllText(dlg.FileName, sb.ToString());
                if (CalMeaSelect == 0)
                {
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        AllSetData.CalFileNameVolt = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.CalFileName = AllSetData.CalFileNameVolt;
                    }
                    else if (AllSetData.VoltCurrSelect == 1)//전류
                    {
                        AllSetData.CalFileNameCurr = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.CalFileName = AllSetData.CalFileNameCurr;
                    }
                }
                else if (CalMeaSelect == 1)
                {
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        AllSetData.MeaFileNameVolt = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.MeaFileName = AllSetData.MeaFileNameVolt;
                    }
                    else if (AllSetData.VoltCurrSelect == 1)//전류
                    {
                        AllSetData.MeaFileNameCurr = System.IO.Path.GetFileName(dlg.FileName);
                        AllSetData.MeaFileName = AllSetData.MeaFileNameCurr;
                    }
                }
            }
            return 0;
        }
        //결과데이터 저장 함수
        private int SaveResultFile(int CalMeaSelect)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            string SavePath = (AllSetData.SaveFilePrixNum + "_" + AllSetData.SerialNumber);//처음 접두사 입력해놓기(prix + serial)

            if (CalMeaSelect == 0)
            {
                if (AllSetData.ChannelSelect == 1)//CH1
                {
                    SavePath += "_CH1";
                }
                else if (AllSetData.ChannelSelect == 2)//CH2
                {
                    SavePath += "_CH2";
                }
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    dt = AllSetData.VoltageCalTable;
                    SavePath += "_Volt";
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    dt = AllSetData.CurrentCalTable;
                    SavePath += "_Curr";
                }
                SavePath += "_Calibration.csv";
            }
            else if (CalMeaSelect == 1)
            {
                if (AllSetData.ChannelSelect == 1)//CH1
                {
                    SavePath += "_CH1";
                }
                else if (AllSetData.ChannelSelect == 2)//CH2
                {
                    SavePath += "_CH2";
                }
                if (AllSetData.VoltCurrSelect == 0)//전압
                {
                    dt = AllSetData.VoltageMeaTable;
                    SavePath += "_Volt";
                }
                else if (AllSetData.VoltCurrSelect == 1)//전류
                {
                    dt = AllSetData.CurrentMeaTable;
                    SavePath += "_Curr";
                }
                SavePath += "_Measurement.csv";
            }
            if (dt.Rows.Count < 1) return -1;
            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(SavePath, sb.ToString());
            return 1;
        }
        //Cal 후 자동 실측으로 넘어가기
        private void AutoMeaChecked(object sender, RoutedEventArgs e)
        {
            AllSetData.AutoMeaStartFlag = 1;
        }

        private void AutoMeaUnChecked(object sender, RoutedEventArgs e)
        {
            AllSetData.AutoMeaStartFlag = 0;
        }
    }
}
