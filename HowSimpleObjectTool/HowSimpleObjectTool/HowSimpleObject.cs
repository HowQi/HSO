using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HowSimpleObjectTool
{
    /// <summary>
    /// How简单对象。
    /// </summary>
    public class HowSimpleObject
    {
        /// <summary>
        /// 真实数据存放位置
        /// </summary>
        public readonly Dictionary<string, HowSimpleItem> inner;

        public HowSimpleObject()
        {
            inner = new Dictionary<string, HowSimpleItem>();
        }
        public HowSimpleObject(int capacity)
        {
            inner = new Dictionary<string, HowSimpleItem>(capacity);
        }
        public HowSimpleObject(Dictionary<string, HowSimpleItem> dict)
        {
            inner = dict;
        }

        /// <summary>
        /// 确实是否存在成员变量
        /// </summary>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public bool ContainsAttr(string attrName)
        {
            return inner.ContainsKey(attrName);
        }

        /// <summary>
        /// 移除某成员变量
        /// </summary>
        public bool RemoveAttr(string attrName)
        {
            return inner.Remove(attrName);
        }

        /// <summary>
        /// 访问成员变量,若变量不存在,返回空
        /// </summary>
        /// <param name="attrName">变量名</param>
        /// <returns>请求的HowSimpleItem实例</returns>
        public HowSimpleItem this[string attrName]
        {
            get
            {
                if (inner.ContainsKey(attrName))
                    return inner[attrName];
                else
                    return null;
            }
            set
            {
                inner[attrName] = value;
            }
        }

    }

    /// <summary>
    /// HowSimpleObject的数据项。HowSimpleObject的属性即是HowSimpleItem。
    /// </summary>
    public abstract class HowSimpleItem
    {
        /// <summary>
        /// 获取本HowSimpleItem的类型
        /// </summary>
        public abstract HowSimpleItemType ItemType { get; }

        /// <summary>
        /// 作为简单数据访问
        /// </summary>
        public virtual string Value
        {
            get { throw new HowItemTypeAndOperateNotMatchException(ItemType.ToString(), "Value"); }
            set { throw new HowItemTypeAndOperateNotMatchException(ItemType.ToString(), "Value"); }
        }
        /// <summary>
        /// 作为字典访问
        /// </summary>
        public virtual string this[string attrname]
        {
            get { throw new HowItemTypeAndOperateNotMatchException(ItemType.ToString(), "Dict"); }
            set { throw new HowItemTypeAndOperateNotMatchException(ItemType.ToString(), "Dict"); }
        }
        /// <summary>
        /// 作为数组访问
        /// </summary>
        public virtual string this[int idx]
        {
            get { throw new HowItemTypeAndOperateNotMatchException(ItemType.ToString(), "Array"); }
            set { throw new HowItemTypeAndOperateNotMatchException(ItemType.ToString(), "Array"); }
        }

        /// <summary>
        /// 将Item转换为文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }

        public static implicit operator HowSimpleItem(string o)
        {
            return o;
        }

        public static implicit operator HowSimpleItem(Dictionary<string,string> o)
        {
            return o;
        }

        public static implicit operator HowSimpleItem(List<string> o)
        {
            return o;
        }

    }

    /// <summary>
    /// HowSimpleItem自身的类型枚举。需要时根据枚举的指示将HowSimpleItem进行转型。
    /// </summary>
    public enum HowSimpleItemType
    {
        /// <summary>
        /// 纯粹数据,可直接访问内容,可转化为HowSimpleValue
        /// </summary>
        Value,
        /// <summary>
        /// 字典,可通过键访问内容,可转化为HowSimpleDictionary
        /// </summary>
        Dict,
        /// <summary>
        /// 数组,可通过索引访问内容,可转化为HowSimpleArray
        /// </summary>
        Ary
    }

    public class TypeNotMatchException : Exception
    {
        public TypeNotMatchException(string msg) : base(msg)
        { }
    }

    /// <summary>
    /// 类型匹配异常。类型不匹配时抛出该异常
    /// </summary>
    public class HowItemTypeAndOperateNotMatchException : TypeNotMatchException
    {
        public HowItemTypeAndOperateNotMatchException(string curType, string tarType)
            : base("类型错误,您正在试图获取的数据不支持这种操作。您的类型:[" + curType + "],期望的类型[" + tarType + "]")
        { }
    }

    /// <summary>
    /// 简单数据项目
    /// </summary>
    public class HowSimpleValueItem : HowSimpleItem
    {
        public override HowSimpleItemType ItemType { get { return HowSimpleItemType.Value; } }
        /// <summary>
        /// 数据真实存储位置。改变它也会改变整个对象的数据。
        /// </summary>
        public string inner;

        public HowSimpleValueItem(string value)
        {
            inner = value;
        }
        public HowSimpleValueItem()
            : this("")
        { }

        /// <summary>
        /// 访问该变量表示的数据
        /// </summary>
        public override string Value
        {
            get { return inner; }
            set { inner = value; }
        }

        public override string ToString()
        {
            return inner;
        }

        public static implicit operator HowSimpleValueItem(string o)
        {
            return new HowSimpleValueItem(o);
        }

    }

    /// <summary>
    /// 带有键值对的项目
    /// </summary>
    public class HowSimpleDictionaryItem : HowSimpleItem
    {
        public override HowSimpleItemType ItemType { get { return HowSimpleItemType.Dict; } }
        /// <summary>
        /// Item的数据真实存放位置。用户可直接操作之以改变整个Item的数据,也可以间接通过Item提供的函数接口访问。
        /// </summary>
        public readonly Dictionary<string, string> inner;

        public HowSimpleDictionaryItem()
        {
            inner = new Dictionary<string, string>();
        }
        public HowSimpleDictionaryItem(int dictCapacity)
        {
            inner = new Dictionary<string, string>(dictCapacity);
        }
        public HowSimpleDictionaryItem(Dictionary<string, string> dict)
        {
            inner = dict;
        }

        /// <summary>
        /// 测试是否包含键。简单地返回了内部字典的相同方法。
        /// </summary>
        public bool ContainsKey(string key)
        {
            return inner.ContainsKey(key);
        }

        /// <summary>
        /// 移除键值对(用键来标志希望移除的键值对)。简单调用了内部字典的移除方法。需要更多样的移除操作可直接访问inner变量。
        /// </summary>
        public bool RemoveKVPair(string key)
        {
            return inner.Remove(key);
        }

        /// <summary>
        /// 返回字典项目数
        /// </summary>
        public int Count { get { return inner.Count; } }

        /// <summary>
        /// 访问字典内的数据。数据不存在时返回空。
        /// </summary>
        /// <param name="attrName">索引名</param>
        /// <returns></returns>
        public override string this[string attrName]
        {
            get
            {
                if (inner.ContainsKey(attrName))
                    return inner[attrName];
                else
                    return null;
            }
            set
            {
                inner[attrName] = value;
            }
        }

        /// <summary>
        /// 以字符串形式返回字典内容,形式类似python风格。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append('{');
            foreach (var item in inner)
            {
                res.Append(item.Key);
                res.Append(':');
                res.Append(item.Value);
                res.Append(',');
            }
            res.Append('}');
            return res.ToString();
        }

        public static implicit operator HowSimpleDictionaryItem(Dictionary<string,string> o)
        {
            return new HowSimpleDictionaryItem(o);
        }

    }

    /// <summary>
    /// 数组项目
    /// </summary>
    public class HowSimpleArrayItem : HowSimpleItem
    {
        public override HowSimpleItemType ItemType { get { return HowSimpleItemType.Ary; } }
        /// <summary>
        /// 数据真实存放位置。可通过操作之修改本对象的数据。
        /// </summary>
        public readonly List<string> inner;

        public HowSimpleArrayItem()
        {
            inner = new List<string>();
        }
        public HowSimpleArrayItem(int aryCapacity)
        {
            inner = new List<string>(aryCapacity);
        }
        public HowSimpleArrayItem(List<string> list)
        {
            inner = list;
        }

        /// <summary>
        /// 添加数据。
        /// </summary>
        public void Add(string val)
        {
            inner.Add(val);
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveAt(int idx)
        {
            inner.RemoveAt(idx);
        }

        /// <summary>
        /// 数组长度
        /// </summary>
        public int Count { get { return inner.Count; } }

        /// <summary>
        /// 访问数组内的数据
        /// </summary>
        /// <param name="idx">索引值</param>
        /// <returns></returns>
        public override string this[int idx]
        {
            get
            {
                return inner[idx];
            }
            set
            {
                inner[idx] = value;
            }
        }
        /// <summary>
        /// 将对象转换为文本形式,风格类似于python。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append('[');
            for (int i = 0; i < inner.Count; i++)
            {
                if (i != 0)
                    res.Append(',');
                res.Append(inner[i]);
            }
            res.Append(']');
            return res.ToString();
        }

        public static implicit operator HowSimpleArrayItem( List<string> o)
        {
            return new HowSimpleArrayItem(o);
        }

    }

}
