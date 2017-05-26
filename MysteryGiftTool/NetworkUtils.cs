using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using MysteryGiftTool.Properties;

namespace MysteryGiftTool
{
    public static class NetworkUtils
    {
        public static byte[] TryDownload(string file)
        {
            try
            {
                return new WebClient().DownloadData(file);
            }
            catch (WebException)
            {
                Program.Log($"Failed to download {file}.");
                return null;
            }
        }

        public static string MakeCertifiedRequest(string URL, bool json = false)
        {
            var ClCertA = new X509Certificate2(Resources.ClCertA, Resources.ClCertA_Password);
            var wr = WebRequest.Create(new Uri(URL)) as HttpWebRequest;
            wr.UserAgent = $"CTR NUP 040600 {DateTime.Now.ToString("MMMM dd yyyy HH:mm:ss")}";
            wr.KeepAlive = true;
            if (json)
                wr.Accept = "application/json";
            wr.Method = WebRequestMethods.Http.Get;
            wr.ClientCertificates.Clear();
            wr.ClientCertificates.Add(ClCertA);
            string response;
            try
            {
                using (var resp = wr.GetResponse() as HttpWebResponse)
                {
                    response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                response = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
            return response;
        }

    }
}