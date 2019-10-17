using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace NewFile.Common
{
    /// <summary>
    /// 获取配置信息
    /// </summary>
    public class ConfigValue {
        /// <summary>
        /// 上传格式
        /// </summary>
        public static string UploadFormat= CoinAppSettings.Instance.AppSettings.UploadFormat;

        /// <summary>
        /// 上传文件大小
        /// </summary>
        public static string UploadFileSize = CoinAppSettings.Instance.AppSettings.UploadFileSize;

        /// <summary>
        /// 上传文件路径
        /// </summary>
        public static string UploadFilePath = CoinAppSettings.Instance.AppSettings.UploadFilePath;
        /// <summary>
        /// 上传文件路径
        /// </summary>
        public static string ServerImgaes = CoinAppSettings.Instance.AppSettings.ServerImgaes;
        /// <summary>
        /// 上传文件路径
        /// </summary>
        public static string SaveToImgaes = CoinAppSettings.Instance.AppSettings.SaveToImgaes;
    }


    public class CoinAppSettings
    {
        public static CoinAppSettings Instance { get; private set; }

        public AppSettings AppSettings { get; }

        public DbConnection ConnectionStrings { get; }

        public static void CreateInstence(IConfigurationRoot builder)
        {
            Instance = new CoinAppSettings(builder);
        }

        public CoinAppSettings(IConfigurationRoot builder)
        {
            this.ConnectionStrings = new DbConnection(builder.GetSection("ConnectionStrings"));
            this.AppSettings = new AppSettings(builder.GetSection("AppSettings"));
        }
    }

    /// <summary>
    /// 链接配置
    /// </summary>
    public class DbConnection
    {
        /// <summary>
        /// 读数据库
        /// </summary>
        public string DapperRead { get; }
        /// <summary>
        /// 写数据库
        /// </summary>
        public string DapperWrite { get; }
        /// <summary>
        /// redis链接
        /// </summary>
        public string RedisConnMain { get; }
        public string RedisConnVice { get; }
        public string RabbitMqHostName { get; }
        public string RabbitMqUserName { get; }
        public string RabbitMqPassword { get; }
        public string RedisConnSignalr { get; }
        public DbConnection(IConfigurationSection section)
        {
            this.DapperRead = section.GetSection("DapperRead").Value;
            this.DapperWrite = section.GetSection("DapperWrite").Value;
            this.RedisConnMain = section.GetSection("RedisConnMain").Value;
            this.RedisConnVice = section.GetSection("RedisConnVice").Value;
            this.RabbitMqHostName = section.GetSection("RabbitMqHostName").Value;
            this.RabbitMqUserName = section.GetSection("RabbitMqUserName").Value;
            this.RabbitMqPassword = section.GetSection("RabbitMqPassword").Value;
            this.RedisConnSignalr = section.GetSection("RedisConnSignalr").Value;
        }
    }

    /// <summary>
    /// 常量配置
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 上传格式
        /// </summary>
        public string UploadFormat { get; }
       
        /// <summary>
        /// 上传文件大小
        /// </summary>
        public string UploadFileSize { get; }

        /// <summary>
        /// 上传文件路径
        /// </summary>
        public string UploadFilePath { get; }
        /// <summary>
        /// 上传文件路径
        /// </summary>
        public string ServerImgaes { get; }
        /// <summary>
        /// 上传文件路径
        /// </summary>
        public string SaveToImgaes { get; }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="section"></param>
        public AppSettings(IConfigurationSection section)
        {
            this.UploadFormat = section.GetSection("UploadFormat").Value;
            this.UploadFileSize = section.GetSection("UploadFileSize").Value;
            this.UploadFilePath = section.GetSection("UploadFilePath").Value;
            this.ServerImgaes = section.GetSection("ServerImgaes").Value;
            this.SaveToImgaes = section.GetSection("SaveToImgaes").Value;
        }
    }
}
