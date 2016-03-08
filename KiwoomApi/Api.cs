using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using log4net;

namespace Kiwoom
{
    public partial class Api: UserControl
    {
        private static string SCR_NO_SEARCH_CONDITION   = "0001";   // 조건검색식 화면번호
        private static string SCR_NO_REQUEST_DATA = "0002";         // 시세데이터 요청
        private ILog log = null;
        public delegate void ConnectionStateHandler(Api sender);
        public delegate void ReceiveTrConditionHandler(Api sender, string[] strCodeList, ConditionInfo info);

        enum SearchConditionType:int
        {
            Normal = 0,     /// 일반조회(0) 
            Realtime = 1    /// 실시간조회(1)
        }

        /// <summary>
        /// 키움API접속성공시 발생하는 이벤트
        /// </summary>
        public event ConnectionStateHandler OnConnected;

        /// <summary>
        /// 사용자조건검색 결과 이벤트
        /// </summary>
        public event ReceiveTrConditionHandler OnReceiveTrCondition;

        public enum ErrorCode : int
        {   
            OP_ERR_NONE             = 0,    ///"정상처리" 
            OP_ERR_LOGIN            =-100,  ///"사용자정보교환에 실패하였습니다. 잠시후 다시 시작하여 주십시오." 
            OP_ERR_CONNECT          =-101,  ///"서버 접속 실패" 
            OP_ERR_VERSION          =-102,  ///"버전처리가 실패하였습니다." 
            OP_ERR_SISE_OVERFLOW    =-200,  ///”시세조회 과부하” 
            OP_ERR_RQ_STRUCT_FAIL   =-201,  ///”REQUEST_INPUT_st Failed” 
            OP_ERR_RQ_STRING_FAIL   =-202,  ///”요청 전문 작성 실패” 
            OP_ERR_ORD_WRONG_INPUT  =-300,  ///”주문 입력값 오류” 
            OP_ERR_ORD_WRONG_ACCNO  =-301,  ///”계좌비밀번호를 입력하십시오.” 
            OP_ERR_OTHER_ACC_USE    =-302,  ///”타인계좌는 사용할 수 없습니다." 
            OP_ERR_MIS_2BILL_EXC    =-303,  ///”주문가격이 20억원을 초과합니다.” 
            OP_ERR_MIS_5BILL_EXC    =-304,  ///”주문가격은 50억원을 초과할 수 없습니다.” 
            OP_ERR_MIS_1PER_EXC     =-305,  ///”주문수량이 총발행주수의 1%를 초과합니다.” 
            OP_ERR_MID_3PER_EXC     =-306  ///”주문수량은 총발행주수의 3%를 초과할 수 없습니다.”   
        }

        #region Properties

        /// <summary>
        /// 키움API연결상태
        /// </summary>
        public bool IsConnected
        {
            get { return m_axKHOpenAPI.GetConnectState()==1;  }
        }
        
        /// <summary>
        /// 로그인 사용자ID
        /// </summary>
        public string UserID
        {
            get { return m_axKHOpenAPI.GetLoginInfo("USER_ID"); }
        }

        /// <summary>
        /// 로그인 사용자명
        /// </summary>
        public string UserName
        {
            get { return m_axKHOpenAPI.GetLoginInfo("USER_NAME"); }
        }

        /// <summary>
        /// 계좌목록
        /// </summary>
        public string[] Accounts
        {
            get
            {
                string ret = m_axKHOpenAPI.GetLoginInfo("ACCNO");
                char[] sep = new char[] { ';' };
                return ret.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        #endregion

        public Api()
        {
            InitializeComponent();
            log = Log.Get(this.GetType());

            m_axKHOpenAPI.OnEventConnect += M_axKHOpenAPI_OnEventConnect;
            m_axKHOpenAPI.OnReceiveTrData += M_axKHOpenAPI_OnReceiveTrData;
            m_axKHOpenAPI.OnReceiveTrCondition += M_axKHOpenAPI_OnReceiveTrCondition;
            m_axKHOpenAPI.OnReceiveRealCondition += M_axKHOpenAPI_OnReceiveRealCondition;
            m_axKHOpenAPI.OnReceiveMsg += M_axKHOpenAPI_OnReceiveMsg;
            m_axKHOpenAPI.OnReceiveConditionVer += M_axKHOpenAPI_OnReceiveConditionVer;
        }

        private void M_axKHOpenAPI_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            e.
            throw new NotImplementedException();
        }

        private void M_axKHOpenAPI_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            log.Debug("msg=" + e.sMsg);
        }

        private void M_axKHOpenAPI_OnReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            throw new NotImplementedException();
        }

