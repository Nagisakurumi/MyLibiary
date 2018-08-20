using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    public class IPException : Exception
    {
        public string ErrorIP = "";

        public IPException(string errorip) : base("ip错误，不是正确的ip")
        {
            ErrorIP = errorip;
        }
    }

    public class IP
    {
        /// <summary>
        /// ip地址
        /// </summary>
        private string ip = "127.0.0.1";
        internal string getIp()
        {
            return ip;
        }
        /// <summary>
        /// Ip
        /// </summary>
        /// <param name="ip"></param>
        public IP(string ip)
        {
            this.ip = changeToIp(ip);
        }
        /// <summary>
        /// 转换为Ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string changeToIp(string ip)
        {
            List<string> ips = ip.Split('.').ToList();
            if(ips.Count != 4)
            {
                throw new IPException(ip);
            }
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in ips)
                {
                    int ipvalue = Convert.ToInt32(item);
                    if(ipvalue < 0 || ipvalue > 255)
                    {
                        throw new IPException(ip);
                    }
                    sb.Append(ipvalue.ToString());
                    sb.Append(".");
                }
                sb.Remove(sb.Length - 1, 1);
                ips.Clear();
                ips = null;
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new IPException(ip);
                LogLib.Log.Write(ex);
            }
            
        }

        public static implicit operator IP(string ip)
        {
            return new IP(ip);
        }

        public static implicit operator string(IP ip)
        {
            return ip.ip;
        }
    }
}
