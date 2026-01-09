using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lab_gaming
{
    public class WordleClient
    {
        private Random rnd;
        public string ip;

        public ServerIP sip_cache;

        public bool isonline;
        public WordleClient()
        {
            rnd = new Random();
            isonline = false;
            SETCACHEDIP();
        }
        private async void SETCACHEDIP()
        {
            List<ServerIP> ipquery = await App.db.RetrieveServerIP();
            sip_cache = ipquery[0];
            ip = sip_cache.ip;
        }
        public async Task<bool> PING()
        {
            bool result = false;
            try
            {
                TcpClient client = new TcpClient();

                await client.ConnectAsync(ip, 65432);
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes("ping");
                await stream.WriteAsync(data, 0, data.Length);
                data = new byte[1024];
                int bytes = await stream.ReadAsync(data, 0, data.Length);
                stream.Close();
                client.Close();
                string response = "";
                response = Encoding.UTF8.GetString(data, 0, bytes);
                if (response == "ECHO") { result = true; } else { result = false; }
                
            }
            catch (Exception e) { result = false; }
            return result;
        }

        public string REQUESTINDEX(string locale)
        {
            string result;
            try
            {
                TcpClient client = new TcpClient(ip, 65432);
                NetworkStream stream = client.GetStream();
                int randomindex = rnd.Next(0, 40000);
                string request = "wordleindex+" + locale + "+" + randomindex;
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);
                data = new byte[1024];
                int bytes = stream.Read(data, 0, data.Length);
                stream.Close();
                client.Close();
                string response = "";
                response = Encoding.UTF8.GetString(data, 0, bytes);
                if (response.Length == 5) { result = response; } else { result = "FAIL"; }

            }
            catch (Exception e) { result = "FAIL"; }
            return result;
        }

        public string REQUESTWORD(string locale, string word)
        {
            string result;
            try
            {
                TcpClient client = new TcpClient(ip, 65432);
                NetworkStream stream = client.GetStream();
                string request = "wordleword+" + locale + "+" + word;
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);
                data = new byte[1024];
                int bytes = stream.Read(data, 0, data.Length);
                stream.Close();
                client.Close();
                string response = "";
                response = Encoding.UTF8.GetString(data, 0, bytes);
                if (response == "TRUE") { result = response; } else { result = "FAIL"; }

            }
            catch (Exception e) { result = "FAIL"; }
            return result;
        }
    }
}
