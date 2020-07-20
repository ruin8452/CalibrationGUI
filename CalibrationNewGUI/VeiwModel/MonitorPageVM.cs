using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.FileSystem;
using CalibrationNewGUI.Model;
using CalibrationNewGUI.VeiwModel.Func;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace CalibrationNewGUI.VeiwModel
{
    /**
     *  @brief Monitor 페이지 연결 클래스
     *  @details Monitor 페이지에 사용되는 변수 및 함수가 포함되어 있다.
     *
     *  @author SSW
     *  @date 2020.07.15
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    class MonitorPageVM
    {
        public bool CalMode { get; set; } = true;   // CAL모드 true : 전압, false : 전류
        public int CalGridSelectedIndex { get; set; }   // CAL 테이블의 선택된 Index
        public int MeaGridSelectedIndex { get; set; }   // MEA 테이블의 선택된 Index

        public CalMeasureInfo CalMeaInfo { get; set; }
        public OthersInfo OtherInfos { get; set; }
        public Mcu Mcu { get; set; }
        public Dmm Dmm { get; set; }

        public string CalVoltFilePath = string.Empty;  // 전압CPT 파일 경로
        public string MeaVoltFilePath = string.Empty;  // 전압MPT 파일 경로
        public string CalCurrFilePath = string.Empty;  // 전류CPT 파일 경로
        public string MeaCurrFilePath = string.Empty;  // 전류MPT 파일 경로

        int ChNumber;

        public string CalFilePath { get; set; } // CPT 파일 경로(GUI 노출용)
        public string MeaFilePath { get; set; } // MPT 파일 경로(GUI 노출용)

        public List<object[]> CalVoltData = new List<object[]>();  // 전압 CalPoint 데이터 리스트
        public List<object[]> MeaVoltData = new List<object[]>();  // 전압 MeaPoint 데이터 리스트
        public List<object[]> CalCurrData = new List<object[]>();  // 전류 CalPoint 데이터 리스트
        public List<object[]> MeaCurrData = new List<object[]>();  // 전류 MeaPoint 데이터 리스트

        public DataTable CalPointTable { get; set; }  // CalPoint 데이터 테이블(GUI 노출용)
        public DataTable MeaPointTable { get; set; }  // MeaPoint 데이터 테이블(GUI 노출용)

        public ICommand FileOpenClick { get; set; }  // 파일 열기 버튼 커맨드
        public ICommand FileSaveClick { get; set; }  // 파일 저장 버튼 커맨드

        public ICommand ModeSelectClick { get; set; }   // 전압/전류 모드 선택 커맨드
        public ICommand ChSelectClick { get; set; } // 채널 선택 커맨드

        public ICommand PointAddClick { get; set; }
        public ICommand PointDeleteClick { get; set; }
        public ICommand PointUpClick { get; set; }
        public ICommand PointDownClick { get; set; }

        /**
         *  @brief Monitor 초기화
         *  @details
         *  
         *  @param
         *  
         *  @return
         */
        public MonitorPageVM()
        {
            CalMeaInfo = CalMeasureInfo.GetObj();
            OtherInfos = OthersInfo.GetObj();
            Mcu = Mcu.GetObj();
            Dmm = Dmm.GetObj();

            CalPointTable = new DataTable();
            CalPointTable = TableManager.ColumnAdd(CalPointTable, new string[] { "NO", "SetVolt", "SetCurr", "OutVolt", "OutCurr", "OutDMM" });

            MeaPointTable = new DataTable();
            MeaPointTable = TableManager.ColumnAdd(MeaPointTable, new string[] { "NO", "SetVolt", "SetCurr", "OutVolt", "OutCurr", "OutDMM" });

            CalFilePath = CalVoltFilePath;
            MeaFilePath = MeaVoltFilePath;

            FileOpenClick = new BaseObjCommand(FileOpenDialog);
            FileSaveClick = new BaseObjCommand(FileSaveDialog);

            ModeSelectClick = new BaseCommand(ModeSelect);
            ChSelectClick = new BaseObjCommand(ChSelect);

            PointAddClick = new BaseObjCommand(PointAdd);
            PointDeleteClick = new BaseObjCommand(PointDelete);
            PointUpClick = new BaseObjCommand(PointUp);
            PointDownClick = new BaseObjCommand(PointDown);

            //CalPointTable.RowChanging += RowChanging;
        }

        private void RowChanging(object sender, DataRowChangeEventArgs e)
        {
            long volt = long.Parse(e.Row[1].ToString());
            long curr = long.Parse(e.Row[2].ToString());

            if (volt > OtherInfos.InputVoltMax || volt < OtherInfos.InputVoltMin)
                MessageBox.Show($"범위를 초과한 값을 입력하셨습니다.\n" +
                                $"전압 최대값 : {OtherInfos.InputVoltMax}\n전압 최소값 : {OtherInfos.InputVoltMin}");

            if (curr > OtherInfos.InputCurrMax || curr < OtherInfos.InputCurrMin)
                MessageBox.Show($"범위를 초과한 값을 입력하셨습니다.\n" +
                                $"전류 최대값 : {OtherInfos.InputCurrMax}\n전류 최소값 : {OtherInfos.InputCurrMin}");
        }

        /**
         *  @brief 포인트 파일 열기
         *  @details 포인트 타입에 따라 다이얼로그를 통해 포인트 파일을 열고 포인트 테이블에 삽입
         *  
         *  @param object type 호출한 객체의 타입(CAL, MEA)
         *  
         *  @return
         */
        private void FileOpenDialog(object type)
        {
            OpenFileDialog openDialog;
            string folderPath;
            if (type.ToString() == "CAL")
            {
                openDialog = new OpenFileDialog
                {
                    Title = "CAL 포인트 파일 선택",
                    Filter = "CPT파일 (*.cpt)|*.cpt|All files (*.*)|*.*"
                };
                folderPath = CalFilePath;
            }
            else
            {
                openDialog = new OpenFileDialog
                {
                    Title = "실측 포인트 파일 선택",
                    Filter = "MPT파일 (*.mpt)|*.mpt|All files (*.*)|*.*"
                };
                folderPath = MeaFilePath;
            }

            // 경로가 정상적일때 해당 경로를 열기
            if (File.Exists(folderPath))
                openDialog.InitialDirectory = folderPath;
            // 아니면 해당 프로그램 폴더 경로를 열기
            else
                openDialog.InitialDirectory = Environment.CurrentDirectory;

            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            if (type.ToString() == "CAL")
            {
                CalFilePath = openDialog.FileName;

                CalPointTable.Rows.Clear();
                foreach (var point in PointFile.PointReader(CalFilePath))
                    CalPointTable.Rows.Add(point);
            }
            else
            {
                MeaFilePath = openDialog.FileName;

                MeaPointTable.Rows.Clear();
                foreach (var point in PointFile.PointReader(MeaFilePath))
                    MeaPointTable.Rows.Add(point);
            }

            openDialog.Dispose();
        }

        /**
         *  @brief 포인트 파일 저장
         *  @details 포인트 타입에 따라 다이얼로그를 통해 저장할 경로를 열고 포인트를 파일로 저장
         *  
         *  @param object type 호출한 객체의 타입(CAL, MEA)
         *  
         *  @return
         */
        private void FileSaveDialog(object type)
        {
            SaveFileDialog saveDialog;
            string folderPath;

            // CAL포인트 Save
            if (type.ToString() == "CAL")
            {
                saveDialog = new SaveFileDialog
                {
                    Title = "CAL 포인트 저장",
                    Filter = "CPT파일 (*.cpt)|*.cpt|All files (*.*)|*.*",
                    DefaultExt = ".cpt",
                    AddExtension = true
                };
                folderPath = CalFilePath;
            }
            // MEA포인트 Save
            else
            {
                saveDialog = new SaveFileDialog
                {
                    Title = "실측 포인트 저장",
                    Filter = "MPT파일 (*.mpt)|*.mpt|All files (*.*)|*.*",
                    DefaultExt = ".mpt",
                    AddExtension = true
                };
                folderPath = MeaFilePath;
            }

            // 경로가 정상적일때 해당 경로를 열기
            if (File.Exists(folderPath))
                saveDialog.InitialDirectory = folderPath;
            // 아니면 해당 프로그램 폴더 경로를 열기
            else
                saveDialog.InitialDirectory = Environment.CurrentDirectory;

            if (saveDialog.ShowDialog() != DialogResult.OK) // 다이얼 로그에서 OK버튼을 눌렀을 경우
                return;

            List<object[]> pointList = new List<object[]>();
            if (type.ToString() == "CAL")
            {
                CalFilePath = saveDialog.FileName;

                foreach (DataRow row in CalPointTable.Rows)
                    pointList.Add(row.ItemArray);
            }
            else
            {
                MeaFilePath = saveDialog.FileName;

                foreach (DataRow row in MeaPointTable.Rows)
                    pointList.Add(row.ItemArray);
            }
            PointFile.PointWriter(pointList, saveDialog.FileName);

            saveDialog.Dispose();
        }

        /**
         *  @brief 포인트 추가
         *  @details 포인트 타입에 따라 테이블에 새로운 포인트 추가@n
         *           선택된 Row 아래에 생성, 선택된 Row가 없을 시 맨 위에 추가
         *  
         *  @param object type 호출한 객체의 타입(CAL, MEA)
         *  
         *  @return
         */
        private void PointAdd(object type)
        {
            // CAL포인트 추가
            if (type.ToString() == "CAL")
                CalPointTable = TableManager.RowAdd(CalPointTable, CalGridSelectedIndex + 1);
            // 실측포인트 추가
            else
                MeaPointTable = TableManager.RowAdd(MeaPointTable, MeaGridSelectedIndex + 1);
        }

        /**
         *  @brief 포인트 삭제
         *  @details 포인트 타입에 따라 테이블의 포인트 삭제@n
         *           선택된 Row 삭제
         *  
         *  @param object type 호출한 객체의 타입(CAL, MEA)
         *  
         *  @return
         */
        private void PointDelete(object type)
        {
            //<C>20.SSW 07.15 : Row 삭제 후 선택 인덱스를 유지하기 위함
            int tempIndex;

            // CAL포인트 삭제
            if (type.ToString() == "CAL")
            {
                tempIndex = CalGridSelectedIndex;
                CalPointTable = TableManager.RowDelete(CalPointTable, tempIndex);
                CalGridSelectedIndex = tempIndex;
            }
            // 실측포인트 삭제
            else
            {
                tempIndex = MeaGridSelectedIndex;
                MeaPointTable = TableManager.RowDelete(MeaPointTable, tempIndex);
                MeaGridSelectedIndex = tempIndex;
            }
        }

        /**
         *  @brief 포인트 Up
         *  @details 포인트 타입에 따라 테이블의 포인트 인덱스를 Up
         *  
         *  @param object type 호출한 객체의 타입(CAL, MEA)
         *  
         *  @return
         */
        private void PointUp(object type)
        {
            //<C>20.SSW 07.15 : 선택되어 있는 Row가 삭제될 시 바인딩 되어 있는 Index 변수가 -1로 변경되기 때문에 다른 변수로 컨트롤
            int tempIndex;

            // CAL포인트 UP
            if (type.ToString() == "CAL")
            {
                tempIndex = CalGridSelectedIndex;
                CalPointTable = TableManager.RowUp(CalPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex <= 0)
                    CalGridSelectedIndex = tempIndex;
                else
                    CalGridSelectedIndex = tempIndex - 1;
            }
            // 실측포인트 UP
            else
            {
                tempIndex = MeaGridSelectedIndex;
                MeaPointTable = TableManager.RowUp(MeaPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex <= 0)
                    MeaGridSelectedIndex = tempIndex;
                else
                    MeaGridSelectedIndex = tempIndex - 1;
            }
        }

        /**
         *  @brief 포인트 Down
         *  @details 포인트 타입에 따라 테이블의 포인트 인덱스를 Down
         *  
         *  @param object type 호출한 객체의 타입(CAL, MEA)
         *  
         *  @return
         */
        private void PointDown(object type)
        {
            //<C>20.SSW 07.15 : 선택되어 있는 Row가 삭제될 시 바인딩 되어 있는 Index 변수가 -1로 변경되기 때문에 다른 변수로 컨트롤
            int tempIndex;

            // CAL포인트 DOWN
            if (type.ToString() == "CAL")
            {
                tempIndex = CalGridSelectedIndex;
                CalPointTable = TableManager.RowDown(CalPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex >= CalPointTable.Rows.Count - 1)
                    CalGridSelectedIndex = tempIndex;
                else
                    CalGridSelectedIndex = tempIndex + 1;
            }
            // 실측포인트 DOWN
            else
            {
                tempIndex = MeaGridSelectedIndex;
                MeaPointTable = TableManager.RowDown(MeaPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if(tempIndex >= MeaPointTable.Rows.Count - 1)
                    MeaGridSelectedIndex = tempIndex;
                else
                    MeaGridSelectedIndex = tempIndex + 1;
            }
        }

        /**
         *  @brief 모드 선택
         *  @details 모드 변경시 교체되어야 하는 노출용 변수를 교체한다.@n
         *           교체 항목
         *  @li 1.CPT 파일 경로 
         *  @li 2.MPT 파일 경로
         *  @li 3.CAL 테이블
         *  @li 4.MEA 테이블
         *  
         *  @param
         *  
         *  @return
         */
        private void ModeSelect()
        {
            // Volt로 교체시
            if (CalMode)
            {
                // CAL 파일경로 변수 교체
                CalCurrFilePath = CalFilePath;
                CalFilePath = CalVoltFilePath;

                // MEA 파일경로 변수 교체
                MeaCurrFilePath = MeaFilePath;
                MeaFilePath = MeaVoltFilePath;

                // CalPoint 데이터 테이블 교체
                CalCurrData.Clear();
                foreach(DataRow row in CalPointTable.Rows)
                    CalCurrData.Add(row.ItemArray);
                CalPointTable.Rows.Clear();
                foreach (object[] rowSource in CalVoltData)
                    CalPointTable.Rows.Add(rowSource);

                // MeaPoint 데이터 테이블 교체
                MeaCurrData.Clear();
                foreach (DataRow row in MeaPointTable.Rows)
                    MeaCurrData.Add(row.ItemArray);
                MeaPointTable.Rows.Clear();
                foreach (object[] rowSource in MeaVoltData)
                    MeaPointTable.Rows.Add(rowSource);
            }
            // Curr로 교체시
            else
            {
                // CAL 파일경로 변수 교체
                CalVoltFilePath = CalFilePath;
                CalFilePath = CalCurrFilePath;

                // MEA 파일경로 변수 교체
                MeaVoltFilePath = MeaFilePath;
                MeaFilePath = MeaCurrFilePath;

                // CalPoint 데이터 테이블 교체
                CalVoltData.Clear();
                foreach (DataRow row in CalPointTable.Rows)
                    CalVoltData.Add(row.ItemArray);
                CalPointTable.Rows.Clear();
                foreach (object[] rowSource in CalCurrData)
                    CalPointTable.Rows.Add(rowSource);

                // MeaPoint 데이터 테이블 교체
                MeaVoltData.Clear();
                foreach (DataRow row in MeaPointTable.Rows)
                    MeaVoltData.Add(row.ItemArray);
                MeaPointTable.Rows.Clear();
                foreach (object[] rowSource in MeaCurrData)
                    MeaPointTable.Rows.Add(rowSource);
            }
        }

        /**
         *  @brief 채널 선택
         *  @details CAL 시 사용되는 채널번호 변경
         *  
         *  @param object chNum 채널 번호
         *  @li 0 : 듀얼
         *  @li 1 : 채널1
         *  @li 2 : 채널2
         *  
         *  @return
         */
        private void ChSelect(object chNum)
        {
            ChNumber = int.Parse(chNum.ToString());
        }
    }
}
