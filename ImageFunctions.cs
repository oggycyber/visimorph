﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisiMorph
{
    internal class ImageFunctions
    {

        public static Bitmap grayTransformation(Bitmap image)
        {
            
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color currentPixel = image.GetPixel(x, y);
                    int grayValue = (int)(currentPixel.R * 0.299 + currentPixel.G * 0.587 + currentPixel.B * 0.114);
                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                    image.SetPixel(x, y, grayColor);
                }
            }

            return image;
        }

        public static Bitmap binaryTransformation(Bitmap image, int threshold)
        {

            image = grayTransformation(image);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color currentPixel = image.GetPixel(x, y);
                    // Grayscale formatta R, G ve B değerleri eşit olduğu için yalnız birisiyle karşılaştırma yapılıyor.
                    if (currentPixel.R < threshold)
                    {
                        Color black = Color.FromArgb(0, 0, 0);
                        image.SetPixel(x, y, black);
                    }

                    else
                    {
                        Color white = Color.FromArgb(255, 255, 255);
                        image.SetPixel(x, y, white);
                    }
                }
            }

            return image;
        }

        public static (double, double, double) RGBtoYCbCr(int R, int G, int B)
        {
            double Y, Cb, Cr;
            int delta = 128;
            Y = 0.299 * R + 0.587 * G + 0.114 * B;
            Cr = (R - Y) * 0.713 + delta;
            Cb = (B - Y) * 0.564 + delta;

            return (Y, Cb, Cr);
        }

        public static (int, int, int) YCbCrtoRGB(double Y, double Cb, double Cr)
        {
            int R, G, B;
            int delta = 128;

            R = (int)(Y + 1.403 * (Cr - delta));
            G = (int)((Y - 0.714 * (Cr - delta)) - 0.344 * (Cb - delta));
            B = (int)(Y + 1.773 * (Cb - delta));
            R = Math.Clamp(R, 0, 255);
            G = Math.Clamp(G, 0, 255);
            B = Math.Clamp(B, 0, 255);
            return (R, G, B);
        }

       

        public static (double H, double S, double V) RGBtoHSV(int R, int G, int B)
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double H = 0;

            if (delta == 0)
            {
                H = 0;
            }
            else if (max == r)
            {
                H = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                H = 60 * (((b - r) / delta) + 2);
            }
            else if (max == b)
            {
                H = 60 * (((r - g) / delta) + 4);
            }

            if (H < 0)
                H += 360;

            double S = (max == 0) ? 0 : delta / max;
            double V = max;

            return (H, S, V);
        }


        public static (int, int, int) HSVtoRGB(double H, double S, double V)
        {
            S /= 100.0;
            V /= 100.0;

            double C = V * S;
            double hPrime = H / 60.0;
            double X = C * (1 - Math.Abs(hPrime % 2 - 1));
            double m = V - C;

            double r = 0, g = 0, b = 0;

            if (hPrime >= 0 && hPrime < 1)
                (r, g, b) = (C, X, 0);
            else if (hPrime >= 1 && hPrime < 2)
                (r, g, b) = (X, C, 0);
            else if (hPrime >= 2 && hPrime < 3)
                (r, g, b) = (0, C, X);
            else if (hPrime >= 3 && hPrime < 4)
                (r, g, b) = (0, X, C);
            else if (hPrime >= 4 && hPrime < 5)
                (r, g, b) = (X, 0, C);
            else if (hPrime >= 5 && hPrime < 6)
                (r, g, b) = (C, 0, X);

            int R = (int)Math.Round((r + m) * 255);
            int G = (int)Math.Round((g + m) * 255);
            int B = (int)Math.Round((b + m) * 255);

            return (R, G, B);
        }





        

        public static Bitmap Convolution(Bitmap image, double[,] kernel, bool isAddEdge)
        {
            int kernelWidth = kernel.GetLength(0);
            int kernelHeight = kernel.GetLength(1);

            int halfKernelWidth = kernelWidth / 2;
            int halfKernelHeight = kernelHeight / 2;
            // Kenarları doldurma
            if (!isAddEdge)
            {
                Bitmap newImage = new Bitmap(image.Width, image.Height);
                for (int y = halfKernelHeight; y < image.Height - halfKernelHeight; y++)
                {
                    for (int x = halfKernelWidth; x < image.Width - halfKernelWidth; x++)
                    {
                        int sumR = 0;
                        int sumG = 0;
                        int sumB = 0;
                        int R, G, B = 0;
                        for (int tX = 0; tX < kernelHeight; tX++)
                        {
                            for (int tY = 0; tY < kernelWidth; tY++)
                            {

                                int pixelX = x + (tX - halfKernelWidth);
                                int pixelY = y + (tY - halfKernelHeight);

                                Color currentPixel = image.GetPixel(pixelX, pixelY);
                                R = (int)(currentPixel.R * kernel[tX, tY]);
                                G = (int)(currentPixel.G * kernel[tX, tY]);
                                B = (int)(currentPixel.B * kernel[tX, tY]);
                                sumR += R;
                                sumG += G;
                                sumB += B;
                            }
                        }

                        sumR = Math.Min(Math.Max(sumR, 0), 255);
                        sumG = Math.Min(Math.Max(sumG, 0), 255);
                        sumB = Math.Min(Math.Max(sumB, 0), 255);
                        Color newPixel = Color.FromArgb(sumR, sumG, sumB);
                        newImage.SetPixel(x, y, newPixel);
                    }
                }
                return newImage;
            }
            else
            {
                Bitmap newImage = FillEdge(image);

                for (int y = halfKernelHeight; y < newImage.Height - halfKernelHeight; y++)
                {
                    for (int x = halfKernelWidth; x < newImage.Width - halfKernelWidth; x++)
                    {
                        int sumR = 0;
                        int sumG = 0;
                        int sumB = 0;
                        int R, G, B = 0;
                        for (int tX = 0; tX < kernelHeight; tX++)
                        {
                            for (int tY = 0; tY < kernelWidth; tY++)
                            {

                                int pixelX = x + (tX - halfKernelWidth);
                                int pixelY = y + (tY - halfKernelHeight);

                                Color currentPixel = newImage.GetPixel(pixelX, pixelY);
                                R = (int)(currentPixel.R * kernel[tX, tY]);
                                G = (int)(currentPixel.G * kernel[tX, tY]);
                                B = (int)(currentPixel.B * kernel[tX, tY]);
                                sumR += R;
                                sumG += G;
                                sumB += B;
                            }
                        }

                        sumR = Math.Min(Math.Max(sumR, 0), 255);
                        sumG = Math.Min(Math.Max(sumG, 0), 255);
                        sumB = Math.Min(Math.Max(sumB, 0), 255);
                        Color newPixel = Color.FromArgb(sumR, sumG, sumB);
                        newImage.SetPixel(x, y, newPixel);
                    }
                }
                return newImage;
            }
        }
        public static Bitmap FillEdge(Bitmap image)
        {
            Bitmap newBitmap = new Bitmap(image.Width + 1, image.Height + 1);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (x == 0 || y == 0 || x == newBitmap.Width || y == newBitmap.Height)
                    {
                        newBitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        newBitmap.SetPixel(x, y, image.GetPixel(x, y));
                    }
                }
            }
            return newBitmap;
        }
        public static double[,] GaussianFilter(double sigma, int kernelSize)
        {

            double[,] kernel = new double[kernelSize, kernelSize];
            double sum = 0.0;
            int center = kernelSize / 2;

            double twoSigmaSquare = 2.0 * Math.PI * sigma * sigma;

            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    int x = j - center;
                    int y = i - center;

                    double exponent = -(x * x + y * y) / twoSigmaSquare;

                    kernel[i, j] = Math.Exp(exponent);

                    sum += kernel[i, j];
                }
            }

            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

            return kernel;
        }

        public static Bitmap brightnessTransformation(Bitmap image, int brightness)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color currentPixel = image.GetPixel(x, y);
                    int R = Math.Min(Math.Max(currentPixel.R + brightness, 0), 255);
                    int G = Math.Min(Math.Max(currentPixel.G + brightness, 0), 255);
                    int B = Math.Min(Math.Max(currentPixel.B + brightness, 0), 255);
                    Color newPixel = Color.FromArgb(R, G, B);
                    result.SetPixel(x, y, newPixel);
                }
            }
            return result;
        }
        // Format24bppRgb kullanımının nedeni bazı dosyaların indexed-pixel kullanması, bu hataya yol açıyor. O yüzden bazı fonksiyonlara dönüştürme için ekliyorum.
        public static int[] calculateHistogram(Bitmap image)
        {
            // zaten grayTransformation kullanılmış, o yüzden Format24bppRgb kullanımına gerek yok.
            image = ImageFunctions.grayTransformation(image);
            int[] histogramCounter = new int[256];
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color currentPixel = image.GetPixel(x, y);
                    int grayValue = currentPixel.R;
                    histogramCounter[grayValue] += 1;
                }
            }
            return histogramCounter;
        }

        public static Bitmap stretchingHistogram(Bitmap image)
        {
            image = ImageFunctions.grayTransformation(image);
            int min = 255;
            int max = 0;

            // Min–max tespiti
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int pixelValue = image.GetPixel(x, y).R;

                    if (pixelValue < min) min = pixelValue;
                    if (pixelValue > max) max = pixelValue;
                }
            }

            if (min == max) return image;

            Bitmap stretchedImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color currentPixel = image.GetPixel(x, y);
                    int stretchedValue = ((currentPixel.R - min) * 255) / (max - min);
                    Color newPixel = Color.FromArgb(stretchedValue, stretchedValue, stretchedValue);
                    stretchedImage.SetPixel(x, y, newPixel);
                }
            }
            return stretchedImage;
        }
        public static Bitmap extendingHistogram(Bitmap image, int minRange, int maxRange)
        {
            image = ImageFunctions.grayTransformation(image);

            Bitmap extendedImage = new Bitmap(image.Width, image.Height);

            int tempMin = Math.Min(minRange, maxRange);
            int tempMax = Math.Max(minRange, maxRange);
            minRange = tempMin;
            maxRange = tempMax;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color currentPixel = image.GetPixel(x, y);
                    if (currentPixel.R >= minRange && currentPixel.R <= maxRange)
                    {
                        
                        int stretchedValue = ((currentPixel.R - minRange) * 255) / (maxRange - minRange);
                        Color newPixel = Color.FromArgb(stretchedValue, stretchedValue, stretchedValue);
                        extendedImage.SetPixel(x, y, newPixel);
                    }

                    else 
                    { 
                        extendedImage.SetPixel(x, y, currentPixel);
                    }
                }
            }

            return extendedImage;
        }
    }
}