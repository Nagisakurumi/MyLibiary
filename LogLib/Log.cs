using System;
using System.IO;
using System.Text;

namespace LogLib
{
    /// <summary>
    /// 日志文件类
    /// </summary>
    public class LogInfo
    {
        /// <summary>
        /// 日志类
        /// </summary>
        public static LogInfo Log { get; } = new LogInfo();
        /// <summary>
        /// 单个日志文件最大允许的存储量（字节）
        /// </summary>
        //public static long MaxFileSize { get; set; } = 10485760;
        /// <summary>
        /// 用于写入锁
        /// </summary>
        private static object lockObj = new object();
        private static string f18 = "▏";
        private static string f14 = "▎";
        private static string f38 = "▍";
        private static string f12 = "▌";
        private static string f58 = "▋";
        private static string f34 = "▊";
        private static string f78 = "▉";
        private static string f11 = "█";
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public static string PathDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "\\Log\\");
        /// <summary>
        /// 保存日志文件的路径
        /// </summary>
        protected string CurrentPath
        {
            get
            {
                return Path.Combine(PathDirectory, FileName + DateTime.Now.ToShortDateString().Replace('/', '-') + ".log");  ///获取日志时间作为文件名
            }
        }
        /// <summary>
        /// 错误信息委托
        /// </summary>
        public event Action<string> ErroStringEvent = null;
        /// <summary>
        /// 输出日志类型
        /// </summary>
        public LogErroType LogForntType = LogErroType.Normal | LogErroType.Waring | LogErroType.Erro;
        /// <summary>
        /// 日志输出详细程度
        /// </summary>
        public LogErroType LogForntDetail = LogErroType.Detail | LogErroType.NoDetail;
        /// <summary>
        /// 日志文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        /// <returns>是否写入成功</returns>
        public bool Write(LogMessage logMsg)
        {
            if ((logMsg.Type & LogForntType) != 0)
            {
                bool isTrueWrite = false;
                StringBuilder msg = new StringBuilder();  ///存放打印日志信息
                //string fileName = PathDirectory + logMsg.LogTime.ToShortDateString().Replace('/', '-') + ".log";  ///获取日志时间作为文件名
                msg.Append("[" + logMsg.LogTime.ToString() + "][" + logMsg.Type + "] -> ");
                if ((LogForntDetail & LogErroType.NoDetail) != 0)
                {
                    msg.Append("[" + LogErroType.NoDetail + "]:" + logMsg.NormalMessage + "\r\n");
                }
                if ((LogForntDetail & LogErroType.Detail) != 0)
                {
                    msg.Append("[" + LogErroType.Detail + "]:" + logMsg.DetailMessage + "]" + "\r\n");
                }
                try
                {
                    LogInfo.Write(this, msg.ToString());
                }
                catch (Exception)
                {
                    isTrueWrite = false;
                }
                finally
                {
                    logMsg.Dispose();
                    msg.Clear();
                    msg = null;
                }
                return isTrueWrite;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="ags"></param>
        /// <returns></returns>
        public bool Write(params object [] ags)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in ags)
            {
                if (item != null)
                    sb.Append(item.ToString());
                else
                    sb.Append("null");
            }
            this.Write(sb.ToString());
            sb.Clear();
            sb = null;
            return true;
        }
        /// <summary>
        /// 统一写入接口
        /// </summary>
        /// <param name="log"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private static void Write(LogInfo log, string message)
        {
            //lock (lockObj)
            //{
            if (!Directory.Exists(PathDirectory))
            {
                Directory.CreateDirectory(PathDirectory);
            }
            FileInfo fileInfo = new FileInfo(log.CurrentPath);
            log.ErroStringEvent?.Invoke(message);
            if(fileInfo.Exists)
            {
                //if(fileInfo.Length > MaxFileSize)
                //{

                //}
                using (FileStream stream = File.Open(log.CurrentPath, FileMode.Append))
                {
                    byte[] datas = System.Text.Encoding.UTF8.GetBytes(message.ToString());
                    stream.Write(datas, 0, datas.Length);
                    datas = null;
                }

            }
            else
            {
                using (FileStream stream = File.Create(log.CurrentPath))
                {
                    byte[] datas = System.Text.Encoding.UTF8.GetBytes(message.ToString());
                    stream.Write(datas, 0, datas.Length);
                    datas = null;
                }
            }
            //}
        }
        /// <summary>
        /// 打印进度
        /// </summary>
        /// <param name="current">进度</param>
        /// <param name="max">最多的格子</param>
        public void Progress(float current, string taskname = "进度", int max = 20)
        {
            

            if (current > 1)
                return;
            float value = ((float)max * current);
            int pboxs = (int)value;
            float point = value - pboxs;

            string processstring = "";

            for (int i = 0; i < max; i++)
            {
                if (i < pboxs)
                {
                    processstring += f11;
                }
                else if (i > pboxs)
                {
                    processstring += " ";
                }
                else if (i == pboxs && pboxs != 0 && point != 0f)
                {
                    if (point > 0 && point <= 1.0f / 8.0f)
                    {
                        processstring += f18;
                    }
                    else if (point > 1.0f / 8.0f && point <= 1.0f / 4.0f)
                    {
                        processstring += f14;
                    }
                    else if (point > 1.0f / 4.0f && point <= 3.0f / 8.0f)
                    {
                        processstring += f38;
                    }
                    else if (point > 3.0f / 8.0f && point <= 1.0f / 2.0f)
                    {
                        processstring += f12;
                    }
                    else if (point > 1.0f / 2.0f && point <= 5.0f / 8.0f)
                    {
                        processstring += f58;
                    }
                    else if (point > 5.0f / 8.0f && point <= 3.0f / 4.0f)
                    {
                        processstring += f34;
                    }
                    else if (point > 3.0f / 4.0f && point <= 7.0f / 8.0f)
                    {
                        processstring += f78;
                    }
                    else
                    {
                        processstring += f11;
                    }
                }
            }
            Write(taskname, " : [", processstring, "]", current * 100, "%");
        }
    }

    /// <summary>
    /// 日志信息
    /// </summary>
    public struct LogMessage : IDisposable
    {
        /// <summary>
        /// 正常日志消息
        /// </summary>
        public string NormalMessage;
        /// <summary>
        /// 详细日志消息
        /// </summary>
        public string DetailMessage;
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogErroType Type;
        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime LogTime;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="norM">一般日志信息</param>
        /// <param name="detM">详细日志信息</param>
        /// <param name="type">日志类型</param>
        public LogMessage(string norM, string detM, LogErroType type)
        {
            this.NormalMessage = norM;
            this.DetailMessage = detM;
            this.Type = type;
            LogTime = DateTime.Now;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ex">异常信息</param>
        /// <param name="type">日志类型</param>
        public LogMessage(Exception ex, LogErroType type)
        {
            this.NormalMessage = ex.Message;
            this.DetailMessage = ex.ToString();
            this.Type = type;
            LogTime = DateTime.Now;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ex">异常信息(默认为一般日志)</param>
        public LogMessage(Exception ex)
        {
            this.NormalMessage = ex.Message;
            this.DetailMessage = ex.ToString();
            this.Type = LogErroType.Waring;
            LogTime = DateTime.Now;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="msg">一般信息</param>
        /// <param name="ex">详细异常</param>
        /// <param name="type">日志类型</param>
        public LogMessage(string msg, Exception ex, LogErroType type = LogErroType.Erro)
        {
            this.NormalMessage = msg + "  " + ex.Message;
            this.DetailMessage = ex.ToString();
            this.Type = type;
            this.LogTime = DateTime.Now;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="norM">一般日志信息</param>
        public LogMessage(string norM)
        {
            this.NormalMessage = norM;
            this.DetailMessage = "";
            this.Type = LogErroType.Normal;
            LogTime = DateTime.Now;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="norM">一般日志信息</param>
        /// <param name="type">日志类型</param>
        public LogMessage(string norM, LogErroType type)
        {
            this.NormalMessage = norM;
            this.DetailMessage = "";
            this.Type = type;
            LogTime = DateTime.Now;
        }
        /// <summary>
        /// 所有日志消息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + this.LogTime.ToString() + "][" + this.Type + "] -> " + "[" + LogErroType.NoDetail + "] : " + this.NormalMessage + "\r\n[" + LogErroType.Detail + "]:" + this.DetailMessage + "\r\n";
        }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="msg"></param>
        public static implicit operator LogMessage(string msg)
        {
            return new LogMessage(msg);
        }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="ex"></param>
        public static implicit operator LogMessage(Exception ex)
        {
            return new LogMessage(ex);
        }
        /// <summary>
        /// 释放内容
        /// </summary>
        public void Dispose()
        {
            NormalMessage = null;
            DetailMessage = null;
        }
    }
    [Flags]
    public enum LogErroType : byte
    {
        Normal = 1,
        Waring = 2,
        Erro = 4,
        Detail = 8,
        NoDetail = 16,
        None = 32,
    }
}
