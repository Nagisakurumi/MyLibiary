using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    /// <summary>
    /// 接受数据对象
    /// </summary>
    public class RecDataObject : IDisposable
    {
        /// <summary>
        /// 套接字对象
        /// </summary>
        public Socket Socket;
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Datas;
        /// <summary>
        /// 字符串数据
        /// </summary>
        public string StringDatas;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Socket">套接字</param>
        /// <param name="Datas">数据</param>
        public RecDataObject(Socket Socket, byte[] Datas, string StringDatas)
        {
            this.Socket = Socket;
            this.Datas = Datas;
            this.StringDatas = StringDatas;
        }

        ~RecDataObject()
        {
            this.Dispose();
        }
        /// <summary>
        /// 去除结束符
        /// </summary>
        public void DelEndString()
        {
            this.StringDatas = this.StringDatas.Split('\0')[0];
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Datas = null;
                    StringDatas = null;
                }
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~RecDataObject() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    /// <summary>
    /// 接受数据格式
    /// </summary>
    public enum RecEncodingType
    {
        UTF8,
        ASCII,
        UTF7,
        Unicode,
        UTF32,
        Defult,
    }

    /// <summary>
    /// Socket要实例化的类型
    /// </summary>
    public enum SocketType
    {
        /// <summary>
        /// 服务器
        /// </summary>
        Server = 1,
        /// <summary>
        /// 客户端
        /// </summary>
        Client = 2
    }

    /// <summary>
    /// socket的连接对象
    /// </summary>
    public struct SocketLinkObject
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        public Socket LinkSocket { get; set; }
        /// <summary>
        /// 连接监听线程
        /// </summary>
        public Thread LinkThread { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="LinkSocket">连接对象</param>
        public SocketLinkObject(Socket LinkSocket, Thread LinkThread)
        {
            this.LinkSocket = LinkSocket;
            this.LinkThread = LinkThread;
        }
    }
}
