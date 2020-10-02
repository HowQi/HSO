using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace HowSimpleObjectTool
{
    public static class HowSimpleObjectBuilder
    {

        public static HowSimpleObject ReadObj(IArticle article)
        {
            var parser = new Parser(article, false);
            var res = parser.StartRead();
            if (!res) throw new HowSimpleObjectParsingException("解析完成,语法不符");
            return parser.ResultHSO;
        }

        public static HowSimpleObject ReadObj(string str)
        {
            return ReadObj(new StringArticle(str));
        }
        public static HowSimpleObject ReadObj(StreamReader sr)
        {
            return ReadObj(new StreamArticle(sr));
        }

        public static HowSimpleObject ReadObj(IArticle article, out string[] comments)
        {
            var parser = new Parser(article, false);
            var res = parser.StartRead(out comments);
            if (!res) throw new HowSimpleObjectParsingException("解析完成,语法不符");
            return parser.ResultHSO;
        }

        public static HowSimpleObject ReadObj(string str, out string[] comments)
        {
            return ReadObj(new StringArticle(str), out comments);
        }

        public static HowSimpleObject ReadObj(StreamReader sr, out string[] comments)
        {
            return ReadObj(new StreamArticle(sr), out comments);
        }

        public static List<HowSimpleObject> ReadObjList(IArticle article)
        {
            var paser = new Parser(article, true);
            var res = paser.StartRead();
            if (!res)
                throw new HowSimpleObjectParsingException("解析完成,语法不符");
            else
                return paser.ResultHSOList;
        }

        public static List<HowSimpleObject> ReadObjList(string str)
        {
            return ReadObjList(new StringArticle(str));
        }
        public static List<HowSimpleObject> ReadObjList(StreamReader sr)
        {
            return ReadObjList(new StreamArticle(sr));
        }

        public static List<HowSimpleObject> ReadObjList(IArticle article, out string[] comments)
        {
            var paser = new Parser(article, true);
            var res = paser.StartRead(out comments);
            if (!res)
                throw new HowSimpleObjectParsingException("解析完成,语法不符");
            else
                return paser.ResultHSOList;
        }

        public static List<HowSimpleObject> ReadObjList(string str, out string[] comments)
        {
            return ReadObjList(new StringArticle(str), out comments);
        }

        public static List<HowSimpleObject> ReadObjList(StreamReader sr, out string[] comments)
        {
            return ReadObjList(new StreamArticle(sr), out comments);
        }

        public static HowSimpleObject[] ReadObjAry(IArticle article)
        {
            var paser = new Parser(article, true);
            var res = paser.StartRead();
            if (!res)
                throw new HowSimpleObjectParsingException("解析完成,语法不符");
            else
                return paser.ResultHSOAry;
        }

        public static HowSimpleObject[] ReadObjAry(string str)
        {
            return ReadObjAry(new StringArticle(str));
        }

        public static HowSimpleObject[] ReadObjAry(StreamReader sr)
        {
            return ReadObjAry(new StreamArticle(sr));
        }

        public static HowSimpleObject[] ReadObjAry(IArticle article, out string[] comments)
        {
            var paser = new Parser(article, true);
            var res = paser.StartRead(out comments);
            if (!res)
                throw new HowSimpleObjectParsingException("解析完成,语法不符");
            else
                return paser.ResultHSOAry;
        }

        public static HowSimpleObject[] ReadObjAry(string str, out string[] comments)
        {
            return ReadObjAry(str, out comments);
        }

        public static HowSimpleObject[] ReadObjAry(StreamReader sr, out string[] comments)
        {
            return ReadObjAry(sr, out comments);
        }

        public interface IArticle
        {
            char? Cur();
            string DetectNextN(int count);
            void Next();
        }

        public class StringArticle : IArticle
        {
            string inner;
            int idx = 0;
            public StringArticle(string article)
            {
                inner = article;
            }
            public char? Cur()
            {
                if (idx < inner.Length)
                    return inner[idx];
                else
                    return null;
            }
            public string DetectNextN(int count)
            {
                if (idx + count < inner.Length)
                {
                    return inner.Substring(idx, count);
                }
                else
                {
                    return null;
                }
            }
            public void Next()
            {
                ++idx;
            }
        }

        public class StreamArticle : IArticle
        {
            StreamReader inner;
            Queue<char> articleBuf = new Queue<char>();
            public StreamArticle(StreamReader stm)
            {
                inner = stm;
            }

            bool Gen()
            {
                if (inner.EndOfStream)
                {
                    return false;
                }
                string res = inner.ReadLine();
                for (int i = 0; i < res.Length; i++)
                {
                    articleBuf.Enqueue(res[i]);
                }
                articleBuf.Enqueue('\r');
                articleBuf.Enqueue('\n');
                return true;
            }

            public char? Cur()
            {
                while (articleBuf.Count <= 0 && Gen()) ;
                if (articleBuf.Count == 0) return null;
                return articleBuf.Peek();
            }

            public string DetectNextN(int count)
            {
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (count == 0)
                {
                    return "";
                }
                while (articleBuf.Count < count && Gen()) ;
                if (articleBuf.Count < count) return null;
                StringBuilder head = new StringBuilder(count);
                int i = 0;
                foreach (var ch in articleBuf)
                {
                    head.Append(ch);
                    ++i;
                    if (i >= count) break;
                }
                return head.ToString();
            }
            public void Next()
            {
                if (articleBuf.Count > 0)
                {
                    char? tch;
                    tch = articleBuf.Dequeue();
                    Console.Write(Convert.ToString(tch));
                }
            }

        }

        /// <summary>
        /// 文档内容解析器,其一个实例仅可做一次解析,不可重复解析。
        /// </summary>
        private class Parser
        {
            /// <summary>
            /// 标记本次读取的数据是否是系列Obj
            /// </summary>
            bool isAry;
            /// <summary>
            /// 文档生成器
            /// </summary>
            IArticle article;
            /// <summary>
            /// 注释寄存器。存放有义注释用。
            /// </summary>
            List<string> commentBuf;
            /// <summary>
            /// 元素寄存器。储存满后用于构造完整的元素。
            /// </summary>
            StringBuilder eleBuf = new StringBuilder();
            /// <summary>
            /// HSI缓存,用于组成HowSimpleItem
            /// </summary>
            List<Element> hsiBuf = new List<Element>();
            /// <summary>
            /// HSO缓存,用于组成HowSimpleObject
            /// </summary>
            Dictionary<string, HowSimpleItem> hsoBuf = new Dictionary<string, HowSimpleItem>();
            /// <summary>
            /// HSOs缓存,用于组成HowSimpleObject数组
            /// </summary>
            List<HowSimpleObject> objs;

            HowSimpleObject resultHSO;
            /// <summary>
            /// 获取独立对象的解析结果
            /// </summary>
            public HowSimpleObject ResultHSO
            {
                get
                {
                    if (!isAry)
                        return resultHSO;
                    else
                        throw new TypeNotMatchException("类型不匹配,您希望获取独立对象,但该解析器只提供数组");
                }
            }
            List<HowSimpleObject> resultHSOList;
            /// <summary>
            /// 获取数组对象的解析结果(返回列表)
            /// </summary>
            public List<HowSimpleObject> ResultHSOList
            {
                get
                {
                    if (isAry)
                        return resultHSOList;
                    else
                        throw new TypeNotMatchException("类型不匹配,您希望获取数组,但该解析器只提供独立对象");
                }
            }
            /// <summary>
            /// 获取列表对象的解析结果(返回数组)
            /// </summary>
            public HowSimpleObject[] ResultHSOAry
            {
                get
                {
                    return ResultHSOList.ToArray();
                }
            }
            public Parser(IArticle article_, bool readAry)
            {
                article = article_;
                isAry = readAry;
                if (isAry)
                {
                    objs = new List<HowSimpleObject>();
                }
            }

            public bool StartRead()
            {
                commentBuf = null;
                bool res;
                if (isAry)
                {
                    res = S0();
                }
                else
                {
                    res = S1();
                }
                return res;
            }
            public bool StartRead(out string[] comments)
            {
                commentBuf = new List<string>();
                bool res;
                if (isAry)
                {
                    res = S0();
                }
                else
                {
                    res = S1();
                }
                comments = commentBuf.ToArray();
                return res;
            }

            #region ===========数据内容采集和处理用函数===========
            /// <summary>
            /// 清理
            /// </summary>
            private void ClearComment()
            {
                var resII = article.DetectNextN(2);
                var resI = article.DetectNextN(1);
                bool record = false;
                if (resII == "//")//1类注释 双斜杠
                {
                    article.Next();
                    article.Next();
                    record = false; ;
                }
                else if (resI == "#")//2类注释 井号
                {
                    article.Next();
                    record = true;
                }
                else//若不是注释,直接返回
                {
                    return;
                }

                if (commentBuf == null)//如果注释记录是关闭的,record也关闭
                {
                    record = false;
                }

                StringBuilder curCmtRec = null;
                if (record)
                {
                    curCmtRec = new StringBuilder();
                }
                char? curCh = article.Cur();
                while (curCh.HasValue && curCh.Value != '\n')
                {
                    if (record && curCh.HasValue && curCh.Value != '\n')
                    {
                        curCmtRec.Append(curCh.Value);
                    }
                    article.Next();
                    curCh = article.Cur();
                }

                if (record)
                {
                    commentBuf.Add(curCmtRec.ToString());
                }
            }

            private void OutHSI()
            {
                var tHsi = hsiBuf;
                if (tHsi.Count == 3)//经典搭配 name="value";
                {
                    var eName = tHsi[0];
                    var eOp = tHsi[1];
                    var eVal = tHsi[2];
                    if (eName.type == Element.TPContent
                        && !string.IsNullOrEmpty(eName.val.Trim())
                        && eOp.type == Element.TPOperator
                        && eOp.val == "="
                        && eVal.type == Element.TPQuotationContent)
                    {
                        var resHsi = new HowSimpleValueItem(eVal.val);
                        var key = eName.val.Trim();
                        if (hsoBuf.ContainsKey(key))
                        {
                            var curItem = hsoBuf[key];
                            throw new HowSimpleObjectParsingException(
                                   string.Format("类型冲突:已有类别为{0}的变量\"{1}\",但是文档试图向其中以普通数据形式覆写数据\"{2}\",请检查数据文档的内容",
                                   curItem.ItemType.ToString(), curItem.Value, eVal));
                        }
                        hsoBuf[key] = resHsi;
                        hsiBuf.Clear();
                        return;
                    }
                }
                if (tHsi.Count == 3)//数组 aryname+="value01";
                {
                    var eName = tHsi[0];
                    var eOp = tHsi[1];
                    var eVal = tHsi[2];
                    if (eName.type == Element.TPContent
                        && !string.IsNullOrEmpty(eName.val.Trim())
                        && eOp.type == Element.TPOperator
                        && eOp.val == "+="
                        && eVal.type == Element.TPQuotationContent)
                    {
                        var key = eName.val.Trim();
                        HowSimpleArrayItem ary;
                        if (!hsoBuf.ContainsKey(key))
                        {
                            hsoBuf[key] = new HowSimpleArrayItem();
                        }
                        if (hsoBuf[key].ItemType != HowSimpleItemType.Ary)
                        {
                            var curItem = hsoBuf[key];
                            throw new HowSimpleObjectParsingException(
                                string.Format("类型冲突:已有类别为{0}的变量\"{1}\",但是文档试图向其中以数组形式添加数据\"{2}\",请检查数据文档的内容",
                                curItem.ItemType.ToString(), curItem.Value, eVal));
                        }
                        ary = hsoBuf[key] as HowSimpleArrayItem;
                        ary.Add(eVal.val);
                        hsiBuf.Clear();
                        return;
                    }
                }
                if (tHsi.Count == 5)//键值对 dictname+="value01"
                {
                    var eName = tHsi[0];
                    var eOp1 = tHsi[1];
                    var eAttr = tHsi[2];
                    var eOp2 = tHsi[3];
                    var eVal = tHsi[4];
                    if (
                        eName.type == Element.TPContent
                        && !string.IsNullOrEmpty(eName.val.Trim())
                        && eOp1.type == Element.TPOperator
                        && eOp1.val == "."
                        && eAttr.type == Element.TPContent
                        && !string.IsNullOrEmpty(eAttr.val.Trim())
                        && eOp2.type == Element.TPOperator
                        && eOp2.val == "="
                        && eVal.type == Element.TPQuotationContent
                        )
                    {
                        var pKey = eName.val.Trim();
                        var cKey = eAttr.val.Trim();

                        HowSimpleDictionaryItem dict;
                        if (!hsoBuf.ContainsKey(pKey))
                        {
                            hsoBuf[pKey] = new HowSimpleDictionaryItem();
                        }
                        if (hsoBuf[pKey].ItemType != HowSimpleItemType.Dict)
                        {
                            var curItem = hsoBuf[pKey];
                            throw new HowSimpleObjectParsingException(
                                string.Format("类型冲突:已有类别为{0}的变量\"{1}\",但是文档试图向其中以字典形式添加数据\"{2}\",请检查数据文档的内容",
                                curItem.ItemType.ToString(), curItem.Value, eVal));
                        }
                        dict = hsoBuf[pKey] as HowSimpleDictionaryItem;
                        dict[cKey] = eVal.val;
                        hsiBuf.Clear();
                        return;
                    }
                }
                //非法组合
                string combineStr = "<<";
                for (int i = 0; i < tHsi.Count; i++)
                {
                    combineStr += tHsi[i].ToString();
                }
                combineStr += ">>";

                throw new HowSimpleObjectParsingException("无效的组合:" + combineStr);

            }

            /// <summary>
            /// 输出Obj,如果是数组,也会加入数组
            /// </summary>
            private HowSimpleObject OutObj()
            {
                var res = new HowSimpleObject(hsoBuf);
                if (isAry)
                {
                    objs.Add(res);
                }
                hsoBuf = new Dictionary<string, HowSimpleItem>();
                //Console.WriteLine("[[out obj]]" + hsoBuf.ToString());
                return res;
            }

            /// <summary>
            /// 输出数组
            /// </summary>
            /// <returns></returns>
            private List<HowSimpleObject> OutAry()
            {
                var res = objs;
                objs = new List<HowSimpleObject>();
                //Console.WriteLine("[[out ary]]" + objs.ToString());
                return res;
            }

            /// <summary>
            /// 完成"内容元素"
            /// </summary>
            private void FinishContent()
            {
                var res = new Element(Element.TPContent, eleBuf.ToString());
                hsiBuf.Add(res);
                //Console.WriteLine("[Fin Content]" + res.ToString());
                eleBuf.Clear();
            }

            /// <summary>
            /// 完成"引用内容"
            /// </summary>
            private void FinishQuotationContent()
            {
                var res = new Element(Element.TPQuotationContent, eleBuf.ToString());
                hsiBuf.Add(res);
                //Console.WriteLine("[Fin Quot Content]" + res.ToString());
                eleBuf.Clear();
            }

            /// <summary>
            /// 完成操作符
            /// </summary>
            private void FinishOperater()
            {
                var res = new Element(Element.TPOperator, eleBuf.ToString());
                hsiBuf.Add(res);
                // Console.WriteLine("[Fin Operator]" + res.ToString());
                eleBuf.Clear();
            }

            /// <summary>
            /// 内容元素。即操作符 or 变量名 or 变量内容
            /// </summary>
            private struct Element
            {
                public const int TPOperator = 0;
                public const int TPContent = 1;
                public const int TPQuotationContent = 2;
                public int type;
                public string val;
                public Element(int type_, string val_)
                {
                    type = type_;
                    val = val_;
                }
                public override string ToString()
                {
                    string typeDesc = "";
                    switch (type)
                    {
                        case TPOperator: typeDesc = "操作符"; break;
                        case TPContent: typeDesc = "直接内容"; break;
                        case TPQuotationContent: typeDesc = "引用内容"; break;
                        default: typeDesc = "未知内容"; break;
                    }
                    return string.Format("({0}:{1})", typeDesc, val);
                }
            }

            #endregion


            #region ===========字符类型归类用函数===========

            static ChType CharClassification(char ch)
            {
                if (ch == '\n') return ChType.NL;
                if (Char.IsWhiteSpace(ch)) return ChType.ISP;
                if (ch == '.' || ch == '\\' || ch == '/' || ch == '=' || ch == '+'
                    )
                {
                    return ChType.OP;
                }
                if (ch == '"' || ch == '{' || ch == '}' || ch == ';')
                {
                    return ChType.SPARA;
                }
                if (ch > 0x0000 && ch <= '/'
                    || ch >= ':' && ch <= '@'
                    || ch >= '[' && ch <= '^'
                    || ch == '`'
                    || ch >= '{' && ch <= 0x007F
                    )//为了不让数据格式带有充满歧义的字符,特独立划分出常用符号以排除
                {
                    return ChType.OTHERSYMBOL;
                }
                //其余所有符号皆视作有意义字符。
                return ChType.CH;
                //注:上述不同判定涉及的集合间有重叠,但最终分类后的结果不带有重叠。
            }

            /// <summary>
            /// ASP = All Space 指包含换行在内的空白字符
            /// </summary>
            /// <param name="cht"></param>
            /// <returns></returns>
            static bool IsASP(ChType cht)
            {
                return cht == ChType.ISP || cht == ChType.NL;
            }

            /// <summary>
            /// 字符类型枚举
            /// </summary>
            private enum ChType
            {
                /// <summary>
                /// 换行符
                /// </summary>
                NL,
                /// <summary>
                /// 空白字符(不包括换行的)
                /// </summary>
                ISP,
                /// <summary>
                /// 有意义的字符
                /// </summary>
                CH,
                /// <summary>
                /// 操作符
                /// </summary>
                OP,
                /// <summary>
                /// 分隔符(引号、花括号、分号)
                /// </summary>
                SPARA,
                /// <summary>
                /// 其他符号
                /// </summary>
                OTHERSYMBOL
            }

            #endregion


            #region ===========状态机===========

            /*
            
            状态机备注:
            
            每个状态机对字符的处理行为是

            取得字符,立即下移一个字符
            根据取得的字符判断应该进入的下一个状态

             */

            bool S0()
            {
                ClearComment();
                var cch = article.Cur();
                article.Next();

                if (cch.HasValue)
                {
                    var cht = CharClassification(cch.Value);
                    if (IsASP(cht))
                    {
                        return S0();
                    }
                    else if (cch.Value == '{')
                    {
                        return S1();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    //读到文件尾时表示读取成功
                    resultHSOList = OutAry();
                    return true;
                }
            }

            bool S1()
            {
                ClearComment();
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (isAry)//读取数组的状态机
                    {
                        if (IsASP(cht))
                        {
                            return S1();
                        }
                        else if (cch == '}')
                        {
                            OutObj();
                            return S0();
                        }
                        else if (cht == ChType.CH)
                        {
                            eleBuf.Append(cch);
                            return S2();
                        }
                        else if (cch == '"')
                        {
                            return SV1();
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else//读取单独对象的状态机
                    {
                        if (IsASP(cht))
                        {
                            return S1();
                        }
                        else if (cht == ChType.CH)
                        {
                            eleBuf.Append(cch);
                            return S2();
                        }
                        else if (cch == '"')
                        {
                            return SV1();
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (isAry)
                    {
                        return false;
                    }
                    else
                    {
                        resultHSO = OutObj();
                        return true;
                    }
                }
            }

            bool S2()
            {
                ClearComment();
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (cht == ChType.CH)
                    {
                        eleBuf.Append(cch);
                        return S2();
                    }
                    else if (IsASP(cht))
                    {
                        FinishContent();
                        return S2F();
                    }
                    else if (cht == ChType.OP)
                    {
                        FinishContent();
                        eleBuf.Append(cch);
                        return S3();
                    }
                    else if (cch == ';')
                    {
                        FinishContent();
                        OutHSI();
                        return S1();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            bool S2F()
            {
                ClearComment();
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (IsASP(cht))
                    {
                        return S2F();
                    }
                    else if (cht == ChType.OP)
                    {
                        eleBuf.Append(cch);
                        return S3();
                    }
                    else if (cch == ';')
                    {
                        OutHSI();
                        return S1();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            bool S3()
            {
                ClearComment();
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (cht == ChType.OP)
                    {
                        eleBuf.Append(cch);
                        return S3();
                    }
                    else if (cht == ChType.CH)
                    {
                        FinishOperater();
                        eleBuf.Append(cch);
                        return S2();
                    }
                    else if (cch == '"')
                    {
                        FinishOperater();
                        return SV1();
                    }
                    else if (IsASP(cht))
                    {
                        FinishOperater();
                        return S4();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            bool S4()
            {
                ClearComment();
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (IsASP(cht))
                    {
                        return S4();
                    }
                    else if (cht == ChType.CH)
                    {
                        eleBuf.Append(cch);
                        return S2();
                    }
                    else if (cch == '"')
                    {
                        return SV1();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            bool SV1()
            {
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (cch == '\n')
                    {
                        return false;
                    }
                    else if (cch == '"')
                    {
                        FinishQuotationContent();
                        return S2F();
                    }
                    else if (cch == '\\')
                    {
                        return SV2();
                    }
                    else
                    {
                        eleBuf.Append(cch);
                        return SV1();
                    }
                }
                else
                {
                    return false;
                }
            }

            bool SV2()
            {
                var rdres = article.Cur();
                article.Next();

                if (rdres.HasValue)
                {
                    var cch = rdres.Value;
                    var cht = CharClassification(cch);
                    if (cch == '\n')
                    {
                        //不允许中途换行
                        return false;
                    }
                    else if (cch == '"' || cch == '\\')
                    {
                        //按原样放入元素缓存
                        eleBuf.Append(cch);
                        return SV1();
                    }
                    else if (cch == 'r')
                    {
                        //回车
                        eleBuf.Append('\r');
                        return SV1();
                    }
                    else if (cch == 'n')
                    {
                        //换行
                        eleBuf.Append('\n');
                        return SV1();
                    }
                    else if (cch == 'b')
                    {
                        //退格
                        eleBuf.Append('\b');
                        return SV1();
                    }
                    else if (cch == 'f')
                    {
                        //换页
                        eleBuf.Append('\f');
                        return SV1();
                    }
                    else if (cch == 't')
                    {
                        //水平制表符
                        eleBuf.Append('\t');
                        return SV1();
                    }
                    else if (cch == 'v')
                    {
                        //垂直制表符
                        eleBuf.Append('\v');
                        return SV1();
                    }
                    else if (cch == 'a')
                    {
                        //响铃
                        eleBuf.Append('\a');
                        return SV1();
                    }
                    else if (cch == '0')
                    {
                        //Null
                        eleBuf.Append('\0');
                        return SV1();
                    }
                    //else if (cht == ChType.ISP || cht == ChType.CH)
                    {
                        //对于其他全部字符,皆可直接放入字符数组
                        //相当于忽视了反斜杠
                        eleBuf.Append(cch);
                        return SV1();
                    }
                }
                else
                {
                    return false;
                }
            }



            #endregion


        }

    }

    /// <summary>
    /// 数据解析时发现的问题将以此异常形式抛出。
    /// </summary>
    public class HowSimpleObjectParsingException : Exception
    {
        public HowSimpleObjectParsingException(string msg) : base(msg) { }
    }

    ///// <summary>
    ///// 希望获取不正确的结果时抛出此异常。
    ///// </summary>
    //public class HowSimpleObjectTypeNotMatchException : Exception
    //{
    //    public HowSimpleObjectTypeNotMatchException(string msg) : base(msg) { }
    //}
}
