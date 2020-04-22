using NewUploadFile.Common;
using NewUploadFile.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace NewUploadFile.Controllers
{
    /// <summary>
    /// 最新整理上传接口
    /// </summary>
    [RoutePrefix("api/uploadfile")]
    public class UploadFileController : ApiController
    {
        [HttpPost]
        [Route("newuploadfile")]
        public IHttpActionResult NewUploadFile()
        {
            ResultMsg result = new ResultMsg();
            string[] pictureFormatArray = ConfigValue.UploadFormat.Split(',');
            try
            {
                //获取上传图片文件
                var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
                var fileImg = context.Request.Files[0];
                Stream userfile = fileImg.InputStream;//.InputStream;
                string suffix = Path.GetExtension(fileImg.FileName).ToLower();//获取文件扩展名(后缀)

                int UploadSize = int.Parse(ConfigValue.UploadFileSize);
                //判断文件大小不允许超过100Mb
                if (fileImg.ContentLength > (UploadSize * 1024 * 1024))
                {
                    result.ResultCode = -1;
                    result.ResultMsgs = "上传失败,文件大小超过100MB";
                    return Ok(result);
                }

                //检查文件后缀名
                if (!pictureFormatArray.Contains(suffix.TrimStart('.')))
                {
                    result.ResultCode = -1;
                    result.ResultMsgs = "上传失败,文件格式必须为" + ConfigValue.UploadFormat + "类型";
                    return Ok(result);
                }

                //如果不存在就创建file文件夹
                string SuffixName = suffix.TrimStart('.').ToLower();
                string FileFullPath = context.Server.MapPath(string.Format("~/upload/{0}//", SuffixName));
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

                string NewFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(99999); //重新处图片名称
                string NewFilePath = FileTime + NewFileName;//最新的图片路径(不带后缀的路径在SaveAs时必须添加后缀SuffixName)

                //Request.Files 获取表单中的文件
                if (context.Request.Files.Count > 0)
                {
                    for (int i = 0; i < context.Request.Files.Count; i++)
                    {
                        HttpPostedFileBase hpf = context.Request.Files[i];//这个对象是用来接收文件,利用这个对象可以获取文件的name path等
                        hpf.SaveAs(NewFilePath + "." + SuffixName);//用SaveAs保存到上面的路径中
                    }
                }

                if (SuffixName != "zip")
                {
                    ImageProcessMethod.MakeThumbnail(NewFilePath + "." + SuffixName, NewFilePath + "_small.jpeg", 150, 80);
                    //ImageProcessMethod.AddTextWatermark(NewFilePath + "." + SuffixName, NewFilePath + "_textseal.jpeg", "版权专用", 14.0f, 120, SuffixName);
                    ImageProcessMethod processMethod = new ImageProcessMethod();
                    bool bo = processMethod.AddTextToImg(NewFilePath + "." + SuffixName, NewFilePath + "_textseal.jpeg", "版权专用", 5, 0, 95, 100, -45, out string error);

                    //string watermarkImgPath = context.Server.MapPath(string.Format("~/Img/jpg/watermarkImg.png"));
                    //ImageProcessMethod.AddImgWatermark(NewFilePath + "." + SuffixName, NewFilePath + "_imgseal.jpeg", watermarkImgPath, 120, SuffixName);
                }

                //string uploadfileurl = System.Configuration.ConfigurationManager.AppSettings.Get("ServerImgaes");
                string retAddr = "/upload/" + SuffixName + "/" + DateTime.Now.ToString("yyyyMMdd") + "/" + NewFileName + "." + SuffixName;

                result.ResultCode = 1;
                result.ResultMsgs = "上传成功";
                result.ResultData = retAddr;

                ImgModel imgModel = new ImgModel();
                imgModel.ImgUrl = retAddr;

                return Ok(ReturnHelpMethod.ReturnSuccess(613, new { data = imgModel }));
            }
            catch (Exception ex)
            {
                WriteLog.WriteLogs(ex, "");
                result.ResultCode = -1;
                result.ResultMsgs = "上传失败";
                return Ok(result);
            }
        }
    }
}
