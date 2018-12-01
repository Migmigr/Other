using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Parser
{
    class Program
    {
        /// <summary>
        /// Класс отвечающий за пура Автор-ссылка на автора
        /// </summary>
        public class Autors
        {
            public string Name { get; set; }
            public string Reference { get; set; }
        }
        static void Main(string[] args)
        {
            Directory.CreateDirectory("Site");
            HttpWebRequest proxy_request =
                (HttpWebRequest)WebRequest.Create("http://www.online-literature.com/author_index.php");
            proxy_request.Method = "GET";
            proxy_request.ContentType = "application/x-www-form-urlencoded";
            proxy_request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.89 Safari/532.5";
            proxy_request.KeepAlive = true;
            HttpWebResponse resp = proxy_request.GetResponse() as HttpWebResponse;

            List<Autors> AutorsList = new List<Autors>();
            MatchCollection referencematch = null; MatchCollection aut = null;
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251)))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    referencematch = Regex.Matches(line, "href = \"[\\w_/:\\.-]{1,}\"");
                    aut = Regex.Matches(line, "title = \"[ \\w_/:\\.-]{1,}\"");
                    if (referencematch.Count != 0)
                    {
                        for (int i = 0; i < referencematch.Count; i++)
                            AutorsList.Add(new Autors()
                            {
                                Name = Regex.Replace(aut[i].Value, "(title = )|(\")", ""),
                                Reference = Regex.Replace(referencematch[i].Value, "(href = )|(\")", "")
                            });
                        break;
                    }
                }
            }
            int colvo = 0;
            int flag = 0;
            for (int i = 0; i < AutorsList.Count; i++)
            {
                Console.WriteLine("Авторы: " + (i + 1) + " from " + AutorsList.Count);
                colvo = 0;
                flag = 0;
                List<string> pr = new List<string>();
                proxy_request = (HttpWebRequest)WebRequest.Create(AutorsList[i].Reference);
                resp = proxy_request.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251)))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (flag == 1 && Regex.Match(line, "(American)|(English)").Value != "")
                            colvo++;
                        if (flag == 2 && Regex.Match(line, "http://schema.org/Book").Value != "")
                        {
                            resp.Close();
                            Directory.CreateDirectory("Site\\" + AutorsList[i].Name);
                            start(AutorsList[i], Regex.Matches(line, "href = \"[\\w_/:\\.-]{1,}\""));
                            break;
                        }

                        if (Regex.Match(line, "<div id=\"introduction\" class=\"panel-body panel-tabbed panel-inView\">").Value != "")
                            flag = 1;
                        if (Regex.Match(line, "<!-- Litnet Author Bio Inline -->").Value != "")
                        {
                            if (colvo == 0)
                            {
                                Console.WriteLine(AutorsList[i].Name + " not American or English");
                                break;
                            }
                            else
                            {
                                Console.WriteLine(AutorsList[i].Name + " American or English");
                                flag = 2;
                            }
                        }
                    }
                }
            }
        }

        static void start(Autors a, MatchCollection pr)
        {
            List<string> text = new List<string>();

            HttpWebRequest proxy_request = (HttpWebRequest)WebRequest.Create(Regex.Replace(pr[0].Value, "(href = )|(\")", ""));
            proxy_request.Method = "GET";
            proxy_request.ContentType = "application/x-www-form-urlencoded";
            proxy_request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.89 Safari/532.5";
            proxy_request.KeepAlive = true;
            HttpWebResponse resp = proxy_request.GetResponse() as HttpWebResponse;
            resp.Close();
            int num = 0;
            int flag = 0;
            foreach (Match l in pr)
            {
                num++;
                Console.WriteLine("Произведения: " + num + " from " + pr.Count);
                try
                {
                    text = new List<string>();
                    flag = 0;
                    MatchCollection aa = null;
                    proxy_request = (HttpWebRequest)WebRequest.Create(Regex.Replace(l.Value, "(href = )|(\")", ""));
                    resp = proxy_request.GetResponse() as HttpWebResponse;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251)))
                    {
                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (Regex.Match(line, "(<div id=\"introduction\" class=\"panel-body panel-tabbed panel-inView\">)").Value != "")
                                flag = 1;
                            if (Regex.Match(line, "(<h1>[^<])").Value != "")
                                flag = 2;
                            if (Regex.Match(line, "(Fan of this book?)").Value != "")
                                flag = 0;
                            if (flag == 2 && Regex.Match(line, "<p class=\"breadcrumb\">").Value != "")
                                flag = 0;

                            if (Regex.Match(line, "http://schema.org/CreativeWork").Value != "")
                                aa = Regex.Matches(line, "href = \"[\\w_/:\\.-]{1,}\"");
                            if (flag == 1)
                                foreach (Match sa in Regex.Matches(line, "[^i]>[^<>]{1,}<"))
                                    text.Add(sa.Value);
                            if (flag == 2)
                                text.Add(line);
                        }
                    }
                    resp.Close();
                    int error = (aa != null) ? error = docach(text, aa) : 0;

                    if (error == 0)
                    {
                        string g = "Site\\" + a.Name + "\\" + l.Value.Split('/')[l.Value.Split('/').Length - 2] + ".txt";
                        using (StreamWriter sr = new StreamWriter(g))
                        {
                            Console.WriteLine(a.Name + " " + l.Value.Split('/')[l.Value.Split('/').Length - 2]);
                            foreach (string s in text)
                                sr.WriteLine(s);
                        }
                    }
                    else
                    {
                        using (StreamWriter sr = File.AppendText("log.txt"))
                            sr.WriteLine(a.Name + " " + l);
                    }
                }
                catch (Exception e)
                {
                    using (StreamWriter sr = File.AppendText("log.txt"))
                    {
                        sr.WriteLine(a.Name + " " + l);
                    }
                }
            }
        }
        static int docach(List<string> text, MatchCollection gl)
        {
            try
            {
                HttpWebRequest proxy_request = (HttpWebRequest)WebRequest.Create(Regex.Replace(gl[0].Value, "(href = )|(\")", ""));
                proxy_request.Method = "GET";
                proxy_request.ContentType = "application/x-www-form-urlencoded";
                proxy_request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.89 Safari/532.5";
                proxy_request.KeepAlive = true;
                HttpWebResponse resp = proxy_request.GetResponse() as HttpWebResponse;
                resp.Close();
                foreach (Match l in gl)
                {
                    int flag = 0;
                    proxy_request = (HttpWebRequest)WebRequest.Create(Regex.Replace(l.Value, "(href = )|(\")", ""));
                    resp = proxy_request.GetResponse() as HttpWebResponse;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251)))
                    {
                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (Regex.Match(line, "<h1>").Value != "")
                                flag = 1;
                            if (flag == 1 && Regex.Match(line, "<p class=\"breadcrumb\">").Value != "")
                                break;
                            if (flag == 1)
                                text.Add(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return 1;
            }
            return 0;
        }
    }
}
