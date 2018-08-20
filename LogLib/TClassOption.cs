using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LogLib
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class TClassOption
    {

        /// <summary>
        /// 根据字符串匹配对应的枚举类型
        /// </summary>
        /// <param name="type">枚举类型的字符串</param>
        /// <returns>对应字符串的枚举类型，如果字符串错误则抛出错误</returns>
        public static T GetEnumTypeByString<T>(string type)
        {
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (type == item.ToString())
                {
                    return item;
                }
            }
            return default(T);
        }
        /// <summary>
        /// 根据字符串匹配对应的枚举类型
        /// </summary>
        /// <param name="type">枚举类型的字符串</param>
        /// <returns>对应字符串的枚举类型，如果字符串错误则抛出错误</returns>
        public static object GetEnumTypeByString(string type, Type types)
        {
            foreach (var item in Enum.GetValues(types))
            {
                if (type == item.ToString())
                {
                    return item;
                }
            }
            return default(object);
        }
        /// <summary>
        /// 线性插值
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="end">终止值</param>
        /// <param name="lerp">插值</param>
        /// <returns></returns>
        public static double Lerp(double start, double end, double lerp)
        {
            lerp = lerp < 0 ? 0 : lerp;
            lerp = lerp > 1 ? 1 : lerp;
            return (end + start) * lerp;
        }
        /// <summary>
        /// 根据T类型的枚举的具体V类型的值获取对应的枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <typeparam name="V">枚举类型继承的类型</typeparam>
        /// <param name="value">对应的值</param>
        /// <returns>对应的枚举值</returns>
        public static T GetEnumTypeByType<T, V>(V value)
        {
            try
            {
                TypeCode code = GetEnumTypeByString<TypeCode>(value.GetType().Name);
                foreach (T item in Enum.GetValues(typeof(T)))
                {
                    if (Convert.ChangeType(value, code).Equals(value))
                    {
                        return item;
                    }
                }
                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }

        }
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj">要序列号的对象</param>
        /// <returns></returns>
        public static byte[] SeriableObj(object obj)
        {
            BinaryFormatter binary = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            binary.Serialize(stream, obj);
            byte[] data = stream.ToArray();
            stream.Close();
            return data;
        }
        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T">转换结果类型</typeparam>
        /// <param name="datas">数据</param>
        /// <returns>返回转换的结果对象</returns>
        public static T DesSeriableObj<T>(byte[] datas)
        {
            BinaryFormatter binary = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(datas);
            T obj = (T)binary.Deserialize(stream);
            stream.Close();
            return obj;
        }
    }
}
