﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NewUploadFile.Common
{
    /// <summary>
    /// 图片处理方法
    /// </summary>
    public class ImageProcessMethod
    {
        /// <summary>
        /// 生成图片缩略图
        /// </summary>
        /// <param name="sourcePath">图片路径</param>
        /// <param name="newPath">新图片路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public static void MakeThumbnail(string sourcePath, string newPath, int width, int height)
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
        /// 添加文字水印
        /// </summary>
        /// <param name="sourcePath">原图片路径</param>
        /// <param name="newPath">新图片路径</param>
        /// <param name="text">水印文字</param>
        /// <param name="fontSize">文字大小</param>
        /// <param name="opacity">文字透明度(0-255 值越大透明度越低)</param>
        /// <param name="externName">文件后缀名</param>
        public static void AddTextWatermark(string sourcePath, string newPath, string text, float fontSize, int opacity, string externName)
        {
            MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(sourcePath));
            Image images = Image.FromStream(memoryStream);

            //Image images = Image.FromFile(sourcePath);

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
        /// <param name="sourcePath">原图片路径</param>
        /// <param name="newPath">新图片路径</param>
        /// <param name="watermarkPath">水印图片地址</param>
        /// <param name="opacity">文字透明度(0-255 值越大透明度越低)</param>
        /// <param name="externName">文件后缀名</param>
        public static void AddImgWatermark(string sourcePath, string newPath, string watermarkPath,  float opacity, string externName)
        {
            MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(sourcePath));
            Image sourceImages = Image.FromStream(memoryStream);

            MemoryStream watermarkStream = new MemoryStream(File.ReadAllBytes(watermarkPath));
            Image watermarkImages = Image.FromStream(watermarkStream);

            //Image sourceImages = Image.FromFile(sourcePath);

            //Image watermarkImages = Image.FromFile(watermarkPath);

            Bitmap bitmap = new Bitmap(sourceImages, sourceImages.Width, sourceImages.Height);

            Graphics graphics = Graphics.FromImage(bitmap);


            //下面定义一个矩形区域      
            float rectWidth = watermarkImages.Width + 10;
            float rectHeight = watermarkImages.Height + 10;

            //声明矩形域
            RectangleF textArea = new RectangleF((sourceImages.Width / 2) - rectWidth / 2, (sourceImages.Width / 2) - rectHeight / 2, rectWidth, rectHeight);

            Bitmap w_bitmap = ChangeOpacity(watermarkImages, opacity);

            graphics.DrawImage(w_bitmap, textArea);

            MemoryStream ms = new MemoryStream();

            //保存图片
            switch (externName)
            {
                case "jpg":
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    break;
                case "gif":
                    bitmap.Save(ms, ImageFormat.Gif);
                    break;
                case "png":
                    bitmap.Save(ms, ImageFormat.Png);
                    break;
                default:
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    break;
            }

            Image newImage = Image.FromStream(ms);

            graphics.Dispose();

            bitmap.Dispose();

            newImage.Save(newPath);
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
    }
}