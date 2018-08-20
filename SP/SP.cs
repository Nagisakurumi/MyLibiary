using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP
{
    /// <summary>
    /// 数据接收回调
    /// </summary>
    /// <param name="data"></param>
    public delegate void RecivedDataCallBackHandler(object data);

    public abstract class SP
    {
        #region 字段
        /// <summary>
        /// 串口
        /// </summary>
        private SerialPort port = new SerialPort();
        /// <summary>
        /// 包头
        /// </summary>
        public byte HAND = 0xff;
        /// <summary>
        /// 包尾
        /// </summary>
        public byte END = 0xff;
        /// <summary>
        /// 接受返回数据
        /// </summary>
        protected List<byte> recData = new List<byte>();
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public RecivedDataCallBackHandler RecivedDataCallBack = null;
        /// <summary>
        /// 数据接收异常回调
        /// </summary>
        public RecivedDataCallBackHandler ErrorDataCallBack = null;
        /// <summary>
        /// 串口连接名称
        /// </summary>
        private string portName = "";
        /// <summary>
        /// 接受长度
        /// </summary>
        private int recLegth = 0;
        /// <summary>
        /// 编号
        /// </summary>
        private string portCode = Guid.NewGuid().ToString();
        /// <summary>
        /// 发送数据锁
        /// </summary>
        private object writeLock = new object();
        #endregion
        #region 访问器
        /// <summary>
        /// 串口连接名称
        /// </summary>
        public string PortName
        {
            get
            {
                return portName;
            }

            set
            {
                portName = value;
            }
        }
        /// <summary>
        /// 是否已经开启
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return port.IsOpen;
            }
        }
        /// <summary>
        /// 编号
        /// </summary>
        public string PortCode
        {
            get
            {
                return portCode;
            }

            set
            {
                portCode = value;
            }
        }
        /// <summary>
        /// 接受长度
        /// </summary>
        public int RecLegth
        {
            get
            {
                return recLegth;
            }
            set
            {
                recLegth = value;
            }       
        }
        #endregion
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bate">波特率</param>
        /// <param name="datap">数据位</param>
        /// <param name="stopbit">停止位</param>
        /// <param name="parity">校验</param>
        public SP(int bate, int datap, StopBits stopbit, Parity parity)
        {
            port.BaudRate = bate;
            port.DataBits = datap;
            port.StopBits = stopbit;
            port.Parity = parity;

            port.DataReceived += Port_DataReceived;
        }
        /// <summary>
        /// 数据接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] rd = new byte[port.BytesToRead];
            port.Read(rd, 0, rd.Length);
            string sb = "";

            foreach (var item in rd)
            {
                recData.Add(item);
                if (recData[0] == HAND && item == END && recData.Count == RecLegth)
                {
                    if (!CheckParity())
                    {
                        sb = "";
                        foreach (var d in recData)
                        {
                            sb += d.ToString("x2") + " ";
                        }
                        //Log.Write("返回的数据校验错误!, 收到的包 : " + sb);
                        ErrorDataCallBack?.Invoke(sb);
                    }
                    else
                    {
                        DataDeal(recData);
                    }
                    recData.Clear();
                }
            }
        }
        #endregion
        #region 方法
        /// <summary>
        /// 校验返回的数据是否正确
        /// </summary>
        /// <returns>返回的数据是否正确</returns>
        protected virtual bool CheckParity()
        {
            if (recData.Count < 3)
                return false;
            byte all = recData[recData.Count - 2];
            byte curall = 0;
            for (int i = 0; i < recData.Count - 2; i++)
            {
                curall += recData[i];
            }
            return all == curall;
        }
        /// <summary>
        /// 分发数据
        /// </summary>
        /// <param name="data"></param>
        protected virtual void DataDeal(List<byte> data)
        {
            object recdata = Deal(data);
            new Thread(new ParameterizedThreadStart((d) => {
                lock (this)
                {
                    RecivedDataCallBack?.Invoke(d);
                }
            })) { IsBackground = true }.Start(recdata);
        }
        /// <summary>
        /// 对数据进行二次处理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract object Deal(List<byte> data);
        /// <summary>
        /// 开启
        /// </summary>
        public virtual bool Open()
        {
            try
            {
                if (!IsOpen && PortName != "")
                {
                    port.PortName = PortName;
                    port.Open();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                //Log.Write(new LogMessage("连接串口失败", ex));
                port.Close();
                return false;
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public virtual bool Close()
        {
            try
            {
                if (IsOpen)
                {
                    port.Close();
                    //Log.Write("编号 : " + this.PortCode + " 被关闭!");
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                //Log.Write(new LogMessage("异常", ex));
                return false;
            }
        }

        /// <summary>
        /// 和校验
        /// </summary>
        /// <param name="data">要校验的数据</param>
        /// <returns>返回整合后的可以发送的数据</returns>
        protected virtual byte[] Parity(byte[] data)
        {
            byte[] sd = new byte[data.Length + 2];
            Array.Copy(data, sd, data.Length);
            byte all = 0;
            for (int i = 0; i < data.Length; i++)
            {
                all += data[i];
            }
            sd[sd.Length - 2] = all;
            sd[sd.Length - 1] = END;
            data = null;
            return sd;
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public virtual bool Send(byte[] data)
        {
            if (!this.IsOpen)
            {
                //Log.Write("未连接!编号:" + this.portCode);
                return false;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("发送的包 ：");
            foreach (var item in data)
            {
                builder.Append(item.ToString("x2"));
                builder.Append(" ");
            }
            //Log.Write(builder.ToString());
            builder.Clear();
            builder = null;

            try
            {
                //byte[] sendata = Parity(data.ToArray());
                lock (writeLock)
                {
                    port.Write(data, 0, data.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Log.Write(new LogMessage("发送数据异常!", ex));
                return false;
            }
        }
        #endregion
    }
}
