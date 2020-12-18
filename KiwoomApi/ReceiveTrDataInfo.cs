using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiwoom
{
    /// <summary>
    /// OnReceiveTrData 이벤트 수신값 정보
    /// </summary>
    public class ReceiveTrDataInfo
    {
        /// <summary>
        /// 화면번호
        /// </summary>
        public readonly string sScrNo;
        /// <summary>
        /// 사용자 구분명
        /// </summary>
        public readonly string sRQName;
        /// <summary>
        /// TR이름
        /// </summary>
        public readonly string sTrCode;
        /// <summary>
        /// 레코드이름
        /// </summary>
        public readonly string sRecordName;
        /// <summary>
        /// 연속조회 유무, 0:연속데이터 없음, 2:연속데이터 있음
        /// </summary>
        public readonly string sPrevNext;

        /// <summary>
        /// 수신된 데이터 개수
        /// </summary>
        public readonly int nDataLength;

        public ReceiveTrDataInfo(string scrNo, string rqName, string trCode, string recordName, string prevNext, int dataLength)
        {
            this.sScrNo = scrNo;
            this.sRQName = rqName;
            this.sTrCode = trCode;
            this.sRecordName = recordName;
            this.sPrevNext = prevNext;
            this.nDataLength = dataLength;
        }
    }
}
