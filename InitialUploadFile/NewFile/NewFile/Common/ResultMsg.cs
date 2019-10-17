using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewFile.Common
{
    public class ResultMsg
    {
        public ResultMsg()
        {
            data = new { };
            msg = string.Empty;
        }
        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 操作信息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public object data { get; set; }
    }
}
