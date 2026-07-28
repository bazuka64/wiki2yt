using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random = new Random();
        HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            初期化時キーワード取得();
        }

        async void 初期化時キーワード取得()
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (dummy)");
            string wiki_url = "https://ja.wikipedia.org/w/api.php?action=query&list=random&rnnamespace=0&rnlimit=10&format=json";
            string json = await client.GetStringAsync(wiki_url);
            JsonDocument doc = JsonDocument.Parse(json);

            int i = 0;
            foreach (Button button in grid.Children)
            {
                string word = doc.RootElement.GetProperty("query").GetProperty("random")[i].GetProperty("title").GetString();
                //System.Diagnostics.Debug.WriteLine(word);
                button.Content = word;
                i++;
            }
        }

        private async void ボタン押した時キーワード取得wiki_links(string word)
        {
            if (word == "") return;

            string url = $"https://ja.wikipedia.org/w/api.php?action=query&prop=links&titles={Uri.EscapeDataString(word)}&format=json&pllimit=max";
            string json = await client.GetStringAsync(url);
            JsonDocument doc = JsonDocument.Parse(json);
            //File.WriteAllText("debug.json", json, Encoding.UTF8);

            JsonElement pages = doc.RootElement.GetProperty("query").GetProperty("pages");
            JsonElement links_array = JsonElement.Parse("[]");
            List<string> list = new List<string>();
            string[] array = new string[0];
            foreach (JsonProperty page in pages.EnumerateObject())
            {
                bool a = page.Value.TryGetProperty("links", out links_array);
                if (a)
                {

                    foreach (var link in links_array.EnumerateArray())
                    {
                        list.Add(link.GetProperty("title").GetString()!);
                        array = list.ToArray();
                        Random.Shared.Shuffle<string>(array);
                    }
                }
                break;
            }

            int i = 0;
            foreach (Button button in grid.Children)
            {
                if (i < array.Length)
                {
                    string word_from_json = array[i];
                    button.Content = word_from_json;
                }
                else
                {
                    button.Content = "";
                }
                i++;
            }
        }
        private async void ボタン押した時キーワード取得google_suggest(string word)
        {
            HttpClient client = new HttpClient();
            string url = $"https://suggestqueries.google.com/complete/search?client=firefox&hl=ja&ie=utf-8&oe=utf-8&q={Uri.EscapeDataString(word)}";
            string json = await client.GetStringAsync(url);
            JsonDocument doc = JsonDocument.Parse(json);
            int length = doc.RootElement[1].GetArrayLength();
            int i = 0;
            foreach (Button button in grid.Children)
            {
                if (i < length)
                {
                    string word_from_json = doc.RootElement[1][i].GetString();
                    button.Content = word_from_json;
                }
                else
                {
                    button.Content = "";
                }
                i++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                //FileName = $"https://www.google.com/search?q={((Button)sender).Content}",
                FileName = $"https://www.youtube.com/results?search_query={((Button)sender).Content}",
                UseShellExecute = true
            });
            ボタン押した時キーワード取得wiki_links((string)((Button)sender).Content);
        }
    }
}