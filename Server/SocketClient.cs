using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Server
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class SocketClient : SocketMast
    {
        #region 属性
        /// <summary>
        /// 连接事件
        /// </summary>
        public event RecLink LinkEvent = null;
        /// <summary>
        /// 接受消息线程
        /// </summary>
        private Thread _recMsgThread = null;
        #endregion
        #region 访问器
        /// <summary>
        /// 是否连接着
        /// </summary>
        public bool IsLink
        {
            get
            {
                if (Socket.Poll(10, SelectMode.SelectRead))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        #endregion
        #region 方法
        /// <summary>
        /// 关闭
        /// </summary>
        public override void Close()
        {
            try
            {
                if (IsOpen)
                {
                    _isOpen = false;
                    _socket.Close();
                    _recMsgThread.Abort();
                    OnCloseLinkEvent(_socket);
                    _recMsgThread = null;
                    //关闭监听
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="msg"></param>
        public override void Send(string msg)
        {
            if(!IsAutoSize)
                _socket.Send(GetSendBytes(msg), DataPageLength, SocketFlags.None);
            else
                _socket.Send(GetSendBytes(msg));
        }
        /// <summary>
        /// 发送字节流数据
        /// </summary>
        /// <param name="msg"></param>
        public override void Send(byte[] msg)
        {
            if(!IsAutoSize)
                _socket.Send(GetForntBytes(msg), DataPageLength, SocketFlags.None);
            else
                _socket.Send(msg);
        }
        /// <summary>
        /// 启动
        /// </summary>
        public override void Start()
        {
            if (!IsInit)
            {
                throw new Exception("Socket 没有被初始化或者初始化设置失败");
            }
            try
            {
                IPAddress ipAdress = IPAddress.Parse(Ip);
                _socket = new Socket(ipAdress.AddressFamily, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipPoint = new IPEndPoint(ipAdress, Port);
                if (IsOpen)
                {
                    this.Close();
                }
                //连接服务器
                if(this.ReciveBuffSize != -1)
                    _socket.ReceiveBufferSize = this.ReciveBuffSize;
                 _socket.Connect(ipPoint);
                LinkEvent?.Invoke(_socket);
                _recMsgThread = new Thread(RecMsg);
                _recMsgThread.IsBackground = true;
                _recMsgThread.Start();
                _isOpen = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 
        /// <summary>
        /// 接受消息
        /// </summary>
        protected void RecMsg()
        {
            List<byte> data = new List<byte>();
            ///创建一个接受数据的字节流
            byte[] recData = new byte[DataPageLength];
            while (true)
            {
                if(!_socket.Connected)
                {
                    Close();
                    break;
                }
                try
                {
                    data.Clear();
                    if (this.IsReciverForAll)
                    {
                        int len = 0;
                        len = _socket.Receive(recData);
                        while (_socket.Available > 0 || len > 0)
                        {
                            if(len == 0)
                                len = _socket.Receive(recData);
                            if (len == recData.Length)
                                data.AddRange(recData);
                            else
                            {
                                for (int i = 0; i < len; i++)
                                {
                                    data.Add(recData[i]);
                                }
                            }
                            len = 0;
                        }
                        //clientSocket.Receive(data);
                    }
                    else
                    {
                        
                        ///接受数据
                        _socket.Receive(recData, DataPageLength, SocketFlags.None);
                        data.AddRange(recData);
                    }

                    if (IsCheckLink)
                    {
                        if (_socket.Poll(10, SelectMode.SelectRead))
                        {
                            throw new Exception("连接已被断开!");
                        }
                    }
                }
                catch(ThreadAbortException abe)
                {
                    break;
                }
                catch (Exception ex)
                {
                    break;
                }
                ///处理消息
                DealMsg(data.ToArray(), _socket);
            }
        }
        #endregion
    }
}
