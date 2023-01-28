using System;
using System.Collections.Generic;
using System.Text;

namespace ServerTest
{
    /// <summary>
    /// MiniHeader 头信息
    /// </summary>
    public class MiniHeader
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 进程id
        /// </summary>
        public int ProcessId { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 虚拟宽度
        /// </summary>
        public int VirtualWidth { get; set; }
        /// <summary>
        /// 虚拟高度
        /// </summary>
        public int VirtualHeight { get; set; }
        /// <summary>
        /// 旋转方向
        /// </summary>
        public int Orentation { get; set; }
        /// <summary>
        /// 图像质量
        /// </summary>
        public int Quirk { get; set; }
    }
}
