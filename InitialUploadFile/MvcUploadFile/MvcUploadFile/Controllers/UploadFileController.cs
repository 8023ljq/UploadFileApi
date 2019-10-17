using MvcUploadFile.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace MvcUploadFile.Controllers
{
    [RoutePrefix("api/uploadfile")]
    public class UploadFileController : ApiController
    {
        [HttpPost]
        [Route("NewUploadFile")]
        public IHttpActionResult NewUploadFile()
        {
            ResultMsg result = new ResultMsg();
            string[] pictureFormatArray = ConfigValue.UploadFormat.Split(',');
            try
            {
                //获取上传图片文件
                // 获取传统context
                var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
                var fileImg = HttpContext.Current.Request.Files[0];
                Stream userfile = fileImg.InputStream;//.InputStream;
                string ext1 = Path.GetExtension(fileImg.FileName).ToLower();//获取文件扩展名(后缀)

                int UploadSize = int.Parse(ConfigValue.UploadFileSize);
                //判断文件大小不允许超过100Mb
                if (fileImg.ContentLength > (UploadSize * 1024 * 1024))
                {
                    result.ResultCode = -1;
                    result.ResultMsgs = "上传失败,文件大小超过100MB";
                    return Ok(result);
                }

                //检查文件后缀名
                if (!pictureFormatArray.Contains(ext1.TrimStart('.')))
                {
                    result.ResultCode = -1;
                    result.ResultMsgs = "上传失败,文件格式必须为" + ConfigValue.UploadFormat + "类型";
                    return Ok(result);
                }
                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, ConfigValue.ServerImgaes + "/Upload/UploadImgageFromWeb?ImgPathEnum=" + 1 + "&IsFullPath=false" + "&ext=" + ext1.Substring(1));
                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(userfile), "file", "file.jpg");
                    //content.Add(new StreamContent(HttpContext.Request.Form.Files[0].OpenReadStream()), "file", "file.jpg");
                    request.Content = content;

                    var response = client.SendAsync(request).Result;
                    response.EnsureSuccessStatusCode();
                    var filenamestr =response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ResultMsg>(filenamestr.Result);
                    return Ok(result);
                }
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
