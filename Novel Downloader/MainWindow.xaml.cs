using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Ookii.Dialogs.Wpf;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Novel_Downloader
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

        static string Zeroe(int x, int y)
        {
            string str = x + "";
            y -= str.Length + 1;
            for (int i = 0; i < y + 1; i++)
            {
                str = "0" + str;
            }
            return str;
        }

        private static readonly Regex _regex = new Regex("[^0-9]+");

        private void PreventLetters(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckErrors()) return;

            int totalChapters = int.Parse(MaxChapterTXT.Text) - int.Parse(MinChapterTXT.Text);
            InitializeProgress(totalChapters);

            string downloadFolder = DownloadFolderTXT.Text;
            string chapterName = ChapnameTXT.Text;
            string chapterContentID = ContentIDTXT.Text;
            string URLText = URLTextBox.Text;
            int minChapter = int.Parse(MinChapterTXT.Text);
            int maxChapter = int.Parse(MaxChapterTXT.Text);

            await Task.Run((() =>
            {
                Download(downloadFolder, chapterName, chapterContentID, URLText, minChapter, maxChapter, totalChapters);
            }));
        }

        private void Download(string downloadFolder, string chapterName, string chapterContentID, string URLText, int minChapter, int maxChapter, int totalChapters)
        {
            HtmlWeb web = new HtmlWeb();
            Directory.CreateDirectory(downloadFolder);
            for (int i = minChapter; i <= maxChapter; i++)
            {
                HtmlAgilityPack.HtmlDocument doc = web.Load(URLText.Replace("[i]", i + ""));

                using (StreamWriter wr = new StreamWriter(
                    downloadFolder + "/" + chapterName.Replace("[i]", "" +
                    Zeroe(i, maxChapter.ToString().Length)) + ".txt", false))
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(doc.GetElementbyId(chapterContentID).OuterHtml);
                    HtmlNodeCollection paragraphs = doc2.DocumentNode.SelectNodes("//p");
                    string[] str = new string[paragraphs.Count];
                    for (int j = 0; j < str.Length; j++)
                    {
                        str[j] = paragraphs[j].InnerText;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LoadingBar.Value++;
                        ProgressLabel.Content = "Downloaded: " + LoadingBar.Value + "/" + totalChapters;
                    });

                    wr.WriteLine(string.Join("\n\n", str));
                }
            }
        }

        private void InitializeProgress(int totalChapters)
        {
            LoadingBar.Maximum = totalChapters;
            ProgressLabel.Content = "Downloaded: " + "0/" + totalChapters;
        }

        private bool CheckErrors()
        {
            try
            {
                int.Parse(MinChapterTXT.Text);
                int.Parse(MaxChapterTXT.Text);
            }
            catch
            {
                MessageBox.Show("Please enter valid chapter numbers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; ;
            }

            if (int.Parse(MinChapterTXT.Text) > int.Parse(MaxChapterTXT.Text))
            {
                MessageBox.Show("Starting chapter number must be smaller than or equal to large chapter number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; ;
            }

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc;
            try
            {
                doc = web.Load(URLTextBox.Text.Replace("[i]", MinChapterTXT.Text + ""));
            }
            catch
            {
                MessageBox.Show("URL is not valid, or chapter number does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc2.LoadHtml(doc.GetElementbyId(ContentIDTXT.Text).OuterHtml);
            }
            catch
            {
                MessageBox.Show("Could not find chapter content ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void Find_folder_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new VistaFolderBrowserDialog();
            if (fbd.ShowDialog() == true)
            {
                DownloadFolderTXT.Text = fbd.SelectedPath;
            }
        }
    }
}
