using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using LogLib;

namespace Server
{
    /// <summary>
    /// Soocket 服务器
    /// </summary>
    public class SocketServer : SocketMast, IDisposable
    {
        #region 字段
        /// <summary>
        /// 监听线程
        /// </summary>
        private Thread _listenningThread = null;
        
        /// <summary>
        /// 接受连接事件
        /// </summary>
        public event RecLink RecLinkSocket = null;
        /// <summary>
        /// 支持的客户端的数量
        /// </summary>
        private int _clientNum = 5;
        #endregion
        #region 属性
        /// <summary>
        /// 支持的客户端的数量
        /// </summary>
        public int ClientNum
        {
            get
            {
                return _clientNum;
            }

            set
            {
                _clientNum = value;
            }
        }
        #endregion
        #region 方法
        /// <summary>
        /// 启动服务器
        /// </summary>
        public override void Start()
        {
            if(!IsInit)
            {
                throw new Exception("Socket 没有被初始化或者初始化设置失败");
            }
            try
            {
                IPAddress ipAdress = IPAddress.Parse(Ip);
                _socket = new Socket(ipAdress.AddressFamily, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipPoint = new IPEndPoint(ipAdress, Port);
                _socket.Bind(ipPoint);
                if(IsOpen)
                {
                    this.Close();
                }
                //启动监听
                _socket.Listen(ClientNum);
                _listenningThread = new Thread(ListenningLink);
                _listenningThread.IsBackground = true;
                _listenningThread.Start();
                _isOpen = true;
                Log.Write("服务器启动成功!");
            }
            catch (Exception ex)
            {
                Log.Write("服务器启动失败!" + ex.Message);
            }
        }

        /// <summary>
        /// 监听连接
        /// </summary>
        private void ListenningLink()
        {
            try
            {
                Log.Write("服务器开启监听等待客户端的连接!");
                while (true)
                {
                    //当新的连接进来的时候保存连接对象
                    Socket clientSocket = _socket.Accept();
                    if(RecLinkSocket != null)
                    {
                        RecLinkSocket(clientSocket);
                    }
                    AddClientSocket(clientSocket);
                }
             }
            catch(ThreadAbortException)
            {
                Log.Write("服务器结束监听!");
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                return;
            }
        }
        /// <summary>
        /// 添加一个连接客户端
        /// </summary>
        /// <param name="client">客户端对象</param>
        protected void AddClientSocket(Socket client)
        {
            if(!_linkPool.Keys.Contains(client.RemoteEndPoint.ToString()) && IsOpen)
            {
                ///开启和客户端之间的通讯
                Thread clientThread = new Thread(CommunicationToClient);
                clientThread.IsBackground = true;
                clientThread.Start(client);
                ///添加
                _linkPool.Add(client.RemoteEndPoint.ToString(), new SocketLinkObject(client, clientThread));
                Log.Write("添加了一个客户端连接实例!");
            }
        }
        /// <summary>
        /// 删除一个连接客户端
        /// </summary>
        /// <param name="client">客户端对象</param>
        protected void DelClientSocket(Socket client)
        {
            try
            {
                string key = client.RemoteEndPoint.ToString();
                //检测是否存在
                if (_linkPool.Keys.Contains(key))
                {
                    /////删除之前要关闭线程
                    //_linkPool[key].LinkThread.Abort();
                    _linkPool.Remove(key);
                    OnCloseLinkEvent(client);
                    //client.Dispose();
                    Log.Write("断开一个客户端连接, 客户端信息为：" + key + "____");
                }
            }
            catch (Exception ex)
            {
                Log.Write("移除客户端实例失败!" + ex.Message);
            }
        }

        /// <summary>
        /// 和客户端通讯线程函数
        /// </summary>
        /// <param name="client">客户端实例</param>
        private void CommunicationToClient(object client)
        {
            Socket clientSocket = client as Socket;
            try
            {
                while(true)
                {
                    ///创建一个接受数据的字节流
                    byte[] recData = new byte[DataPageLength];
                    
                    ///接受数据
                    clientSocket.Receive(recData, DataPageLength, SocketFlags.None);
                    if (clientSocket.Poll(10, SelectMode.SelectRead))
                    {
                        throw new Exception("连接已被断开!");
                    }
                    DealMsg(recData, clientSocket);
                }
             }
            catch(ThreadAbortException abort)
            {
                Log.Write("关闭线程!");
                return;
            }
            catch (Exception ex)
            {
                DelClientSocket(clientSocket);
                Log.Write("断开一个客户端连接, 客户端信息为：" + "____" + ex.Message);
                return;
            }
            
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public override void Close()
        {
            try
            {
                if(IsOpen)
                {
                    Log.Write("开始关闭服务器!");
                    _socket.Close();
                    //关闭监听
                    _listenningThread.Abort();
                    foreach (var item in _linkPool)
                    {
                        item.Value.LinkSocket.Close();
                        if(item.Value.LinkThread.IsAlive != false)
                            item.Value.LinkThread.Abort();
                    }
                    _linkPool.Clear();
                    //_socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Write("服务器关闭失败！" + ex.Message);
            }
            finally
            {
                _isOpen = false;
            }  
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public override void Send(string msg)
        {
            byte[] sendData = GetSendBytes(msg);
            string reMO = "";
            foreach (var item in _linkPool)
            {
                if (item.Value.LinkSocket != null && !item.Value.LinkSocket.Poll(10, SelectMode.SelectRead))
                    item.Value.LinkSocket.Send(sendData, SocketFlags.None);
                else
                    reMO = item.Key;
            }
            if(reMO != "")
            {
                _linkPool.Remove(reMO);
            }
        }
        /// <summary>
        /// 发送字节数据
        /// </summary>
        /// <param name="msg"></param>
        public override void Send(byte[] msg)
        {
            if(!IsAutoSize)
                msg = GetForntBytes(msg);
            string reMO = "";
            foreach (var item in _linkPool)
            {
                if (item.Value.LinkSocket != null && !item.Value.LinkSocket.Poll(10, SelectMode.SelectRead))
                    item.Value.LinkSocket.Send(msg, SocketFlags.None);
                else
                    reMO = item.Key;
            }
            if (reMO != "")
            {
                _linkPool.Remove(reMO);
            }
        }
        /// <summary>
        /// 发送消息给指定的客户端
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="client">客户端</param>
        public void Send(string msg, Socket client)
        {
            Log.Write("send: [" + msg + "]");
            byte[] sendData = GetSendBytes(msg);
            try
            {
                client.Send(sendData, SocketFlags.None);
            }
            catch (Exception ex)
            {
                
            }
        }
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用
        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~SocketServer() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
}
