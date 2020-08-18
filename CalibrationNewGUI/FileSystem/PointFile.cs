using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.FileSystem
{
    /**
     *  @brief CPT, MPT 파일 관련 클래스
     *  @details CPT, MPT 파일을 Write 및 Read한다
     *
     *  @author SSW
     *  @date 2020.07.15
     *  @version 1.0.0
     */
    public class PointFile
    {
        /**
         *  @brief 포인트 파일 읽기
         *  @details CAL 포인트 또는 실측 포인트 파일을 읽어 List로 변환
         *  
         *  @param string filePath 읽을 포인트 파일의 경로
         *  
         *  @return List<object[]> List로 변환한 포인트 정보
         */
        public static List<object[]> PointReader(string filePath)
        {
            List<object[]> pointList = new List<object[]>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        string str = reader.ReadLine();
                        pointList.Add(str.Split(','));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("PointReader 오류발생 : {0}", e.Message);
            }

            return pointList;
        }

        /**
         *  @brief 포인트 파일 쓰기
         *  @details CAL 포인트 또는 실측 포인트 리스트를 파일로 변환하여 저장
         *  
         *  @param List<object[]> pointList 포인트가 저장되어 있는 리스트
         *  @param string filePath 파일을 저장할 경로
         *  
         *  @return bool 저장 성공 여부
         */
        public static bool PointWriter(List<object[]> pointList, string filePath)
        {
            try
            {
                using (StreamWriter saveStream = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    foreach (var point in pointList)
                        saveStream.WriteLine(string.Join(",", point.Take(3)));
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
