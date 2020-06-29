namespace J_Project.Communication.CommFlags
{
    /**
     * @brief 통신 인터페이스 종류 열거형
     * 
     * @li NONE   : 무소속
     * @li SERIAL : 시리얼 통신
     * @li USB    : USB 통신
     * @li GPIB   : GPIB 통신
     * @li TCP    : TCP 통신
     */
    public enum CommTypeFlag
    {
        NONE,
        SERIAL,
        USB,
        GPIB,
        TCP
    }

    /**
     * @brief 통신 온오프 플래그
     * 
     * @li OFF : 출력 중지
     * @li ON : 출력
     */
    public enum CtrlFlag
    {
        OFF,
        ON
    }

    /**
     * @brief 통신 시도 결과 플래그
     * 
     * @li FAIL : 통신 실패
     * @li WAIT : 대기
     * @li SUCCESS : 통신 성공
     */
    public enum TryResultFlag
    {
        FAIL,
        WAIT,
        SUCCESS
    }
}
