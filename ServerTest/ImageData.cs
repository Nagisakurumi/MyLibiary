using System;
using System.Collections.Generic;
using System.Text;

namespace ServerTest
{
    /// <summary>
    /// 图像数据
    /// </summary>
    public class ImageData
    {
        /// <summary>
        /// 长度
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public List<byte> Datas
        {
            get; set;
        } = new List<byte>();
    }
}
