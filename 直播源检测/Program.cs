using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace 直播源检测
{
    class Program
    {
        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        private static List<string> list = new List<string>(); //有效
        private static List<string> errlist = new List<string>(); //无效
        private static int checkunm = 1;//无效重测次数
        static void Main(string[] args)
        {
            
            
            list = File.ReadAllLines(System.Environment.CurrentDirectory + "\\tv.txt").ToList();
            //int enumnum = typeof(ConsoleColor).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Length;
            

            int i = 0;
            int count = list.Count;
            foreach (string item in list.ToArray())
            {
                i++;
                if (item.Contains("http://") || item.Contains("https://"))
                {
                    string[] temp = item.Split(',');
                    if (temp.Length==1)
                    {
                        continue;
                    }
                    string[] urls = temp[1].Split('#');
                    foreach (var url in urls)
                    {
                        if (!url.Contains(".m3u8"))
                        {
                            continue;
                        }
                        if (!TV_Check(url))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"检测第{i}个,共{count}个，状态：失效直播源，有效源{list.Count}个");
                            Console.ResetColor();
                            for (int n = 0; n < checkunm; n++)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                                Console.WriteLine($"================第{n + 1}次重测无效================");
                                if (TV_Check(url))
                                {
                                    Console.ResetColor();
                                    Console.WriteLine($"重测第{i}个,共{count}个，状态：有效直播源，有效源{list.Count}个");
                                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                                    Console.WriteLine($"================第{n + 1}次重测无效================");
                                    Console.ResetColor();
                                    break;
                                }
                                if (n == checkunm - 1)
                                {
                                    list.Remove(item);
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"重测第{i}个,共{count}个，状态：失效直播源，有效源{list.Count}个");
                                    errlist.Add(item);
                                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                                    Console.WriteLine($"================第{n + 1}次重测无效================");
                                    Console.ResetColor();
                                }

                            }
                        }
                        else
                        {
                            Console.WriteLine($"检测第{i}个,共{count}个，状态：有效直播源，有效源{list.Count}个");
                        }
                    }
                    
                }
                
            }
            
            //for (int n = 0; n < checkunm; n++)
            //{
            //    Console.WriteLine($"================第{n+1}次重测无效================");
            //    TV_CheckC(errlist, ref errlist);
            //}

            
            Dictionary<string, string> tvdic = new Dictionary<string, string>();
            foreach (string item in list.ToArray())
            {
                string[] temp = item.Split(',');
                if (temp.Length == 1)
                {
                    continue;
                }
                if (tvdic.Count == 0)
                {
                    tvdic.Add(temp[0], temp[1]);
                }
                else
                {
                    foreach (KeyValuePair<string, string> item1 in tvdic)
                    {
                        KeyValuePair<string, string> tt = tvdic.FirstOrDefault(x => x.Key.Equals(temp[0]));
                        if (tt.Key == null)
                        {
                            if (item1.Key.Equals(temp[0]))
                            {
                                if (tvdic[temp[0]].Contains(temp[1]))
                                {
                                    continue;
                                }
                                tvdic[temp[0]] = tvdic[temp[0]] + "#" + temp[1];
                                break;
                            }
                            else
                            {

                                tvdic.Add(temp[0], temp[1]);
                                break;
                            }
                        }
                        else
                        {
                            if (tvdic[temp[0]].Contains(temp[1]))
                            {
                                continue;
                            }
                            tvdic[temp[0]] = tvdic[temp[0]] + "#" + temp[1];
                            break;
                        }

                    }
                }



            }
            List<string> list1 = new List<string>();
            foreach (KeyValuePair<string, string> item in tvdic)
            {
                list1.Add(item.Key + "," + item.Value);
            }
            //string time = DateTime.Now.Ticks.ToString();
            string time = DateTime.Now.ToString("yyMMddHHmmss");
            File.WriteAllLines(System.Environment.CurrentDirectory + $"\\ok_tv{time}.txt", list1.ToArray());
            File.WriteAllLines(System.Environment.CurrentDirectory + $"\\err_tv{time}.txt", errlist.ToArray());

         }
        static void TV_CheckC(List<string> list2, ref List<string> errlist)
        {
            errlist = new List<string>();
            int i = 0;
            int count = list2.Count;
            List<ConsoleColor> colornum = new List<ConsoleColor>();
            foreach (var item in Enum.GetValues(typeof(ConsoleColor)))
            {
                //Type t = item.GetType();
                //string a = item.ToString();
                //Console.ForegroundColor = (ConsoleColor)item;
                if (item.ToString().ToLower().Contains("white") || item.ToString().ToLower().Contains("black") || item.ToString().ToLower().Contains("gray"))
                {
                    continue;
                }
                colornum.Add((ConsoleColor)item);
            }
            ConsoleColor errcolor = colornum[new Random().Next(colornum.Count)];

            foreach (string item in list2.ToArray())
            {
                i++;
                if (item.Contains("http://") || item.Contains("https://"))
                {
                    string[] temp = item.Split(',');
                    string url = temp[1];
                    if (!TV_Check(url))
                    {
                        list2.Remove(item);
                        Console.ForegroundColor = errcolor;
                        // Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"失效重新检测第{i}个,共{count}个，状态：失效直播源");
                        Console.ResetColor();
                        errlist.Add(item);
                    }
                    else
                    {
                        list.Add(item);
                        Console.WriteLine($"失效重新检检测第{i}个,共{count}个，状态：有效直播源");
                    }
                }

            }
        }
        static bool TV_Check(string url)
        {
            //解决https不安全提示
            //ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)4080; //SecurityProtocolType.Tls ; //Or SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.SystemDefault;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            ServicePointManager.DefaultConnectionLimit = 1024;

            string Content_Type = "application/x-www-form-urlencoded; charset=UTF-8";
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
            hwr.Method = "get";
            hwr.ContentType = Content_Type;
            hwr.KeepAlive = true;
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4919.0 Safari/537.36";
            //hwr.AllowAutoRedirect = false;
            hwr.Timeout = 15000;

            bool isok = true;
            try
            {
                HttpWebResponse hwrs = (HttpWebResponse)hwr.GetResponse();
                HttpStatusCode hsc = hwrs.StatusCode;
                int errcode = (int)hsc;
            }
            catch (WebException ex)
            {

                isok = false;
            }
            
            return isok;
        }
       
    }
}
