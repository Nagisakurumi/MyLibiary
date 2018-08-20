using System;
using System.IO;
using static LogLib.LogInfo;


namespace LogLib
{
    /// <summary>
    /// 存储文件
    /// </summary>
    public static class IniFile
    {
        /// <summary>
        /// 保存对象到文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="obj">对象</param>
        /// <returns>是否操作成功</returns>
        public static bool SaveObjToFile(string path, object obj)
        {
            try
            {
                using (Stream file = File.Open(path, FileMode.Create))
                {
                    byte[] datas = TClassOption.SeriableObj(obj);
                    file.Write(datas, 0, datas.Length);
                    datas = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        /// 从文件中获取对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        /// <returns>返回该对象</returns>
        public static T GetTypeObjFromFile<T>(string path)
        {
            try
            {
                T t = default(T);
                using (FileStream file = File.Open(path, FileMode.Open))
                {
                    byte[] datas = new byte[file.Length];
                    file.Read(datas, 0, datas.Length);
                    t = TClassOption.DesSeriableObj<T>(datas);
                    datas = null;
                }
                return t;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static bool SaveStringToFile(string path, string obj)
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Create))
                {
                    byte[] datas = System.Text.Encoding.UTF8.GetBytes(obj);
                    stream.Write(datas, 0, datas.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetStringFromFile(string path)
        {
            try
            {
                string data = "";
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    byte[] datas = new byte[stream.Length];
                    stream.Read(datas, 0, datas.Length);
                    data = System.Text.Encoding.UTF8.GetString(datas);
                }
                return data;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
