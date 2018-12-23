using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//using System.Drawing;
//using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;

using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

class ImageTooSmall : Exception
{
    public ImageTooSmall()
    { }

    public ImageTooSmall(string message) : base(message)
    { }

    public ImageTooSmall(string message, Exception inner) : base(message, inner)
    { }


}

static class ImageRenderer
{
    private static byte GetBitsPerPixel(System.Drawing.Imaging.PixelFormat pixelFormat)
    {
        switch (pixelFormat)
        {
            case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                return 24;
                break;
            case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
            case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
            case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                return 32;
                break;
            default:
                throw new ArgumentException("Only 24 and 32 bit images are supported");

        }
    }


    static byte valswitch(bool boolval, byte val)
    {
        if (boolval)
            return val;
        else
            return 0;
    }

    [Obsolete("Don't use this.", true)]
    public static unsafe System.Drawing.Image RendertoBWImg(byte[] src)
    {
        var bitdata = new BitArray(src);

        int imgdimension = (int)Math.Ceiling(Math.Sqrt(bitdata.Length));

        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imgdimension, imgdimension);

        System.Drawing.Imaging.BitmapData bitmapdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

        byte bitsperpixel = GetBitsPerPixel(bitmapdata.PixelFormat);

        byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();

        for (int i = 0; i < bitmapdata.Height; i++)
        {
            for (int j = 0; j < bitmapdata.Width; j++)
            {
                if (j + (i * bitmapdata.Width) >= bitdata.Length)
                {
                    break;
                }

                byte* data = scan0 + i * bitmapdata.Stride + j * bitsperpixel / 8;

                //Console.WriteLine($"Writepos {j + (i * bitmapdata.Width)}. J = {j}; I = {i}");

                if (bitdata[j + (i * bitmapdata.Width)] == true)
                {
                    data[0] = 255;
                    data[1] = 255;
                    data[2] = 255;
                }
            }
        }

        bmp.UnlockBits(bitmapdata);

        return bmp;
    }

    [Obsolete("Don't use this.", true)]
    public static unsafe void SavetoBWImg(byte[] src, string name)
    {
        var bitdata = new BitArray(src);

        int imgdimension = (int)Math.Ceiling(Math.Sqrt(bitdata.Length));

        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imgdimension, imgdimension);

        System.Drawing.Imaging.BitmapData bitmapdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

        byte bitsperpixel = GetBitsPerPixel(bitmapdata.PixelFormat);

        byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();

        for (int i = 0; i < bitmapdata.Height; i++)
        {
            for (int j = 0; j < bitmapdata.Width; j++)
            {
                if (j + (i * bitmapdata.Width) >= bitdata.Length)
                {
                    break;
                }

                byte* data = scan0 + i * bitmapdata.Stride + j * bitsperpixel / 8;

                //Console.WriteLine($"Writepos {j + (i * bitmapdata.Width)}. J = {j}; I = {i}");

                if (bitdata[j + (i * bitmapdata.Width)] == true)
                {
                    data[0] = 255;
                    data[1] = 255;
                    data[2] = 255;
                }
            }
        }

        bmp.UnlockBits(bitmapdata);

        bmp.Save(name, System.Drawing.Imaging.ImageFormat.Bmp);
    }

    public static BitmapSource RenderToBWImg(byte[] src, int scaling = 1, bool reversed = false, int maxwidth = 0, int maxheight = 0)
    {
        if (src.Length == 0)
            return null;

        var bitdata = new BitArray(src);
        //var bitdata = new BitArray(new bool[] { false, true, false, true, true, false, true, false, false, true, false, true, true, false, true, false });

        int imgdimension = (int)Math.Ceiling(Math.Sqrt(bitdata.Length));

        int stride = imgdimension;

        if (imgdimension > maxwidth * maxheight)
        {

        }

        int width = imgdimension;
        int height = imgdimension;

        if (maxwidth != 0 && maxheight == 0)
        {
            width = maxwidth;
            height = (int)Math.Ceiling(bitdata.Length / (double)width);
        }
        
        else if(maxheight != 0 && maxwidth == 0)
        {
            height = maxheight;
            width = (int)Math.Ceiling(bitdata.Length / (double)height);}
        
        else if(maxheight != 0 && maxwidth != 0)
        {
            height = maxheight;
            width = maxwidth;
        }

        stride = width;

        //if (maxwidth != 0)
        //{
        //    width = maxwidth;
        //    stride = width;

        //    height = (int)Math.Ceiling(bitdata.Length / (double)width);
        //}

        //if (maxheight != 0)
        //{
        //    height = maxheight;
        //}

        if (width * height < bitdata.Length)
            //throw new ;
            throw new ImageTooSmall("Image is too small for the data");

        #region Abandoned Code
        //Abandoning this for a more better solution
        //
        //int stride = imgdimension * 4;

        //byte[] pixel_data = new byte[imgdimension * imgdimension * 4];

        //for (int i = 0; i < imgdimension; i++)
        //{
        //    for (int j = 0; j < imgdimension * 4; j += 4)
        //    {
        //        int datapos = j + (i * (imgdimension * 4));
        //        int bitpos = (j / 4) + (i * imgdimension);

        //        if (bitdata[bitpos])
        //        {
        //            pixel_data[datapos      ] = 255;
        //            pixel_data[datapos + 1  ] = 255;
        //            pixel_data[datapos + 2  ] = 255;
        //            pixel_data[datapos + 3  ] = 255;
        //        }
        //        else
        //        {
        //            pixel_data[datapos + 3] = 255;
        //        }
        //    }
        //}
        #endregion

        byte[] pixel_data = new byte[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int pos = j + (i * width);

                if (pos >= bitdata.Length)
                    break;

                //Yeah i know this is a silly one.
                if (!reversed)
                    if (bitdata[pos])
                        pixel_data[pos] = 255;
                    else
                        pixel_data[pos] = 0;
                else
                    if (!bitdata[pos])
                        pixel_data[pos] = 255;
                    else
                        pixel_data[pos] = 0;
            }
        }

        var scaled = resizePixels(pixel_data, width, height, width * scaling, height * scaling);

        return BitmapSource.Create(width * scaling, height * scaling, 300, 300, PixelFormats.Indexed8, BitmapPalettes.Gray256, scaled, stride * scaling);
    }

    //Using this code from this site : 
    //http://tech-algorithm.com/articles/nearest-neighbor-image-scaling/
    public static byte[] resizePixels(byte[] pixels, int _w1, int _h1, int _w2, int _h2)
    {
        byte[] temp = new byte[_w2 * _h2];

        ulong w1 = (ulong)_w1;
        ulong h1 = (ulong)_h1;
        ulong w2 = (ulong)_w2;
        ulong h2 = (ulong)_h2;

        // EDIT: added +1 to account for an early rounding problem
        ulong x_ratio = (ulong)((w1 << 16) / w2);// + 1;
        ulong y_ratio = (ulong)((h1 << 16) / h2);// + 1;
        //int x_ratio = (int)((w1<<16)/w2) ;
        //int y_ratio = (int)((h1<<16)/h2) ;
        ulong x2, y2;
        for (ulong i = 0; i < h2; i++)
        {
            for (ulong j = 0; j < w2; j++)
            {
                x2 = ((j * x_ratio) >> 16);
                y2 = ((i * y_ratio) >> 16);
                temp[(i * w2) + j] = pixels[(y2 * w1) + x2];
            }
        }
        return temp;
    }
}