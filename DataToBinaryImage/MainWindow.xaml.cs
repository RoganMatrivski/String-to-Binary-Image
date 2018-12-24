using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DataToBinaryImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string text_to_render;

        private static readonly Regex binaryregex = new Regex("[^0-1]+"); //regex that matches disallowed text
        private void update_preview(string string_text)
        {
            text_to_render = string_text;

            bool[] booldata = new bool[0];

            if (!binaryregex.IsMatch(text_to_render))
            {
                booldata = new bool[text_to_render.Length];

                for (int i = 0; i < text_to_render.Length; i++)
                {
                    booldata[i] = Convert.ToBoolean(char.GetNumericValue(text_to_render[i]));
                }
            }

            if (ImageView == null || imgsize == null || imgsize_scaled == null || ScaleOption == null)
                return;

            if (ImageView != null)
            {
                var text = text_to_render.TrimEnd(new char[] { '\r', '\n' });

                int width = 0;
                int height = 0;

                if (!string.IsNullOrWhiteSpace(MaxWidth.Text) && MaxWidth.Text != "0")
                    width = Convert.ToInt32(MaxWidth.Text);

                if (!string.IsNullOrWhiteSpace(MaxHeight.Text) && MaxHeight.Text != "0")
                    height = Convert.ToInt32(MaxHeight.Text);

                BitmapSource result;

                string errmessage = "";

                try
                {
                    if (booldata == null || booldata.Length == 0)
                        result = ImageRenderer.RenderToBWImg(Encoding.UTF8.GetBytes(text), 1, invert_checkbox.IsChecked ?? false, width, height);
                    else
                        result = ImageRenderer.RenderToBWImg(booldata, 1, invert_checkbox.IsChecked ?? false, width, height);
                }
                catch(ImageTooSmallException)
                {
                    result = null;
                    errmessage = "Image Too Small For Data";
                }

                if (result != null)
                {
                    ImageView.Source = result;
                    ImageView.Stretch = Stretch.Uniform;

                    imgsize.Content = $"Current Size : {result.PixelWidth}x{result.PixelHeight}";
                    imgsize_scaled.Content = $"Current Scaled Size : {result.PixelWidth * ((scale_options)ScaleOption.SelectedItem).value}x{result.PixelHeight * ((scale_options)ScaleOption.SelectedItem).value}";
                    status_label.Content = "";
                    status_label.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    ImageView.Source = new BitmapImage(new Uri("pack://application:,,,/broken.png"));
                    ImageView.Stretch = Stretch.None;

                    imgsize.Content = $"Current Size : NaN";
                    imgsize_scaled.Content = $"Current Scaled Size : NaN";
                    status_label.Content = errmessage;
                    status_label.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void update_preview(object sender, RoutedEventArgs e)
        {
            update_preview(text_to_render);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int width = 0;
            int height = 0;

            if (!string.IsNullOrWhiteSpace(MaxWidth.Text) && MaxWidth.Text != "0")
                width = Convert.ToInt32(MaxWidth.Text);

            if (!string.IsNullOrWhiteSpace(MaxHeight.Text) && MaxHeight.Text != "0")
                height = Convert.ToInt32(MaxHeight.Text);

            //var res = ImageRenderer.RenderToBWImg(Encoding.UTF8.GetBytes(new TextRange(MessageBox.Document.ContentStart, MessageBox.Document.ContentEnd).Text));
            var text = msg_string.Text.TrimEnd(new char[] { '\r', '\n'});

            //var res = ImageRenderer.RenderToBWImg(Encoding.UTF8.GetBytes(text));
            var res = ImageRenderer.RenderToBWImg(Encoding.UTF8.GetBytes(text), ((scale_options)ScaleOption.SelectedItem).value, invert_checkbox.IsChecked ?? false, width, height);

            ShowExportDialog(res);
        }

        private void ShowExportDialog(BitmapSource image)
        {
            if (image == null) return;

            SaveFileDialog saveImg = new SaveFileDialog();
            saveImg.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp|TIFF Image|*.tiff|Windows Media Bitmap|*.wmb";
            saveImg.Title = "Export Single Image";
            saveImg.FileName = "Export";
            saveImg.AddExtension = true;
            saveImg.DefaultExt = "*.png";

            saveImg.CheckPathExists = true;

            BitmapEncoder encoder = null;
            if (saveImg.ShowDialog() == true)
            {
                string ext = Path.GetExtension(saveImg.FileName);
                switch (ext.ToLower())
                {
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".jpeg":
                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    case ".wmb":
                        encoder = new WmpBitmapEncoder();
                        break;
                    default:
                        encoder = new BmpBitmapEncoder();
                        saveImg.FileName += ".bmp";
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(image));
                try
                {
                    using (FileStream fs = new FileStream(saveImg.FileName, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export file failed. See details of the error below :" +
                        Environment.NewLine +
                        Environment.NewLine +
                        $"{ex.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }

                status_label.Content = "Export Completed!";
            }
        }


        private void msg_string_TextChanged(object sender, TextChangedEventArgs e)
        {
            update_preview(msg_string.Text);
        }

        private void ScaleOption_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                int test = (int)Math.Pow(2, i);
                ScaleOption.Items.Add(new scale_options((int)Math.Pow(2, i), Math.Pow(2, i) + "x"));
            }

            ScaleOption.SelectedIndex = 0;
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void MaxHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            update_preview(msg_string.Text);
        }

        private void MaxHeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void MaxWidth_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void MaxWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            update_preview(msg_string.Text);
        }

        //private static Task runningtask;

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            //runningtask = Task.Run(async () =>
            //{
            //    char[] charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            //    Random rand = new Random();

            //    while (true)
            //    {
            //        //this.Dispatcher.Invoke(() => MaxWidth.Text = rand.Next(2, 32).ToString());

            //        int remaining = rand.Next(8, 64);
            //        while (remaining-- > 0)
            //        {

            //            int word = rand.Next(4, 10);
            //            while (word-- > 0)
            //            {
            //                this.Dispatcher.Invoke(() => msg_string.Text += charset[rand.Next(charset.Length)]);
            //                await Task.Delay(3);

            //            }

            //            this.Dispatcher.Invoke(() => msg_string.Text += " ");

            //            if (rand.NextDouble() > 0.85)
            //                this.Dispatcher.Invoke(() => msg_string.Text += "\n");
            //        }
            //        this.Dispatcher.Invoke(() => msg_string.Text = "");
            //    }
            //});
        }

        private async void seq_save_Click(object sender, RoutedEventArgs e)
        {
            int width = 0;
            int height = 0;

            if (!string.IsNullOrWhiteSpace(MaxWidth.Text) && MaxWidth.Text != "0")
                width = Convert.ToInt32(MaxWidth.Text);

            if (!string.IsNullOrWhiteSpace(MaxHeight.Text) && MaxHeight.Text != "0")
                height = Convert.ToInt32(MaxHeight.Text);

            var text = msg_string.Text.TrimEnd(new char[] { '\r', '\n' });

            List<string> stringsequenced = new List<string>();

            for (int i = 1; i <= text.Length; i++)
            {
                char[] container = new char[i];

                for (int x = 0; x < i; x++)
                {
                    container[x] = text[x];
                }

                stringsequenced.Add(new string(container));
            }

            await Task.Run(() =>
            {
                int counter = 1;
                foreach (var subtext in stringsequenced)
                {
                    Dispatcher.Invoke(() =>
                    {
                        msg_string.Text = subtext;

                        var res = ImageRenderer.RenderToBWImg(Encoding.UTF8.GetBytes(subtext), ((scale_options)ScaleOption.SelectedItem).value, invert_checkbox.IsChecked ?? false, width, height);

                        PngBitmapEncoder pngencoder = new PngBitmapEncoder();

                        pngencoder.Frames.Add(BitmapFrame.Create(res));

                        using (var stream = new FileStream($"Test_{counter++}.png", FileMode.Create))
                            pngencoder.Save(stream);
                    });
                }
            });
        }

        private Task runtask;
        private void easteregg_Click(object sender, RoutedEventArgs e)
        {
            if (runtask == null || runtask.IsCompleted)
                runtask = Task.Run(async () =>
                {
                    const string loss = "0000010000001000101000010001010100100010101000000100000111111111110000010000001010101000010101010000101010111000000100000";

                    this.Dispatcher.Invoke(() => msg_string.Text = "");
                    this.Dispatcher.Invoke(() => MaxHeight.Text = "");
                    this.Dispatcher.Invoke(() => MaxWidth.Text = "");

                    foreach (var item in loss)
                    {
                        this.Dispatcher.Invoke(() => msg_string.Text += item);

                        await Task.Delay(10);
                    }

                    this.Dispatcher.Invoke(() => invert_checkbox.IsChecked = true);

                    await Task.Delay(3000);

                    System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=ZcoqR9Bwx1Y");

                    await Task.Delay(7000);

                    System.Diagnostics.Process.Start("https://pm1.narvii.com/6631/36671839389f4e334f66c6e1f24ddf08fd45de9f_hq.jpg");
                });
        }
    }

    public struct scale_options
    {
        public int value;
        public string text;

        public scale_options(int _value, string _text)
        {
            value = _value;
            text = _text;
        }

        public override string ToString()
        {
            return text;
        }
    }
}
