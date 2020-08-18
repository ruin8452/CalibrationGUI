using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.FileSystem
{
    /**
     *  @brief CSV 파일 관리 클래스
     *  @details CSV 파일의 전반적인 관리를 담당
     *
     *  @author SSW
     *  @date 2020.08.18
     *  @version 1.0.0
     */
    public class CsvFile
    {
        /**
         *  @brief 데이터 저장
         *  @details 저장경로에 데이터를 ','로 구분하여 저장한다.@n
         *           경로에 파일이 없다면 생성하여 저장한다.
         *  @version 1.0.0
         *  
         *  @param string filePath 저장할 파일의 경로
         *  @param bool overWrite 덮어쓰기 여부
         *  @param object[] data 추가할 열의 이름
         *  
         *  @return bool 저장 성공여부@n
         *               True : 파일저장 성공@n
         *               False : 파일저장 실패
         */
        public static bool Save(string filePath, bool overWrite, object[] data)
        {
            try
            {
                using (StreamWriter csvStream = new StreamWriter(filePath, overWrite, Encoding.UTF8))
                {
                    csvStream.WriteLine(string.Join(",", data));
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /**
         *  @brief 데이터 읽기
         *  @details 저장경로의 파일을 읽어 반환한다.
         *  @version 1.0.0
         *  
         *  @param string filePath 읽을 파일의 경로
         *  
         *  @return string[] 파일에서 읽은 데이터@n
         *                   '\n'으로 구분하여 자른 문자열배열
         */
        public static string[] Read(string filePath)
        {
            string[] data;

            using (StreamReader csvStream = new StreamReader(filePath, Encoding.UTF8))
            {
                data = csvStream.ReadToEnd().Split('\n');
            }

            return data;
        }
    }
}
