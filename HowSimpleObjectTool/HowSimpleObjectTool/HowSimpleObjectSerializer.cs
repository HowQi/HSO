using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HowSimpleObjectTool
{
    public class HowSimpleObjectSerializer
    {
        public static void WriteObj(HowSimpleObject obj, IPaper paper)
        {
            var srl = new Serializer(paper, obj);
            srl.SerializeObj();
        }

        public static void WriteObj(HowSimpleObject obj, StringBuilder sb)
        {
            var paper = new StringBuilderPaper(sb);
            WriteObj(obj, paper);
        }

        public static void WriteObj(HowSimpleObject obj, StreamWriter sw)
        {
            var paper = new StreamWriterPaper(sw);
            WriteObj(obj, paper);
        }

        public static void WriteAry(HowSimpleObject[] objs, IPaper paper)
        {
            var srl = new Serializer(paper, objs);
            srl.SerializeAry();
        }

        public static void WriteAry(HowSimpleObject[] objs, StringBuilder sb)
        {
            var paper = new StringBuilderPaper(sb);
            WriteAry(objs, paper);
        }
        
        public static void WriteAry(HowSimpleObject[] objs, StreamWriter sw)
        {
            var paper = new StreamWriterPaper(sw);
            WriteAry(objs, paper);
        }

        public interface IPaper
        {
            void Write(string article);
            void Write(char ch);
        }

        public class StringBuilderPaper : IPaper
        {
            StringBuilder inner;
            public StringBuilderPaper(StringBuilder stringBuilder)
            {
                inner = stringBuilder;
            }
            public void Write(string article)
            {
                inner.Append(article);
            }
            public void Write(char ch)
            {
                inner.Append(ch);
            }
        }

        public class StreamWriterPaper : IPaper
        {
            StreamWriter inner;
            public StreamWriterPaper(StreamWriter streamWriter)
            {
                inner = streamWriter;
            }
            public void Write(string article)
            {
                inner.Write(article);
            }
            public void Write(char ch)
            {
                inner.Write(ch);
            }
        }

        private class Serializer
        {
            bool isAry;
            IPaper paper;
            HowSimpleObject obj;
            HowSimpleObject[] objs;
            int indentation = 0;
            public Serializer(IPaper paper_, HowSimpleObject obj_)
            {
                paper = paper_;
                obj = obj_;
                isAry = false;
            }

            public Serializer(IPaper paper_, HowSimpleObject[] objs_)
            {
                paper = paper_;
                objs = objs_;
                isAry = true;
            }

            public void SerializeObj()
            {
                if (isAry) throw new TypeNotMatchException("您建立的对象用于生成对象数组,但是却使用了生成独立对象的函数");
                FillObj(obj);
            }

            public void SerializeAry()
            {
                if (!isAry) throw new TypeNotMatchException("您建立的对象用于生成独立对象,但是却使用了生成数组的函数");
                FillObjAry(objs);
            }

            private void DrawIndentation()
            {
                for (int i = 0; i < indentation; i++)
                {
                    paper.Write(' ');
                }
            }

            private void DrawLine()
            {
                paper.Write("\r\n");
            }

            private void DrawQuotContent(string content)
            {
                paper.Write('"');

                for (int i = 0; i < content.Length; i++)
                {
                    char cur = content[i];
                    if (cur == '"' || cur == '\\')
                    {
                        paper.Write('\\');
                        paper.Write(cur);
                    }
                    else if (cur == '\r')
                    {
                        paper.Write('\\');
                        paper.Write('r');
                    }
                    else if (cur == '\n')
                    {
                        paper.Write('\\');
                        paper.Write('n');
                    }
                    else if (cur == '\b')
                    {
                        paper.Write('\\');
                        paper.Write('b');
                    }
                    else if (cur == '\f')
                    {
                        paper.Write('\\');
                        paper.Write('f');
                    }
                    else if (cur == '\t')
                    {
                        paper.Write('\\');
                        paper.Write('t');
                    }
                    else if (cur == '\v')
                    {
                        paper.Write('\\');
                        paper.Write('v');
                    }
                    else if (cur == '\a')
                    {
                        paper.Write('\\');
                        paper.Write('a');
                    }
                    else if (cur == '0')
                    {
                        paper.Write('\\');
                        paper.Write('0');
                    }
                    else
                    {
                        paper.Write(cur);
                    }
                }

                paper.Write('"');
            }

            private void FillObjAry(HowSimpleObject[] ary)
            {
                for (int i = 0; i < ary.Length; i++)
                {
                    DrawIndentation();
                    paper.Write('{');
                    DrawLine();

                    indentation += 4;
                    FillObj(ary[i]);
                    indentation -= 4;

                    DrawIndentation();
                    paper.Write('}');
                    DrawLine();
                }
            }

            private void FillObj(HowSimpleObject sobj)
            {
                foreach (var item in sobj.inner)
                {
                    switch (item.Value.ItemType)
                    {
                        case HowSimpleItemType.Value:
                            DrawVal(item.Key, item.Value as HowSimpleValueItem);
                            break;
                        case HowSimpleItemType.Dict:
                            DrawDict(item.Key, item.Value as HowSimpleDictionaryItem);
                            break;
                        case HowSimpleItemType.Ary:
                            DrawAry(item.Key, item.Value as HowSimpleArrayItem);
                            break;
                        default:
                            break;
                    }
                }
            }

            private void DrawVal(string attrName, HowSimpleValueItem vali)
            {
                DrawIndentation();
                paper.Write(attrName);
                paper.Write('=');
                DrawQuotContent(vali.Value);
                paper.Write(';');
                DrawLine();
            }

            private void DrawAry(string attrName, HowSimpleArrayItem aryi)
            {
                for (int i = 0; i < aryi.Count; i++)
                {
                    DrawIndentation();
                    paper.Write(attrName);
                    paper.Write("+=");
                    DrawQuotContent(aryi[i]);
                    paper.Write(';');
                    DrawLine();
                }
            }

            private void DrawDict(string attrName, HowSimpleDictionaryItem dicti)
            {
                foreach (var item in dicti.inner)
                {
                    var dattrName = item.Key;
                    DrawIndentation();
                    paper.Write(attrName);
                    paper.Write('.');
                    paper.Write(dattrName);
                    paper.Write('=');
                    DrawQuotContent(item.Value);
                    paper.Write(';');
                    DrawLine();
                }
            }

        }

    }
}
