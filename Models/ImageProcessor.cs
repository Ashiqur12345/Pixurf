using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace Pixurf.Models
{
    public class ImageProcessor : IDisposable
    {
        private static int THUMBNAIL_MAX_WIDTH = 400;
        private static int THUMBNAIL_MAX_HEIGHT = 300;

        public Image CreateThumbnail(Image srcImage)
        {
            double height = srcImage.Height;
            double width = srcImage.Width;
            double ratio = 4.0 / 3.0;
            Rectangle cropRectangle;

            if (height < width)
            {
                width = height * ratio; 
                cropRectangle = new Rectangle((int)(srcImage.Width - width) / 2, 0, (int) width, (int) height);
            }
            else if (height > width)
            {
                height = width / ratio;
                cropRectangle = new Rectangle(0, (int)((srcImage.Height - height) / 2), (int) width, (int) height);
            }
            else
            {
                cropRectangle = new Rectangle(0, 0, (int) width, (int) height);
            }
            
            Image croppedImage = CropImage(srcImage as Bitmap, cropRectangle);

            Image result = ResizeImage(croppedImage, THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_HEIGHT);
            return result;
        }

        private Image ResizeImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {

                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }


        private Bitmap CropImage(Bitmap srcImage, Rectangle cropRect)
        {
            Bitmap croppedImage = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(srcImage, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height),
                    cropRect,
                    GraphicsUnit.Pixel);
            }

            return croppedImage;
        }

        public static ImageCodecInfo GetJpegEncoderInfo()
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.MimeType == "image/jpeg");
        }

        public static EncoderParameters GetEncoderParameters()
        {
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, 50); // 50% quality
            EncoderParameters encoderParams = new EncoderParameters(1) {Param = {[0] = qualityParam}};
            return encoderParams;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}