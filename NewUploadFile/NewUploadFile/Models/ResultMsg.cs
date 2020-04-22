namespace NewUploadFile.Models
{
    /// <summary>
    /// 返回参数类
    /// </summary>
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
        /// 状态码
        /// </summary>
        public string ResultType { get; set; }

        /// <summary>
        /// 操作信息
        /// </summary>
        public string ResultMsgs { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public object ResultData { get; set; }
    }
}