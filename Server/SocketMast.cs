using LogLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    /// <summary>
    /// 获取到连接
    /// </summary>
    /// <param name="socket"></param>
    public delegate void RecLink(Socket socket);
    /// <summary>
    /// 断开连接事件
    /// </summary>
    /// <param name="socket"></param>
    public delegate void ClsedLink(Socket socket);
    /// <summary>
    /// 接受数据委托
    /// </summary>
    /// <param name="datas"></param>
    public delegate void RecDataFrom(RecDataObject obj);
    public abstract class SocketMast
    {
        #region 字段
        /// <summary>
        /// 接受数据事件
        /// </summary>
        public event RecDataFrom RecDataEvent = null;
        /// <summary>
        /// socket对象
        /// </summary>
        protected Socket _socket = null;
        /// <summary>
        /// 是否是自动尺寸
        /// </summary>
        private bool _isAutoSize = false;
        /// <summary>
        /// 服务器的IP地址
        /// </summary>
        private string _ip = string.Empty;
        /// <summary>
        /// 监听的端口号
        /// </summary>
        private int _port = 7878;
        /// <summary>
        /// 是否已经初始化
        /// </summary>
        private bool _isInit = false;
        /// <summary>
        /// 设置数据包长度
        /// </summary>
        private int _dataPageLength = 4096;
        /// <summary>
        /// socket的连接对象池
        /// </summary>
        protected Dictionary<string, SocketLinkObject> _linkPool = new Dictionary<string, SocketLinkObject>();
        /// <summary>
        /// 是否已经启动
        /// </summary>
        protected bool _isOpen = false;
        /// <summary>
        /// 接受数据的编码格式
        /// </summary>
        private RecEncodingType encodingType = RecEncodingType.UTF8;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ClsedLink CloseLinkEvent = null;

        #endregion
        #region 属性
        /// <summary>
        /// socket对象
        /// </summary>
        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }
        /// <summary>
        /// 服务器的IP地址
        /// </summary>
        public string Ip
        {
            get
            {
                return _ip;
            }

            set
            {
                _ip = value;
            }
        }
        /// <summary>
        /// 监听的端口号
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
            }
        }
        /// <summary>
        /// socket的连接对象池
        /// </summary>
        public Dictionary<string, SocketLinkObject> LinkPool
        {
            get
            {
                return _linkPool;
            }
        }
        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public bool IsInit
        {
            get
            {
                return _isInit;
            }
        }
        /// <summary>
        /// 设置数据包长度
        /// </summary>
        public int DataPageLength
        {
            get
            {
                return _dataPageLength;
            }

            set
            {
                if(_socket != null)
                {
                    _socket.ReceiveBufferSize = value;
                    _dataPageLength = value;
                }
            }
        }
        /// <summary>
        /// 是否已经启动
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }
        /// <summary>
        /// 接受数据的编码格式
        /// </summary>
        public RecEncodingType EncodingType
        {
            get
            {
                return encodingType;
            }

            set
            {
                encodingType = value;
            }
        }
        /// <summary>
        /// 是否是自动尺寸
        /// </summary>
        public bool IsAutoSize
        {
            get
            {
                return _isAutoSize;
            }

            set
            {
                _isAutoSize = value;
            }
        }

        #endregion
        #region 方法
        public SocketMast()
        {

        }
        /// <summary>
        /// 初始化Socket
        /// </summary>
        /// <param name="IP">绑定的IP</param>
        /// <param name="Port">绑定的端口</param>
        /// <returns>是否初始化成功</returns>
        public bool InitSocket(IP Ip, int Port)
        {
            if ((Port < 1000 || Port > 9999) || !IsFrontIp(Ip))
                return false;
            this.Ip = Ip;
            this.Port = Port;
            _isInit = true;
            Log.Write("服务器初始化设置成功！");
            return true;
        }
        /// <summary>
        /// 启动Socket
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// 关闭
        /// </summary>
        public abstract void Close();
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public abstract void Send(string msg);
        /// <summary>
        /// 发送字节数据信息
        /// </summary>
        /// <param name="msg">消息数据</param>
        public abstract void Send(byte [] msg);
        /// <summary>
        /// 获取要发送的字节流
        /// </summary>
        /// <param name="msg">要发送的字符串</param>
        /// <returns>要发送的字节流</returns>
        public byte []GetSendBytes(string msg)
        {
            byte[] bmsg = null;
            ///转换为字符串
            switch (EncodingType)
            {
                case RecEncodingType.UTF8:
                    bmsg = Encoding.UTF8.GetBytes(msg);
                    break;
                case RecEncodingType.UTF7:
                    bmsg = Encoding.UTF7.GetBytes(msg);
                    break;
                case RecEncodingType.UTF32:
                    bmsg = Encoding.UTF32.GetBytes(msg);
                    break;
                case RecEncodingType.Unicode:
                    bmsg = Encoding.Unicode.GetBytes(msg);
                    break;
                case RecEncodingType.ASCII:
                    bmsg = Encoding.ASCII.GetBytes(msg);
                    break;
                default:
                    bmsg = Encoding.Default.GetBytes(msg);
                    break;
            }
            if (!_isAutoSize)
                return GetForntBytes(bmsg);
            else
                return bmsg;
        }
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void DealMsg(byte [] msg, Socket client)
        {
            string msgStr = "";
            ///转换为字符串
            switch (EncodingType)
            {
                case RecEncodingType.UTF8:
                    msgStr = Encoding.UTF8.GetString(msg);
                    break;
                case RecEncodingType.UTF7:
                    msgStr = Encoding.UTF7.GetString(msg);
                    break;
                case RecEncodingType.UTF32:
                    msgStr = Encoding.UTF32.GetString(msg);
                    break;
                case RecEncodingType.Unicode:
                    msgStr = Encoding.Unicode.GetString(msg);
                    break;
                case RecEncodingType.ASCII:
                    msgStr = Encoding.ASCII.GetString(msg);
                    break;
                default:
                    msgStr = Encoding.Default.GetString(msg);
                    break;
            }
            msgStr.Trim('\0');
            this.SendDataTo(new RecDataObject(client, msg, msgStr));
        }
        /// <summary>
        /// Ip的格式是否符合规范
        /// </summary>
        /// <param name="Ip">Ip</param>
        /// <returns>true为符合</returns>
        public bool IsFrontIp(string Ip)
        {
            string[] ips = Ip.Split('.');
            if (ips.Length != 4)
                return false;
            foreach(string ipitem in ips)
            {
                int item = int.Parse(ipitem);
                if (item < 0 || item > 255)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 规范化发送的数据
        /// </summary>
        /// <param name="sendData">发送的数据</param>
        /// <returns>规范化发送的数据</returns>
        public byte[] GetForntBytes(byte [] sendData)
        {
            byte[] reData = new byte[DataPageLength];
            Array.Copy(sendData, reData, sendData.Length);
            return reData;
        }
        /// <summary>  
        /// 获取本地IP  
        /// </summary>  
        /// <param name="pos">由于获取的有很多的地址如本地地址局域网地址公网地址根据不同的pos获取不同的地址</param>  
        /// <returns></returns>  
        public static String GetLocalIP(int pos = 1)
        {
            try
            {
                // 获得本计算机的名称  
                string hostName = Dns.GetHostName();

                // 通过名字获得IP地址  
                IPHostEntry hostInfo = Dns.GetHostByName(hostName);
                IPAddress[] address = hostInfo.AddressList;
                // 创建 ipAddress 数组来存放字符串形式的IP地址  
                string[] ipAddress = new string[address.Length];
                for (int index = 0; index < address.Length; index++)
                {
                    ipAddress[index] = address[index].ToString();
                }
                pos = ipAddress.Length - 1;
                return Dns.Resolve(Dns.GetHostName()).AddressList[pos].ToString();
            }
            catch (Exception exc)
            {
                //Error_Info = exc.Message;  
                //WriteLogger(exc.ToString());  
                return "";
            }

        }
        /// <summary>
        /// 分发数据
        /// </summary>
        /// <param name="recObj"></param>
        protected void SendDataTo(RecDataObject recObj)
        {
            Thread dataTp = new Thread(DataTo);
            dataTp.IsBackground = true;
            dataTp.Start(recObj);
        }

        /// <summary>
        /// 数据分发具体线程
        /// </summary>
        /// <param name="obj"></param>
        protected void DataTo(object obj)
        {
            OnRecDataEvent(obj as RecDataObject);
        }
        #endregion
        /// <summary>
        /// 主动引发断开连接事件
        /// </summary>
        /// <param name="sock"></param>
        public void OnCloseLinkEvent(Socket sock)
        {
            CloseLinkEvent?.Invoke(sock);
        }
        /// <summary>
        /// 主动引发数据接收事件
        /// </summary>
        /// <param name="obj"></param>
        public void OnRecDataEvent(RecDataObject obj)
        {
            lock (this)
            {
                RecDataEvent?.Invoke(obj);
            }
        }
    }
}
