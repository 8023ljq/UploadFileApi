using ImageServer.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace ImageServer.Controllers
{
    public class AddWaterMark
    {
        /// <summary>
        /// 添加文字水印
        /// </summary>
        /// <param name="imgPath">原图片地址</param>
        /// <param name="sImgPath">水印图片地址</param>
        /// <returns>resMsg[0] 成功,失败 </returns>
        public static void AddWaterMarks(string imgPath, string sImgPath)
        {
            using (Image image = Image.FromFile(imgPath))
            {
                try
                {
                    Bitmap bitmap = new Bitmap(image);

                    int width = bitmap.Width, height = bitmap.Height;
                    //水印文字
                    string text = "版权保密";

                    Graphics g = Graphics.FromImage(bitmap);

                    g.DrawImage(bitmap, 0, 0);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    g.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);

                    Font crFont = new Font("微软雅黑", 20, FontStyle.Bold);
                    SizeF crSize = new SizeF();
                    crSize = g.MeasureString(text, crFont);

                    //背景位置(去掉了. 如果想用可以自己调一调 位置.)
                    //g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), (width - crSize.Width) / 2, (height - crSize.Height) / 2, crSize.Width, crSize.Height);

                    //获取绘画水印文字的格式,即文字对齐方式
                    //StringFormat format = GetStringFormat(5);

                    //处理水印的颜色
                    SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(120, 177, 171, 171));

                    //将原点移动 到图片中点
                    g.TranslateTransform(width / 2, height / 2);
                    //以原点为中心 转 -45度
                    g.RotateTransform(-45);

                    g.DrawString(text, crFont, semiTransBrush, new PointF(0, 0));

                    //保存文件
                    bitmap.Save(sImgPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                }
                catch (Exception ex)
                {
                    WriteLog.WriteLogs(ex);
                }
            }
        }


        /// <summary>
        /// 给图片加入图片水印,且设置水印透明度，旋转角度
        /// </summary>
        /// <param name="destPath">保存地址</param>
        /// <param name="srcPath">源文件地址，如果想覆盖源图片，两个地址参数一定要一样</param>
        /// <param name="waterPath">水印图片地址</param>      
        /// <param name="pos">设置水印位置，1左上，2中上，3右上
        ///                                 4左中，5中，  6右中
        ///                                 7左下，8中下，9右下</param>
        /// <param name="padding">跟css里的padding一个意思</param>
        /// <param name="quality">1~100整数,无效值，则取默认值95</param>
        /// <param name="opcity">不透明度  100 为完全不透明，0为完全透明</param>
        /// <param name="angle">顺时针旋转角度</param>
        /// <param name="error"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static void DrawWaterImage(string destPath, string srcPath, string waterPath,  out string error, string mimeType = "image/jpeg")
        {
            error = string.Empty;
            Image srcImage = null;
            Image waterImage = null;
            Image destImage = Image.FromFile(srcPath);
            Graphics graphics = null;
            try
            {
                int pos = 5, padding = 10, quality = 95, opcity = 60, angle = 45;
                //获取原图
                srcImage = Image.FromFile(srcPath, false);
                //获取水印图片
                waterImage = Image.FromFile(waterPath, false);
                var waterRect = new Rectangle(0, 0, waterImage.Width, waterImage.Height);
                //定义画布
                destImage = new Bitmap(srcImage);
                //获取高清Graphics
                graphics = GetGraphics(destImage);
                //将源图画到画布上
                graphics.DrawImage(srcImage, new Rectangle(0, 0, destImage.Width, destImage.Height), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                //不透明度大于0，则画水印
                if (opcity > 0)
                {
                    //获取可以用来绘制水印图片的有效区域
                    Rectangle validRect = new Rectangle(padding, padding, srcImage.Width - padding * 2, srcImage.Height - padding * 2);
                    //如果要进行旋转
                    if (angle != 0)
                    {
                        Image rotateImage = null;
                        try
                        {
                            //获取水印图像旋转后的图像
                            rotateImage = GetRotateImage(waterImage, angle);
                            if (rotateImage != null)
                            {
                                //旋转后图像的矩形区域
                                var rotateRect = new Rectangle(0, 0, rotateImage.Width, rotateImage.Height);
                                //计算水印图片的绘制位置
                                var destRect = GetRectangleByPostion(validRect, rotateRect, pos);
                                //如果不透明度>=100,那么直接将水印画到当前画布上.
                                if (opcity == 100)
                                {
                                    graphics.DrawImage(rotateImage, destRect, rotateRect, GraphicsUnit.Pixel);
                                }
                                else
                                {
                                    //如果不透明度在0到100之间，设置透明参数
                                    ImageAttributes imageAtt = GetAlphaImgAttr(opcity);
                                    //将旋转后的图片画到画布上
                                    graphics.DrawImage(rotateImage, destRect, 0, 0, rotateRect.Width, rotateRect.Height, GraphicsUnit.Pixel, imageAtt);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog.WriteLogs(ex);
                            error = ex.Message;
                        }
                        finally
                        {
                            if (rotateImage != null)
                                rotateImage.Dispose();
                        }
                    }
                    else
                    {
                        //计算水印图片的绘制位置
                        var destRect = GetRectangleByPostion(validRect, waterRect, pos);
                        //如果不透明度=100,那么直接将水印画到当前画布上.
                        if (opcity == 100)
                        {
                            graphics.DrawImage(waterImage, destRect, waterRect, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            //如果不透明度在0到100之间，设置透明参数
                            ImageAttributes imageAtt = GetAlphaImgAttr(opcity);
                            //将水印图片画到画布上
                            graphics.DrawImage(waterImage, destRect, 0, 0, waterRect.Width, waterRect.Height, GraphicsUnit.Pixel, imageAtt);

                        }
                    }

                }
                //如果两个地址相同即覆盖，则提前Dispose源资源
                if (destPath.ToLower() == srcPath.ToLower())
                {
                    srcImage.Dispose();
                }
                SaveImage2File(destPath, destImage, quality, mimeType);
            }
            catch (Exception ex)
            {
                WriteLog.WriteLogs(ex);
                error = ex.Message;
            }
            finally
            {
                if (srcImage != null)
                    srcImage.Dispose();
                if (destImage != null)
                    destImage.Dispose();
                if (graphics != null)
                    graphics.Dispose();
                if (waterImage != null)
                    waterImage.Dispose();
            }
        }


        /// <summary>
        /// 将Image实例保存到文件,注意此方法不执行 img.Dispose()
        /// 图片保存时本可以直接使用destImage.Save(path, ImageFormat.Jpeg)，但是这种方法无法进行进一步控制图片质量
        /// </summary>
        /// <param name="path"></param>
        /// <param name="img"></param>
        /// <param name="quality">1~100整数,无效值，则取默认值95</param>
        /// <param name="mimeType"></param>
        public static void SaveImage2File(string path, Image destImage, int quality, string mimeType = "image/jpeg")
        {
            if (quality <= 0 || quality > 100) quality = 95;
            //创建保存的文件夹
            FileInfo fileInfo = new FileInfo(path);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
            //设置保存参数，保存参数里进一步控制质量
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qua = new long[] { quality };
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            //获取指定mimeType的mimeType的ImageCodecInfo
            var codecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ici => ici.MimeType == mimeType);
            destImage.Save(path, codecInfo, encoderParams);
        }

        /// <summary>
        /// 获取高清的Graphics
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Graphics GetGraphics(Image img)
        {
            var g = Graphics.FromImage(img);
            //设置质量
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            //InterpolationMode不能使用High或者HighQualityBicubic,如果是灰色或者部分浅色的图像是会在边缘处出一白色透明的线
            //用HighQualityBilinear却会使图片比其他两种模式模糊（需要肉眼仔细对比才可以看出）
            g.InterpolationMode = InterpolationMode.Default;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            return g;
        }

        /// <summary>
        /// 计算矩形绕中心任意角度旋转后所占区域矩形宽高
        /// </summary>
        /// <param name="width">原矩形的宽</param>
        /// <param name="height">原矩形高</param>
        /// <param name="angle">顺时针旋转角度</param>
        /// <returns></returns>
        public static Rectangle GetRotateRectangle(int width, int height, float angle)
        {
            double radian = angle * Math.PI / 180; ;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);
            //只需要考虑到第四象限和第三象限的情况取大值(中间用绝对值就可以包括第一和第二象限)
            int newWidth = (int)(Math.Max(Math.Abs(width * cos - height * sin), Math.Abs(width * cos + height * sin)));
            int newHeight = (int)(Math.Max(Math.Abs(width * sin - height * cos), Math.Abs(width * sin + height * cos)));
            return new Rectangle(0, 0, newWidth, newHeight);
        }   


        /// <summary>
        /// 获取原图像绕中心任意角度旋转后的图像
        /// </summary>
        /// <param name="rawImg"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Image GetRotateImage(Image srcImage, int angle)
        {
            angle = angle % 360;
            //原图的宽和高
            int srcWidth = srcImage.Width;
            int srcHeight = srcImage.Height;
            //图像旋转之后所占区域宽和高
            Rectangle rotateRec = GetRotateRectangle(srcWidth, srcHeight, angle);
            int rotateWidth = rotateRec.Width;
            int rotateHeight = rotateRec.Height;
            //目标位图
            Bitmap destImage = null;
            Graphics graphics = null;
            try
            {
                //定义画布，宽高为图像旋转后的宽高
                destImage = new Bitmap(rotateWidth, rotateHeight);
                //graphics根据destImage创建，因此其原点此时在destImage左上角
                graphics = Graphics.FromImage(destImage);
                //要让graphics围绕某矩形中心点旋转N度，分三步
                //第一步，将graphics坐标原点移到矩形中心点,假设其中点坐标（x,y）
                //第二步，graphics旋转相应的角度(沿当前原点)
                //第三步，移回（-x,-y）
                //获取画布中心点
                Point centerPoint = new Point(rotateWidth / 2, rotateHeight / 2);
                //将graphics坐标原点移到中心点
                graphics.TranslateTransform(centerPoint.X, centerPoint.Y);
                //graphics旋转相应的角度(绕当前原点)
                graphics.RotateTransform(angle);
                //恢复graphics在水平和垂直方向的平移(沿当前原点)
                graphics.TranslateTransform(-centerPoint.X, -centerPoint.Y);
                //此时已经完成了graphics的旋转

                //计算:如果要将源图像画到画布上且中心与画布中心重合，需要的偏移量
                Point Offset = new Point((rotateWidth - srcWidth) / 2, (rotateHeight - srcHeight) / 2);
                //将源图片画到rect里（rotateRec的中心）
                graphics.DrawImage(srcImage, new Rectangle(Offset.X, Offset.Y, srcWidth, srcHeight));
                //重至绘图的所有变换
                graphics.ResetTransform();
                graphics.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (graphics != null)
                    graphics.Dispose();
            }
            return destImage;
        }


        /// <summary>
        /// 获取图片水印位置，及small在big里的位置
        /// 如果small的高度大于big的高度，返回big的高度
        /// 如果small的宽度大于big的宽度，返回big的宽度
        /// </summary>
        /// <param name="pos">
        ///         1左上，2中上，3右上
        ///         4左中，5中，  6右中
        ///         7左下，8中下，9右下
        /// </param>
        /// <returns></returns>
        public static Rectangle GetRectangleByPostion(Rectangle big, Rectangle small, int pos)
        {
            if (big.Width < small.Width)
            {
                small.Width = big.Width;
            }
            if (big.Height < small.Height)
            {
                small.Height = big.Height;
            }
            Rectangle retVal = small;
            switch (pos)
            {
                case 1: retVal.X = 0; retVal.Y = 0; break;
                case 2: retVal.X = (big.Width - small.Width) / 2; retVal.Y = 0; break;
                case 3: retVal.X = big.Width - small.Width; retVal.Y = 0; break;
                case 4: retVal.X = 0; retVal.Y = (big.Height - small.Height) / 2; break;
                case 6: retVal.X = big.Width - small.Width; retVal.Y = (big.Height - small.Height) / 2; break;
                case 7: retVal.X = 0; retVal.Y = big.Height - small.Height; break;
                case 8: retVal.X = (big.Width - small.Width) / 2; retVal.Y = big.Height - small.Height; break;
                case 9: retVal.X = big.Width - small.Width; retVal.Y = big.Height - small.Height; break;
                default: retVal.X = (big.Width - small.Width) / 2; retVal.Y = (big.Height - small.Height) / 2; break;
            }
            retVal.X += big.X;
            retVal.Y += big.Y;
            return retVal;
        }

        /// <summary>
        /// 获取一个带有透明度的ImageAttributes
        /// </summary>
        /// <param name="opcity"></param>
        /// <returns></returns>
        public static ImageAttributes GetAlphaImgAttr(int opcity)
        {
            if (opcity < 0 || opcity > 100)
            {
                throw new ArgumentOutOfRangeException("opcity 值为 0~100");
            }
            //颜色矩阵
            float[][] matrixItems =
            {
                new float[]{1,0,0,0,0},
                new float[]{0,1,0,0,0},
                new float[]{0,0,1,0,0},
                new float[]{0,0,0,(float)opcity / 100,0},
                new float[]{0,0,0,0,1}
            };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
            ImageAttributes imageAtt = new ImageAttributes();
            imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            return imageAtt;
        }

        public static StringFormat GetStringFormat(int pos)
        {
            StringFormat format = new StringFormat();
            switch (pos)
            {
                case 1: format.Alignment = StringAlignment.Near; format.LineAlignment = StringAlignment.Near; break;
                case 2: format.Alignment = StringAlignment.Center; format.LineAlignment = StringAlignment.Near; break;
                case 3: format.Alignment = StringAlignment.Far; format.LineAlignment = StringAlignment.Near; break;
                case 4: format.Alignment = StringAlignment.Near; format.LineAlignment = StringAlignment.Center; break;
                case 6: format.Alignment = StringAlignment.Far; format.LineAlignment = StringAlignment.Center; break;
                case 7: format.Alignment = StringAlignment.Near; format.LineAlignment = StringAlignment.Far; break;
                case 8: format.Alignment = StringAlignment.Center; format.LineAlignment = StringAlignment.Far; break;
                case 9: format.Alignment = StringAlignment.Far; format.LineAlignment = StringAlignment.Far; break;
                default: format.Alignment = StringAlignment.Center; format.LineAlignment = StringAlignment.Center; break;
            }
            return format;
        }
    }
}