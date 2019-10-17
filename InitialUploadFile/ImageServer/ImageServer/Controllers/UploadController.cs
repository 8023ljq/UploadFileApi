using ImageServer.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ImageServer.Controllers
{
    public class UploadController : ApiController
    {
        public UploadController()
        {

        }

        [Route("api/Upload/Index")]
        public IHttpActionResult Index()
        {
            return Ok("网络OK");
        }
        /// <summary>
        /// 图片上传
        /// </summary>
        /// <param name="ImgPathEnum"></param>
        /// <param name="IsFullPath">是否完整路径</param>
        /// <param name="ext"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Upload/UploadImgageFromWeb")]
        public IHttpActionResult UploadImgageFromWeb(string ext)
        {
            ResultMsg resultMsg = new ResultMsg();
            //获取配置文件
            string UploadFormat = System.Configuration.ConfigurationManager.AppSettings["UploadFormat"];
            string UploadFileSize = System.Configuration.ConfigurationManager.AppSettings["UploadFileSize"];

            // 获取传统context
            var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            //图片名
            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(99999);

            if (string.IsNullOrEmpty(ext))
            {
                resultMsg.ResultCode = -1;
                resultMsg.ResultMsgs = "上传失败,文件格式必须为" + UploadFormat + "类型";
                return Ok(resultMsg);
            }
            ext = ext.ToLower();

            //检查文件格式
            string[] extArr = UploadFormat.Split(',');
            if (string.IsNullOrEmpty(ext) || !extArr.Contains(ext))
            {
                // HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent("文件格式不支持", Encoding.GetEncoding("UTF-8"), "text/plain") };
                resultMsg.ResultCode = -1;
                resultMsg.ResultMsgs = "上传失败,文件格式必须为" + UploadFormat + "类型";
                return Ok(resultMsg);
            }

            try
            {
                string FileFullPath = context.Server.MapPath(string.Format("~/upload/{0}//", ext));

                //测试代码
                //string waterPath = "E:/Demo/接口/ImageServer/ImageServer/img/jpg/20181117/tz.png";

                //如果不存在就创建file文件夹
                if (!Directory.Exists(FileFullPath))
                {
                    Directory.CreateDirectory(FileFullPath);
                }
                //不同时间上传文件
                string FileTime = FileFullPath + DateTime.Now.ToString("yyyyMMdd") + $@"\";
                if (!Directory.Exists(FileTime))
                {
                    Directory.CreateDirectory(FileTime);
                }

                var filePath = FileTime + FileName + "." + ext;

                if (context.Request.Files.Count > 0)  //Request.Files 获取表单中的文件
                {
                    for (int i = 0; i < context.Request.Files.Count; i++)
                    {
                        HttpPostedFileBase hpf = context.Request.Files[i];//这个对象是用来接收文件,利用这个对象可以获取文件的name path等
                        hpf.SaveAs(filePath);//用SaveAs保存到上面的路径中
                    }
                }
                if (ext != "zip")
                {
                    MakeThumbnail(filePath, filePath + "_small.jpeg", 150, 80);
                    NewAddWaterMark.AddTextWatermark(filePath, filePath + "_seal.jpeg", "版权专用", 14.0f, 120, ext);
                }
                string uploadfileurl = System.Configuration.ConfigurationManager.AppSettings.Get("ServerImgaes");
                string retAddr = "/upload/" + ext + "/" + DateTime.Now.ToString("yyyyMMdd") + "/" + FileName + "." + ext;

                resultMsg.ResultCode = 1;
                resultMsg.ResultMsgs = "上传成功";
                resultMsg.ResultData = retAddr;
                return Ok(resultMsg);
            }
            catch (Exception ex)
            {
                WriteLog.WriteLogs(ex);
                resultMsg.ResultCode = -1;
                resultMsg.ResultMsgs = "上传失败";
                return Ok(resultMsg);
            }
        }

        /// <summary>
        /// 图片上传
        /// </summary>
        /// <param name="ImgPathEnum"></param>
        /// <param name="IsFullPath"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        private HttpResponseMessage UploadImgageFromAdmin(HttpContextBase context, int ImgPathEnum, bool IsFullPath, string ext)
        {
            // 获取传统context
            //context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            Stream userfile = context.Request.InputStream;
            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(99999);
            string FileExt = context.Request.Headers["fileext"];
            if (!string.IsNullOrEmpty(FileExt))
            {
                FileExt = FileExt.ToLower();
            }
            else
            {
                FileExt = ext;
            }
            string[] extArr = new string[] { "gif", "jpg", "jpeg", "png", "ico" };
            if (string.IsNullOrEmpty(FileExt) || !extArr.Contains(FileExt))
            {
                HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent("图片格式不支持", Encoding.GetEncoding("UTF-8"), "text/plain") };
                return result;
            }
            string uploadfolder = context.Server.MapPath(string.Format("~/img/{0}/", ImgPathEnum));
            string uploadfileurl = System.Configuration.ConfigurationManager.AppSettings.Get("ServerImgaes");
            if (!Directory.Exists(uploadfolder))
            {
                Directory.CreateDirectory(uploadfolder);
            }
            FileExt = "." + FileExt;
            string retAddr = "";
            try
            {
                byte[] buffer = new byte[256];
                int len;
                string FileFullPath = uploadfolder + FileName + FileExt;
                FileStream fs = new FileStream(FileFullPath, FileMode.Create, FileAccess.Write);
                while ((len = userfile.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, len);
                }
                fs.Close();

                if (IsFullPath)
                {
                    retAddr = uploadfileurl + ImgPathEnum + "/" + FileName + FileExt;
                }
                else
                {
                    retAddr = FileName + FileExt;
                }
                MakeThumbnail(FileFullPath, FileFullPath + "_small.jpeg", 150, 80);
                NewAddWaterMark.AddTextWatermark(FileFullPath, FileFullPath + "_seal.jpeg", "版权专用", 14, 120, ext);
                HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(retAddr, Encoding.GetEncoding("UTF-8"), "text/plain") };
                return result;
            }
            catch (Exception ex)
            {
                HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent("上传出错：" + ex.Message, Encoding.GetEncoding("UTF-8"), "text/plain") };
                return result;
            }
        }

        /// <summary>
        /// 生成图片缩略图
        /// </summary>
        /// <param name="sourcePath">图片路径</param>
        /// <param name="newPath">新图片路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        private static void MakeThumbnail(string sourcePath, string newPath, int width, int height)
        {
            Image ig = Image.FromFile(sourcePath);
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = ig.Width;
            int oh = ig.Height;
            if ((double)ig.Width / (double)ig.Height > (double)towidth / (double)toheight)
            {
                oh = ig.Height;
                ow = ig.Height * towidth / toheight;
                y = 0;
                x = (ig.Width - ow) / 2;

            }
            else
            {
                ow = ig.Width;
                oh = ig.Width * height / towidth;
                x = 0;
                y = (ig.Height - oh) / 2;
            }
            Image bitmap = new Bitmap(towidth, toheight);
            Graphics g = Graphics.FromImage(bitmap);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(ig, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);
            try
            {
                bitmap.Save(newPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ig.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        /// <summary>
        /// 上传编辑图片
        /// </summary>
        /// <param name="ImgPathEnum"></param>
        /// <param name="IsFullPath"></param>
        /// <returns></returns>
        //[HttpPost]
        //[HttpGet]
        //public HttpResponseMessage UploadEditImgage(EnumCommon ImgPathEnum, bool IsFullPath)
        //{
        //    try
        //    {
        //        // 获取传统context
        //        var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
        //        string ext = context.Request.Files[0].FileName;
        //        ext = Path.GetExtension(ext);
        //        return UploadImgageFromWeb((int)ImgPathEnum, IsFullPath, ext);
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent("上传出错：" + ex.Message, Encoding.GetEncoding("UTF-8"), "text/plain") };
        //        return result;
        //    }
        //}

        //[HttpPost]
        //[HttpGet]
        //public HttpResponseMessage UploadEditImgage(EnumCommon ImgPathEnum, bool IsFullPath, string ext)
        //{
        //    try
        //    {
        //        return UploadImgageFromWeb((int)ImgPathEnum, IsFullPath, ext);
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent("上传出错：" + ex.Message, Encoding.GetEncoding("UTF-8"), "text/plain") };
        //        return result;
        //    }
        //}

        /// <summary>
        /// ios图片上传 
        /// </summary> 
        /// <returns>成功上传返回上传后的文件名</returns>
        //[HttpPost]
        //public IHttpActionResult UpLoadImage()
        //{
        //    var resultMsg = new ResultMsg
        //    {
        //        statue = 0
        //    };
        //    try
        //    {
        //        // 获取传统context
        //        var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
        //        string FileFullPath = context.Server.MapPath(string.Format("~/img/{0}//", (int)EnumCommon.Img_VerifyIDCard));

        //        var path = FileFullPath;
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }

        //        var files = context.Request.Files;
        //        if (files.AllKeys.Any())
        //        {
        //            if (context.Request.Files.Count > 0)  //Request.Files 获取表单中的文件
        //            {
        //                for (int i = 0; i < context.Request.Files.Count; i++)
        //                {
        //                    HttpPostedFileBase hpf = context.Request.Files[i];//这个对象是用来接收文件,利用这个对象可以获取文件的name path等
        //                    hpf.SaveAs(FileFullPath);//用SaveAs保存到上面的路径中

        //                    var fileInfo = new FileInfo(FileFullPath + context.Request.Files[i].FileName);
        //                    var ext = Path.GetExtension(context.Request.Files[i].FileName);
        //                    var newfileName = Guid.NewGuid() + ext;
        //                    var newName = path + newfileName;
        //                    fileInfo.MoveTo(newName);

        //                    resultMsg.statue = Convert.ToInt32(1);
        //                    resultMsg.ResultMsgs = "OK";
        //                    resultMsg.data = new
        //                    {
        //                        fileName = newfileName,
        //                    };

        //                }
        //            }
        //        }
        //        else
        //        {
        //            resultMsg.ResultMsgs = "Fail";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resultMsg.statue = Convert.ToInt32(0);
        //        resultMsg.ResultMsgs = "Fail:" + ex.Message;
        //    }
        //    return Ok(resultMsg);
        //}

        /// <summary>
        /// Android图片上传 
        /// </summary> 
        /// <returns>成功上传返回上传后的文件名</returns>
        //[HttpPost]
        //public IHttpActionResult AndroidUpLoadImage()
        //{
        //    var resultMsg = new ResultMsg
        //    {
        //        statue = Convert.ToInt32(0)
        //    };
        //    try
        //    {
        //        // 获取传统context
        //        var context = (HttpContextBase)Request.Properties["MS_HttpContext"];

        //        var files = context.Request.Form;
        //        if (files.Count > 0)
        //        {
        //            var base64Code = files["Filedata"];
        //            string FileFullPath = context.Server.MapPath(string.Format("~/img/{0}//", (int)EnumCommon.Img_VerifyIDCard));
        //            var path = FileFullPath;
        //            if (!Directory.Exists(path))
        //            {
        //                Directory.CreateDirectory(path);
        //            }
        //            string ext = string.Empty;
        //            if (base64Code.Contains("data:image/jpeg;base64,"))
        //            {
        //                ext = ".jpeg";
        //                base64Code = base64Code.Substring(23);
        //            }
        //            else if (base64Code.Contains("data:image/png;base64,"))
        //            {
        //                ext = ".png";
        //                base64Code = base64Code.Substring(22);
        //            }
        //            else if (base64Code.Contains("data:image/jpg;base64,"))
        //            {
        //                ext = ".jpg";
        //                base64Code = base64Code.Substring(22);
        //            }
        //            var bytes = Convert.FromBase64String(base64Code);
        //            var ms = new MemoryStream(bytes);
        //            var newfilename = Guid.NewGuid().ToString("N") + ext;
        //            new Bitmap(ms).Save(path + newfilename);
        //            resultMsg.statue = Convert.ToInt32(1);
        //            resultMsg.ResultMsgs = "OK";
        //            resultMsg.data = new
        //            {
        //                filename = newfilename,
        //            };
        //        }
        //        else
        //        {
        //            resultMsg.ResultMsgs = "Fail";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resultMsg.statue = Convert.ToInt32(0);
        //        resultMsg.ResultMsgs = "Fail" + ex.Message;
        //    }
        //    return Ok(resultMsg);
        //}
    }



    /// <summary>
    /// 返回类
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
        /// 操作信息
        /// </summary>
        public string ResultMsgs { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public object ResultData { get; set; }

    }

    public enum EnumCommon
    {
        #region 图片路径枚举
        /// <summary>
        ///区域图片
        /// </summary>

        Img_Area = 1,
        /// <summary>
        /// 菜单图片
        /// </summary>

        Img_Menu = 2,
        /// <summary>
        /// 设备结构图
        /// </summary>

        Img_EquipmentStructure = 3,
        /// <summary>
        /// 设备类型图
        /// </summary>

        Img_DeviceType = 4,
        /// <summary>
        /// 设备状态图
        /// </summary>

        Img_DeviceState = 5,
        /// <summary>
        /// 身份证认证图
        /// </summary>

        Img_VerifyIDCard = 6,
        /// <summary>
        /// 文章上传图片
        /// </summary>

        Img_ArticleUpload = 7,
        #endregion

        #region 展示类型
        /// <summary>
        /// 电子地图
        /// </summary>

        Display_Map = 101,
        /// <summary>
        /// 设备列表
        /// </summary>

        Display_List = 102,
        /// <summary>
        /// 结构图
        /// </summary>

        Display_Structure = 103,
        #endregion
    }
}
