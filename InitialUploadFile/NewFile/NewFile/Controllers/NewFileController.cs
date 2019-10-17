using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewFile.Common;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NewFile.Controllers
{
    public class NewFileController : Controller
    {
        // GET: /<controller>/
        [HttpPost]
        [Route("api/newfile/uploadfile")]
        public async Task<IActionResult> UploadFile()
        {
            ResultMsg result = new ResultMsg();
            string[] pictureFormatArray = ConfigValue.UploadFormat.Split(',');
            try
            {
                //获取上传图片文件
                var fileImg = HttpContext.Request.Form.Files[0];
                Stream userfile = fileImg.OpenReadStream();//.InputStream;
                string ext1 = Path.GetExtension(fileImg.FileName).ToLower();//获取文件扩展名(后缀)

                int UploadSize = int.Parse(ConfigValue.UploadFileSize);
                //判断文件大小不允许超过100Mb
                if (fileImg.Length > (UploadSize * 1024 * 1024))
                {
                    result.code = -1;
                    result.msg = "上传失败,文件大小超过100MB";
                    return Ok(result);
                }

                //检查文件后缀名
                if (!pictureFormatArray.Contains(ext1.TrimStart('.')))
                {
                    result.code = -1;
                    result.msg = "上传失败,文件格式必须为" + ConfigValue.UploadFormat + "类型";
                    return Ok(result);
                }
                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, ConfigValue.ServerImgaes + "/Upload/UploadImgageFromWeb?ImgPathEnum=" + 1 + "&IsFullPath=false" + "&ext=" + ext1.Substring(1));
                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(userfile), "file", "file.jpg");
                    //content.Add(new StreamContent(HttpContext.Request.Form.Files[0].OpenReadStream()), "file", "file.jpg");
                    request.Content = content;

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var filenamestr = await response.Content.ReadAsStringAsync();

                    result = JsonConvert.DeserializeObject<ResultMsg>(filenamestr);
                    //result.code = 1;
                    //result.msg = "上传成功";
                    //result.data = filenamestr;
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteLogs(ex, "");
                result.code = -1;
                result.msg = "上传失败";
                return Ok(result);
            }
        }
    }
}
