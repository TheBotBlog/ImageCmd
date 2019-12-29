/*
 * A simple command-line that can be used for image manipulation.
 * This is used by thebotbloglib to do image manipulation in D using System.Drawing.
 * 
 * Copyright: The Bot Blog © 2019
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCmd
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args == null || args.Length < 3) return 1;

                var imagePath1 = args[0];
                var imagePath2 = args[1];

                var actionData = args[2].Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                var action = actionData[0];
                var actionArgs = actionData.Length > 1 ? actionData.Skip(1).ToArray() : new string[0];

                var imagePath2IsOutput = !(action == "merge" || action == "drawImage");

                var output = (imagePath2IsOutput || args.Length < 4) ? imagePath2 : args[3];

                using (var _LOADED1 = new Bitmap(imagePath1))
                {
                    using (var image1 = new Bitmap(_LOADED1.Width, _LOADED1.Height))
                    {
                        using (var gfx1 = Graphics.FromImage(image1))
                        {
                            gfx1.DrawImage(_LOADED1, 0, 0, _LOADED1.Width, _LOADED1.Height);

                            if (imagePath2IsOutput)
                            {
                                HandleImages(image1, null, gfx1, null, action, actionArgs);
                            }
                            else
                            {
                                using (var _LOADED2 = new Bitmap(imagePath2))
                                {
                                    using (var image2 = new Bitmap(_LOADED2.Width, _LOADED2.Height))
                                    {
                                        using (var gfx2 = Graphics.FromImage(image2))
                                        {
                                            gfx2.DrawImage(_LOADED2, 0, 0, _LOADED2.Width, _LOADED2.Height);

                                            HandleImages(image1, image2, gfx1, gfx2, action, actionArgs);
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            var fi = new FileInfo(output);

                            if (fi.Directory.Exists)
                            {
                                if (fi.Exists)
                                {
                                    fi.Delete();
                                }

                                image1.Save(output);
                            }
                            else
                            {
                                return 1;
                            }
                        }
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        static void HandleImages(Bitmap image1, Bitmap image2, Graphics gfx1, Graphics gfx2, string action, string[] args)
        {
            switch (action)
            {
                case "merge": Merge(image1, image2, gfx1, gfx2);  break;

                case "rotate90": Rotate90(image1, gfx1);  break;

                case "rotate180": Rotate180(image1, gfx1); break;

                case "flipH": FlipHorizontal(image1, gfx1);  break;

                case "flipV": FlipVertical(image1, gfx1); break;

                case "inverse": Inverse(image1, gfx1);  break;

                case "blackAndWhite": BlackAndWhite(image1, gfx1); break;

                case "drawText": DrawText(image1, gfx1, args); break;

                case "drawRect": DrawRect(image1, gfx1, args);  break;

                case "drawLine": DrawLine(image1, gfx1, args); break;

                case "drawImage": DrawImage(image1, gfx1, image2, gfx2, args); break;

                default: break;
            }
        }

        static void Merge(Bitmap image1, Image image2, Graphics gfx1, Graphics gfx2)
        {
            gfx1.DrawImage(image2, 0, 0, image2.Width, image2.Height);
        }

        static void Rotate90(Bitmap image1, Graphics gfx1)
        {
            image1.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        static void Rotate180(Bitmap image1, Graphics gfx1)
        {
            image1.RotateFlip(RotateFlipType.Rotate180FlipNone);
        }

        static void FlipHorizontal(Bitmap image1, Graphics gfx1)
        {
            image1.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        static void FlipVertical(Bitmap image1, Graphics gfx1)
        {
            image1.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        static void Inverse(Bitmap image1, Graphics gfx1)
        {
            for (var x = 0; x < image1.Width; x++)
            {
                for (var y = 0; y < image1.Height; y++)
                {
                    var color = image1.GetPixel(x, y);

                    var argb = color.ToArgb();

                    var reversedARGB = 0x00FFFFFF ^ argb;

                    var newColor = Color.FromArgb(reversedARGB);

                    image1.SetPixel(x, y, newColor);
                }
            }
        }

        static void BlackAndWhite(Bitmap image1, Graphics gfx1)
        {
            for (var x = 0; x < image1.Width; x++)
            {
                for (var y = 0; y < image1.Height; y++)
                {
                    var color = image1.GetPixel(x, y);
                    
                    var grayScaled = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);

                    var newColor = Color.FromArgb(grayScaled, grayScaled, grayScaled);

                    image1.SetPixel(x, y, newColor);
                }
            }
        }

        public class FontHolder
        {
            public PrivateFontCollection Collection { get; private set; }

            public FontFamily Family { get; private set; }

            public FontHolder(string file)
            {
                Collection = new PrivateFontCollection();
                Collection.AddFontFile(file);

                Family = Collection.Families.FirstOrDefault();
            }
        }

        static void LoadFontFromFile(string file)
        {
            var font = new FontHolder(file);

            Fonts.Add(file, font);
        }

        static Dictionary<string, FontHolder> Fonts = new Dictionary<string, FontHolder>();

        static void DrawText(Bitmap image1, Graphics gfx1, string[] args)
        {
            gfx1.InterpolationMode = InterpolationMode.High;
            gfx1.SmoothingMode = SmoothingMode.HighQuality;
            gfx1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gfx1.CompositingQuality = CompositingQuality.HighQuality;

            var argsDict = args.Select(arg => arg.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(kv => kv[0], kv => kv[1]);

            var text = argsDict["text"];

            string font;
            if (!argsDict.TryGetValue("font", out font))
            {
                font = "Calibri";
            }

            string fontSizeArg;
            float fontSize;
            if (!argsDict.TryGetValue("fontSize", out fontSizeArg) || !float.TryParse(fontSizeArg, out fontSize))
            {
                fontSize = 18;
            }

            string fontFile;
            argsDict.TryGetValue("fontFile", out fontFile);

            if (!string.IsNullOrWhiteSpace(fontFile))
            {
                LoadFontFromFile(fontFile);
            }

            string colorRGBA;
            if (!argsDict.TryGetValue("color", out colorRGBA) || string.IsNullOrWhiteSpace(colorRGBA))
            {
                colorRGBA = "0,0,0,255";
            }

            byte R;
            byte G;
            byte B;
            byte A;
            var colorRGBAData = colorRGBA.Split(',');

            if (colorRGBAData.Length != 4)
            {
                colorRGBAData = new [] { "0", "0", "0", "255" };
            }

            byte.TryParse(colorRGBAData[0], out R);
            byte.TryParse(colorRGBAData[1], out G);
            byte.TryParse(colorRGBAData[2], out B);
            byte.TryParse(colorRGBAData[3], out A);

            var color = Color.FromArgb(A, R, G, B);

            string centerTextArgs;
            bool centerText = false;

            if (argsDict.TryGetValue("centerText", out centerTextArgs) && !string.IsNullOrWhiteSpace(centerTextArgs))
            {
                bool.TryParse(centerTextArgs, out centerText);
            }

            string rectArgsText;
            if (!argsDict.TryGetValue("rect", out rectArgsText) || string.IsNullOrWhiteSpace(rectArgsText))
            {
                rectArgsText = "0,0,*,*";
            }

            var rectArgs = rectArgsText.Split(',');

            int x;
            int.TryParse(rectArgs[0], out x);
            int y;
            int.TryParse(rectArgs[1], out y);
            int width;
            if (rectArgs[2] == "*")
            {
                width = (image1.Width - x);
            }
            else if (rectArgs[2].StartsWith("-"))
            {
                int relative;
                if (int.TryParse(rectArgs[2].Substring(1), out relative))
                {
                    width = (image1.Width - relative);
                }
                else
                {
                    width = 0;
                }
            }
            else if (rectArgs[2].StartsWith("+"))
            {
                int relative;
                if (int.TryParse(rectArgs[2].Substring(1), out relative))
                {
                    width = (image1.Width + relative);
                }
                else
                {
                    width = 0;
                }
            }
            else
            {
                int.TryParse(rectArgs[2], out width);
            }
            int height;
            if (rectArgs[3] == "*")
            {
                height = (image1.Height - y);
            }
            else if (rectArgs[3].StartsWith("-"))
            {
                int relative;
                if (int.TryParse(rectArgs[3].Substring(1), out relative))
                {
                    height = (image1.Height - relative);
                }
                else
                {
                    height = 0;
                }
            }
            else if (rectArgs[3].StartsWith("+"))
            {
                int relative;
                if (int.TryParse(rectArgs[3].Substring(1), out relative))
                {
                    height = (image1.Height + relative);
                }
                else
                {
                    height = 0;
                }
            }
            else
            {
                int.TryParse(rectArgs[3], out height);
            }

            var rect = new Rectangle(x, y, width, height);

            StringFormat format = null;

            if (centerText)
            {
                format = new StringFormat();
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                Font loadedFont;
                if (!string.IsNullOrWhiteSpace(fontFile))
                {
                    loadedFont = new Font(Fonts[fontFile].Family, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                }
                else
                {
                    loadedFont = new Font(font, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                }

                try
                {
                    using (var brush = new SolidBrush(color))
                    {
                        if (format != null)
                        {
                            gfx1.DrawString(text, loadedFont, brush, rect, format);
                        }
                        else
                        {
                            gfx1.DrawString(text, loadedFont, brush, rect);
                        }
                    }
                }
                finally
                {
                    if (loadedFont != null)
                    {
                        loadedFont.Dispose();
                    }
                }
            }
        }

        static void DrawRect(Bitmap image1, Graphics gfx1, string[] args)
        {
            var argsDict = args.Select(arg => arg.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(kv => kv[0], kv => kv[1]);

            string colorRGBA;
            if (!argsDict.TryGetValue("color", out colorRGBA) || string.IsNullOrWhiteSpace(colorRGBA))
            {
                colorRGBA = "0,0,0,255";
            }

            byte R;
            byte G;
            byte B;
            byte A;
            var colorRGBAData = colorRGBA.Split(',');

            if (colorRGBAData.Length != 4)
            {
                colorRGBAData = new[] { "0", "0", "0", "255" };
            }

            byte.TryParse(colorRGBAData[0], out R);
            byte.TryParse(colorRGBAData[1], out G);
            byte.TryParse(colorRGBAData[2], out B);
            byte.TryParse(colorRGBAData[3], out A);

            var color = Color.FromArgb(A, R, G, B);

            string rectArgsText;
            if (!argsDict.TryGetValue("rect", out rectArgsText) || string.IsNullOrWhiteSpace(rectArgsText))
            {
                rectArgsText = "0,0,0,0";
            }

            var rectArgs = rectArgsText.Split(',');

            int x;
            int.TryParse(rectArgs[0], out x);
            int y;
            int.TryParse(rectArgs[1], out y);
            int width;
            int.TryParse(rectArgs[2], out width);
            int height;
            int.TryParse(rectArgs[3], out height);

            var rect = new Rectangle(x, y, width, height);

            string fillTextArgs;
            bool fill = false;

            if (argsDict.TryGetValue("fill", out fillTextArgs) && !string.IsNullOrWhiteSpace(fillTextArgs))
            {
                bool.TryParse(fillTextArgs, out fill);
            }

            if (fill)
            {
                using (var brush = new SolidBrush(color))
                {
                    gfx1.FillRectangle(brush, rect);
                }
            }
            else
            {
                using (var pen = new Pen(color))
                {
                    gfx1.DrawRectangle(pen, rect);
                }
            }
        }

        static void DrawLine(Bitmap image1, Graphics gfx1, string[] args)
        {
            var argsDict = args.Select(arg => arg.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(kv => kv[0], kv => kv[1]);

            string colorRGBA;
            if (!argsDict.TryGetValue("color", out colorRGBA) || string.IsNullOrWhiteSpace(colorRGBA))
            {
                colorRGBA = "0,0,0,255";
            }

            byte R;
            byte G;
            byte B;
            byte A;
            var colorRGBAData = colorRGBA.Split(',');

            if (colorRGBAData.Length != 4)
            {
                colorRGBAData = new[] { "0", "0", "0", "255" };
            }

            byte.TryParse(colorRGBAData[0], out R);
            byte.TryParse(colorRGBAData[1], out G);
            byte.TryParse(colorRGBAData[2], out B);
            byte.TryParse(colorRGBAData[3], out A);

            var color = Color.FromArgb(A, R, G, B);

            string lineArgsText;
            if (!argsDict.TryGetValue("args", out lineArgsText) || string.IsNullOrWhiteSpace(lineArgsText))
            {
                lineArgsText = "0,0,0,0";
            }

            var lineArgs = lineArgsText.Split(',');

            int startX;
            int.TryParse(lineArgs[0], out startX);
            int startY;
            int.TryParse(lineArgs[1], out startY);
            int endX;
            int.TryParse(lineArgs[2], out endX);
            int endY;
            int.TryParse(lineArgs[3], out endY);

            using (var pen = new Pen(color))
            {
                gfx1.DrawLine(pen, startX, startY, endX, endY);
            }
        }

        static void DrawImage(Bitmap image1, Graphics gfx1, Bitmap image2, Graphics gfx2, string[] args)
        {
            var argsDict = args.Select(arg => arg.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(kv => kv[0], kv => kv[1]);

            string sizeArgsText;
            if (!argsDict.TryGetValue("size", out sizeArgsText) || string.IsNullOrWhiteSpace(sizeArgsText))
            {
                sizeArgsText = "*,*";
            }

            var sizeArgs = sizeArgsText.Split(',');

            int width;
            if (sizeArgs[0] == "*")
            {
                width = image2.Width;
            }
            else
            {
                int.TryParse(sizeArgs[0], out width);
            }
            int height;
            if (sizeArgs[0] == "*")
            {
                height = image2.Height;
            }
            else
            {
                int.TryParse(sizeArgs[1], out height);
            }

            string posArgsText;
            if (!argsDict.TryGetValue("position", out posArgsText) || string.IsNullOrWhiteSpace(posArgsText))
            {
                posArgsText = "0,0";
            }

            var posArgs = posArgsText.Split(',');

            int x;
            int.TryParse(posArgs[0], out x);
            int y;
            int.TryParse(posArgs[1], out y);

            gfx1.DrawImage(image2, x, y, width, height);
        }
    }
}
