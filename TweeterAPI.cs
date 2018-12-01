using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TweetSharp;

namespace ТвиттерАПИ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class Tweet
        {
            public string Autor;
            public string Retwettee;
            public Tweet(string Autor, string Retwettee)
            {
                this.Autor = Autor;
                this.Retwettee = Retwettee;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string keyword = textBox3.Text;
            int count = Convert.ToInt32(textBox2.Text);
            File.Delete(keyword + ".txt");

            int tweetcount = 0;
            long max_id = -1L;

            var tweets_search = new TwitterSearchResult();
            var resultList = new List<TwitterStatus>();

            var twitterService = new TwitterService("key", "key");
            twitterService.AuthenticateWith("key", "key");
            var options = new SearchOptions()
            {
                Q = keyword,
                Lang = textBox4.Text,
                Resulttype = TwitterSearchResultType.Mixed,
                Count = Convert.ToInt32(count)
            };

            while (tweetcount < Convert.ToInt32(textBox1.Text))
            {
                try
                {
                    if (max_id > 0) options.SinceId = max_id - 1;
                    tweets_search = twitterService.Search(options);
                }
                catch (Exception ex)
                {
                    using (var fulls = File.AppendText("log.txt"))
                    {
                        fulls.WriteLine("Exception: " + ex.Data);
                        fulls.WriteLine("MaxID: " + max_id);
                    }
                }

                resultList = new List<TwitterStatus>(tweets_search.Statuses);

                using (var full = File.AppendText(keyword + ".txt"))
                {
                    foreach (TwitterStatus ts in resultList)
                        full.WriteLine(ts.RawSource);
                }

                if (resultList.Count == 0) break;
                max_id = resultList.Last().Id;
                tweetcount += resultList.Count;
            }
        }
    }
}
