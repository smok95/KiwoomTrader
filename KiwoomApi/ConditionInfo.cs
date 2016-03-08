using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kiwoom
{
    /// <summary>
    /// 조건검색정보
    /// </summary>
    public class ConditionInfo
    {
        /// <summary>
        /// 인덱스값
        /// </summary>
        public int Index
        {
            get;
            protected set;
        }

        /// <summary>
        /// 조건검색식 이름
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }

        public ConditionInfo(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