        private void M_axKHOpenAPI_OnReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            /* OnReceiveTrCondition(LPCTSTR sScrNo, LPCTSTR strCodeList, LPCTSTR strConditionName, int nIndex, int nNext) 
                이벤트 함수로 종목리스트가 들어옵니다. 
                -파라메터 설명 
                sScrNo : 화면번호 
                strCodeList : 조회된 종목리스트(ex:039490;005930;036570;…;) 
                strConditionName : 조회된 조건명 
                nIndex : 조회된 조건명 인덱스 
                nNext : 연속조회 여부(0:연속조회없음, 2:연속조회 있음) */
            if (e.sScrNo == SCR_NO_SEARCH_CONDITION && OnReceiveTrCondition != null)
            {
                string[] codeList = e.strCodeList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                OnReceiveTrCondition(this, codeList, new ConditionInfo(e.nIndex, e.strConditionName));

                if (e.nNext == 2)
                    m_axKHOpenAPI.SendCondition(e.sScrNo, e.strConditionName, e.nIndex, (int)SearchConditionType.Realtime);
            }   
        }

        private void M_axKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            int cCount = m_axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRecordName);

            for(int i=0; i<cCount; i++)
            {
                string sName = m_axKHOpenAPI.GetCommData(e.sTrCode, e.sRecordName, i, "종목명");
                sName = sName.Trim();
                string sPrice = m_axKHOpenAPI.GetCommData(e.sTrCode, e.sRecordName, i, "현재가");
                sPrice = sPrice.Trim();
                log.Debug("[" + i.ToString("00") + "] name=" + sName + ", price=" + sPrice);
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// 키움API 접속
        /// </summary>
        public void Connect(){
            Disconnect();
            m_axKHOpenAPI.CommConnect();
        }

        /// <summary>
        /// 키움API 접속종료
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                m_axKHOpenAPI.CommTerminate();
                log.Debug("KHOpenAPI.CommTerminate() Call...");
            }
        }

        /// <summary>
        /// 사용자가 설정한 조건식 리스트를 가져온다.
        /// </summary>
        /// <returns>조건식리스트</returns>
        public ConditionInfo[] GetConditionInfoList()
        {
            List<ConditionInfo> list = new List<ConditionInfo>();

            // 서버에 저장된 사용자 조건식을 가져온다.
            int ret = m_axKHOpenAPI.GetConditionLoad();
            Debug.Assert(ret != 0);
            if (ret > 0)
            {
                string str = m_axKHOpenAPI.GetConditionNameList();
                log.Debug("수신받은 조건식목록='" + str + "'");
                char[] sep = new char[] { ';' };
                string[] strList = str.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                foreach(string sItem in strList)
                {
                    string[] strKV = sItem.Split('^');
                    Debug.Assert(strKV.Length == 2);

                    ConditionInfo info = new ConditionInfo(Convert.ToInt32(strKV[0]), strKV[1]);
                    list.Add(info);
                }
            }
            return list.ToArray();
        }
        
        /// <summary>
        /// 조건검색 결과 요청(실시간)
        /// </summary>
        /// <param name="info">요청할 조건검색식 정보</param>
        /// <returns>성공시 true</returns>
        public bool RequestSearchCondition(ConditionInfo info)
        {
            /*  SendCondition
                -반환값 : FALSE(실패), TRUE(성공) 
                -파라메터 설명 
                    strScrNo : 화면번호 
                    strConditionName :GetConditionNameList()로 불러온 조건명중 하나의 조건명. 
                    nIndex : GetCondionNameList()로 불러온 조건인덱스. 
                    nSearch : 일반조회(0), 실시간조회(1), 연속조회(2) 
                        nSearch 를 0으로 조회하면 단순 해당 조건명(식)에 맞는 종목리스트를  
                        받아올 수 있습니다. 1로 조회하면 해당 조건명(식)에 맞는 종목리스트를 받아 
                        오면서 실시간으로 편입, 이탈하는 종목을 받을 수 있는 조건이 됩니다. 
                        -1번으로 조회 할 수 있는 화면 개수는 최대 10개까지 입니다. 
                        -2은 OnReceiveTrCondition 이벤트 함수에서 마지막 파라메터인 nNext가 “2”로 
                        들어오면 종목이 더 있기 때문에 다음 조회를 원할 때 OnReceiveTrCondition 
                        이벤트 함수에서 사용하시면 됩니다. 
                -결과값 
                OnReceiveTrCondition(LPCTSTR sScrNo, LPCTSTR strCodeList, LPCTSTR strConditionName, int nIndex, int nNext) 
                이벤트 함수로 종목리스트가 들어옵니다. 
            */
            return m_axKHOpenAPI.SendCondition(SCR_NO_SEARCH_CONDITION, info.Name, (int)info.Index, (int)SearchConditionType.Realtime) == 1;
        }
        
        public ErrorCode RequestData(string[] codeList)
        {
            string strCodes = string.Join(";", codeList);
            /*
            LONG CommKwRqData(LPCTSTR sArrCode, BOOL bNext, int nCodeCount, int nTypeFlag, LPCTSTR sRQName, LPCTSTR sScreenNo) 
            설명 복수종목조회 Tran을 서버로 송신한다. 
            입력값 
                sArrCode – 종목리스트 
                bNext – 연속조회요청 
                nCodeCount – 종목개수 
                nTypeFlag – 조회구분 
                sRQName – 사용자구분 명 
                sScreenNo – 화면번호[4] 
            반환값 
                OP_ERR_RQ_STRING – 요청 전문 작성 실패 
                OP_ERR_NONE - 정상처리 
            비고 
                sArrCode – 종목간 구분은 ‘;’이다. nTypeFlag – 0:주식관심종목정보, 3:선물옵션관심종목정보 ex) openApi.CommKwRqData(“000660;005930”, 0, 2, 0, “RQ_1”, “0101”); */
            return (ErrorCode)m_axKHOpenAPI.CommKwRqData(strCodes, 0, codeList.Length, 0, "RQName", SCR_NO_REQUEST_DATA);
        }

        private void M_axKHOpenAPI_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            log.Debug("errCode=" + e.nErrCode.ToString());
            Debug.WriteLine("KHOpenAPI_OnEventConnect=" + e.nErrCode.ToString());
            if (OnConnected != null)
                OnConnected(this);
        }
    }
}
