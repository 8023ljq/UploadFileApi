using System;

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace ImageServer.Controllers
{
    public class NewAddWaterMark
    {
        #region 添加水印
        /// <summary>
        /// 添加文字水印
        /// </summary>
        /// <param name="image"></param>
        /// <param name="text"></param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="rectX">水印开始X坐标（自动扣除文字宽度）</param>
        /// <param name="rectY">水印开始Y坐标（自动扣除文字高度</param>
        /// <param name="opacity">0-255 值越大透明度越低</param>
        /// <param name="externName">文件后缀名</param>
        /// <returns></returns>
        public static Image AddTextToImg(Image image, string text, float fontSize, float rectX, float rectY, int opacity, string externName)
        {
            Bitmap bitmap = new Bitmap(image, image.Width, image.Height);

            Graphics g = Graphics.FromImage(bitmap);

            //下面定义一个矩形区域            
            float rectWidth = text.Length * (fontSize + 10);
            float rectHeight = fontSize + 10;

            //声明矩形域

            // RectangleF textArea = new RectangleF(rectX - rectWidth, rectY - rectHeight, rectWidth, rectHeight);

            RectangleF textArea = new RectangleF(rectX - rectWidth / 2, rectY - rectHeight / 2, rectWidth, rectHeight);
            Font font = new Font("微软雅黑", fontSize, FontStyle.Bold); //定义字体

            Brush whiteBrush = new SolidBrush(Color.FromArgb(opacity, 255, 255, 255)); //画文字用

            g.DrawString(text, font, whiteBrush, textArea);

            MemoryStream ms = new MemoryStream();

            //保存图片
            switch (externName)
            {
                case ".jpg":
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    break;
                case ".gif":
                    bitmap.Save(ms, ImageFormat.Gif);
                    break;
                case ".png":
                    bitmap.Save(ms, ImageFormat.Png);
                    break;
                default:
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    break;
            }


            Image h_hovercImg = Image.FromStream(ms);

            g.Dispose();

            bitmap.Dispose();

            return h_hovercImg;

        }

        /// <summary>
        /// 添加文字水印(修改)
        /// </summary>
        /// <param name="sourcePath">原图片路径</param>
        /// <param name="newPath">新图片路径</param>
        /// <param name="text">水印文字</param>
        /// <param name="fontSize">文字大小</param>
        /// <param name="opacity">文字透明度(0-255 值越大透明度越低)</param>
        /// <param name="externName">文件后缀名</param>
        public static void AddTextWatermark(string sourcePath, string newPath, string text, float fontSize, int opacity, string externName)
        {
            //MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(sourcePath));

            //Image images = Image.FromStream(memoryStream);
            Image images = Image.FromFile(sourcePath);

            Bitmap bitmap = new Bitmap(images, images.Width, images.Height);

            Graphics graphics = Graphics.FromImage(bitmap);

            //下面定义一个矩形区域            
            float rectWidth = text.Length * (fontSize + 10);
            float rectHeight = fontSize + 10;

            //声明矩形域
            RectangleF textArea = new RectangleF((images.Width / 2) - rectWidth / 2, (images.Height / 2) - rectHeight / 2, rectWidth, rectHeight);
            Font font = new Font("微软雅黑", fontSize, FontStyle.Bold); //定义字体

            Brush whiteBrush = new SolidBrush(Color.FromArgb(opacity, 255, 255, 255)); //画文字用

            graphics.DrawString(text, font, whiteBrush, textArea);

            MemoryStream newMemorystream = new MemoryStream();

            //保存图片
            switch (externName)
            {
                case "jpg":
                    bitmap.Save(newMemorystream, ImageFormat.Jpeg);
                    break;
                case "gif":
                    bitmap.Save(newMemorystream, ImageFormat.Gif);
                    break;
                case "png":
                    bitmap.Save(newMemorystream, ImageFormat.Png);
                    break;
                default:
                    bitmap.Save(newMemorystream, ImageFormat.Jpeg);
                    break;
            }

            Image newImage = Image.FromStream(newMemorystream);

            graphics.Dispose();

            bitmap.Dispose();

            newImage.Save(newPath);
        }



        /// <summary>
        /// 添加图片水印
        /// </summary>
        /// <param name="image"></param>
        /// <param name="text"></param>
        /// <param name="rectX">水印开始X坐标（自动扣除图片宽度）</param>
        /// <param name="rectY">水印开始Y坐标（自动扣除图片高度</param>
        /// <param name="opacity">透明度 0-1</param>
        /// <param name="externName">文件后缀名</param>
        /// <returns></returns>
        public static Image AddImgToImg(Image image, Image watermark, float rectX, float rectY, float opacity, string externName)
        {

            Bitmap bitmap = new Bitmap(image, image.Width, image.Height);

            Graphics g = Graphics.FromImage(bitmap);


            //下面定义一个矩形区域      
            float rectWidth = watermark.Width + 10;
            float rectHeight = watermark.Height + 10;

            //声明矩形域
            RectangleF textArea = new RectangleF(rectX - rectWidth / 2, rectY - rectHeight / 2, rectWidth, rectHeight);

            Bitmap w_bitmap = ChangeOpacity(watermark, opacity);

            g.DrawImage(w_bitmap, textArea);

            MemoryStream ms = new MemoryStream();

            //保存图片
            switch (externName)
            {
                case ".jpg":
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    break;
                case ".gif":
                    bitmap.Save(ms, ImageFormat.Gif);
                    break;
                case ".png":
                    bitmap.Save(ms, ImageFormat.Png);
                    break;
                default:
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    break;
            }

            Image h_hovercImg = Image.FromStream(ms);

            g.Dispose();

            bitmap.Dispose();
            return h_hovercImg;

        }

        /// <summary>
        /// 改变图片的透明度
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="opacityvalue">透明度</param>
        /// <returns></returns>
        public static Bitmap ChangeOpacity(Image img, float opacityvalue)
        {

            float[][] nArray ={ new float[] {1, 0, 0, 0, 0},

                                new float[] {0, 1, 0, 0, 0},

                                new float[] {0, 0, 1, 0, 0},

                                new float[] {0, 0, 0, opacityvalue, 0},

                                new float[] {0, 0, 0, 0, 1}};

            ColorMatrix matrix = new ColorMatrix(nArray);

            ImageAttributes attributes = new ImageAttributes();

            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            Image srcImage = img;

            Bitmap resultImage = new Bitmap(srcImage.Width, srcImage.Height);

            Graphics g = Graphics.FromImage(resultImage);

            g.DrawImage(srcImage, new Rectangle(0, 0, srcImage.Width, srcImage.Height), 0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel, attributes);

            return resultImage;
        }

        #endregion
    }
}