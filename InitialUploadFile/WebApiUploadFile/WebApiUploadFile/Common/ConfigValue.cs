using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUploadFile.Common
{
    public class ConfigValue
    {
        /// <summary>
        /// 图片上传格式
        /// </summary>
        public static string UploadFormat = System.Configuration.ConfigurationManager.AppSettings["UploadFormat"];

        /// <summary>
        /// 图片上传大小
        /// </summary>
        public static string UploadFileSize = System.Configuration.ConfigurationManager.AppSettings["UploadFileSize"];

        /// <summary>
        /// 图片上传地址
        /// </summary>
        public static string ServerImgaes = System.Configuration.ConfigurationManager.AppSettings["ServerImgaes"];
    }
}