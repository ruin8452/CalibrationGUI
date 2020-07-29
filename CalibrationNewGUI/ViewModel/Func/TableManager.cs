﻿using CalibrationNewGUI.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.ViewModel.Func
{
    /**
     *  @brief DataTable 관리 클래스
     *  @details DataTable의 전반적인 관리를 담당
     *
     *  @author SSW
     *  @date 2020.07.16
     *  @version 1.0.0
     */
    public class TableManager
    {
        /**
         *  @brief 테이블 열 추가
         *  @details 테이블에 새로운 복수의 열 추가@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param string[] columnNames 추가할 열의 이름
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        public static DataTable ColumnAdd(DataTable table, string[] columnNames)
        {
            foreach (string name in columnNames)
                table.Columns.Add(name);

            return table;
        }

        /**
         *  @brief 테이블 행 추가
         *  @details 테이블에 새로운 행 추가 후 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 추가할 행의 위치
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        //TODO:테이블이 가지고 있는 행을 벗어난 인덱스가 삽입될 시 예외처리
        public static DataTable RowAdd(DataTable table, int index)
        {
            DataRow newRow = table.NewRow();
            table.Rows.InsertAt(newRow, index);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }

        /**
         *  @brief 테이블 행 추가
         *  @details 테이블에 새로운 행 추가 후 데이터 삽입 및 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 추가할 행의 위치
         *  @param int setVolt 추가한 행의 2번 열의 값
         *  @param int setCurr 추가할 행의 3번 열의 값
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        //TODO:테이블이 가지고 있는 행을 벗어난 인덱스가 삽입될 시 예외처리
        public static DataTable RowAdd(DataTable table, int index, int data1, int data2)
        {
            DataRow newRow = table.NewRow();
            newRow[1] = data1;
            newRow[2] = data2;
            table.Rows.InsertAt(newRow, index);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }

        /**
         *  @brief 테이블 행 추가
         *  @details 테이블에 새로운 행 추가 후 데이터 삽입 및 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 추가할 행의 위치
         *  @param object[] data 추가할 행에 삽입 할 데이터
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        //TODO:테이블이 가지고 있는 행을 벗어난 인덱스가 삽입될 시 예외처리
        public static DataTable RowAdd(DataTable table, int index, object[] data)
        {
            DataRow newRow = table.NewRow();
            newRow.ItemArray = data;
            table.Rows.InsertAt(newRow, index);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }

        /**
         *  @brief 테이블 행 추가
         *  @details 테이블에 새로운 행 추가 후 데이터 삽입 및 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 추가할 행의 위치
         *  @param DataRow addRow 추가할 행
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        //TODO:테이블이 가지고 있는 행을 벗어난 인덱스가 삽입될 시 예외처리
        public static DataTable RowAdd(DataTable table, int index, DataRow addRow)
        {
            DataRow newRow = table.NewRow();
            newRow.ItemArray = addRow.ItemArray;
            table.Rows.InsertAt(newRow, index);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }

        /**
         *  @brief 포인트 삭제
         *  @details 테이블의 행 삭제 후 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 삭제할 행의 위치
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        public static DataTable RowDelete(DataTable table, int index)
        {
            // 삭제할 행이 없다면 패스
            if(table.Rows.Count == 0)
                return table;

            // 인덱스가 행 범위를 벗어났다면 마지막 행 삭제
            if (index < 0 || index >= table.Rows.Count)
                table.Rows.RemoveAt(table.Rows.Count - 1);
            else
                table.Rows.RemoveAt(index);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }

        /**
         *  @brief 포인트 Up
         *  @details 테이블의 행위치를 Up 후 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 이동시키려는 행의 위치
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        //TODO:테이블이 가지고 있는 행을 벗어난 인덱스가 삽입될 시 예외처리
        public static DataTable RowUp(DataTable table, int index)
        {
            // Row 선택이 안되어있거나(-1), 최상위 Index일 경우(0)
            if (index <= 0 || index > table.Rows.Count - 1)
                return table;

            DataRow tempRow = table.NewRow();
            tempRow.ItemArray = table.Rows[index].ItemArray;
            table.Rows.RemoveAt(index);
            table.Rows.InsertAt(tempRow, index - 1);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }

        /**
         *  @brief 포인트 Down
         *  @details 테이블의 행위치를 Down 후 인덱스 수정@n
         *  
         *  @param DataTable table 컨트롤 할 데이터 테이블
         *  @param int index 이동시키려는 행의 위치
         *  
         *  @return DataTable 수정된 데이터 테이블
         */
        //TODO:테이블이 가지고 있는 행을 벗어난 인덱스가 삽입될 시 예외처리
        public static DataTable RowDown(DataTable table, int index)
        {
            // Row 선택이 안되어있거나(-1), 최하위 Index일 경우
            if (index <= -1 || index >= table.Rows.Count - 1)
                return table;

            DataRow tempRow = table.NewRow();
            tempRow.ItemArray = table.Rows[index].ItemArray;
            table.Rows.RemoveAt(index);
            table.Rows.InsertAt(tempRow, index + 1);

            // 인덱스 수정
            for (int i = 0; i < table.Rows.Count; i++)
                table.Rows[i][0] = i + 1;

            return table;
        }
    }
}