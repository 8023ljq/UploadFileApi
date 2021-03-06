﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUploadFile.Common
{
    public class ResultMsg
    {
        public ResultMsg()
        {
            ResultData = new { };
            ResultMsgs = string.Empty;
        }
        /// <summary>
        /// 状态码
        /// </summary>
        public int ResultCode { get; set; }
        /// <summary>
        /// 操作信息
        /// </summary>
        public string ResultMsgs { get; set; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public object ResultData { get; set; }
    }
}