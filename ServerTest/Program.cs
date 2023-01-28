using Server;
using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;

namespace ServerTest
{
    class Program
    {

        public static SocketClient SocketClient { get; set; } = new SocketClient();

        public static MiniHeader header = new MiniHeader();

        public static ImageData ImageData { get; set; }

        static bool issave = false;
        /// <summary>
        /// 是否初始化
        /// </summary>
        public static bool Isinit = false;
        static void Main(string[] args)
        {
            SocketClient.LinkEvent += LinkEvent;
            SocketClient.RecDataEvent += RecDataEvent;
            SocketClient.InitSocket("127.0.0.1", 8100);
            SocketClient.Start();
            Console.WriteLine(SocketClient.DataPageLength);

            Console.ReadKey();
        }
        /// <summary>
        /// 接受数据事件
        /// </summary>
        /// <param name="obj"></param>
        private static void RecDataEvent(RecDataObject obj)
        {
            if (!Isinit)
            {
                header.Version = obj.Datas[0];
                header.Size = obj.Datas[1];
                header.ProcessId = GetDatas(obj.Datas, 2, 5);
                header.Width = GetDatas(obj.Datas, 6, 9);
                header.Height = GetDatas(obj.Datas, 10, 13);
                header.VirtualWidth = GetDatas(obj.Datas, 14, 17);
                header.VirtualHeight = GetDatas(obj.Datas, 18, 21);
                header.Orentation = obj.Datas[22];
                header.Quirk = obj.Datas[23];
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(header));
                Isinit = true;
            }
            else if (!issave)
            {
                if(ImageData.Datas.Count == 0)
                {
                    ImageData.Size = GetDatas(obj.Datas, 0, 3);
                    for (int i = 4; i < obj.Datas.Length; i++)
                    {
                        ImageData.Datas.Add(obj.Datas[i]);
                        if (ImageData.Datas.Count == ImageData.Size)
                        {
                            issave = true;
                            SaveImage();
                            break;
                        }
                    }
                    
                }
                else
                {
                    for (int i = 0; i < obj.Datas.Length; i++)
                    {
                        ImageData.Datas.Add(obj.Datas[i]);
                        if (ImageData.Datas.Count == ImageData.Size)
                        {
                            issave = true;
                            SaveImage();
                            break;
                        }
                    }
                }
            }
            //Console.WriteLine(obj.Datas.ToString());
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="obj"></param>
        private static void LinkEvent(Socket obj)
        {
            Console.WriteLine("--------");
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static int GetDatas(byte [] datas, int from, int to)
        {
            int value = 0;
            int len = 0;
            for (int i = from; i <= to; i++)
            {
                value += datas[i] << (len++ * 8);
            }
            return value;
        }
        /// <summary>
        /// 保存图像
        /// </summary>
        public static void SaveImage()
        {
            MemoryStream stream = new MemoryStream(ImageData.Datas.ToArray());
            stream.Position = 0;
            Bitmap bitmap = new Bitmap(stream);
            bitmap.Save("111.jpg");
        }
    }
}
