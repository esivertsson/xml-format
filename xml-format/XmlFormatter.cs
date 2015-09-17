using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace XmlFormat
{
    public static class XmlFormatter
    {
        private static int _indentation;
        private static FormatSettings _settings;

        public static string Format(string xmlValue, FormatSettings settings)
        {
            _indentation = 0;
            _settings = settings;
            StringBuilder output = new StringBuilder();
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

            string linarizedXml = LinarizeXml(xmlValue);
            using (StringReader stringReader = new StringReader(linarizedXml))
            {
                using (XmlReader reader = XmlReader.Create(stringReader, xmlReaderSettings))
                {
                    while (reader.Read())
                    {
                        bool increaseIndentAfter = false;
                        bool decreaseIndentBefore = false;
                        StringBuilder content = new StringBuilder();
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.IsEmptyElement && !reader.HasAttributes)
                                {
                                    content.AppendFormat("<{0} />", reader.Name);
                                }
                                else if (reader.IsEmptyElement && reader.HasAttributes)
                                {
                                    content.AppendFormat("<{0}", reader.Name);
                                    if (_settings.IgnoreNewLineWhenManyAttributes.Contains(reader.Name))
                                        WriteAttributesStraight(reader, content);
                                    else
                                        WriteAttributes(reader, content, _settings.NewLineWhenManyAttributes);
                                    content.AppendFormat(" />");
                                }
                                else if (reader.HasAttributes)
                                {
                                    content.AppendFormat("<{0}", reader.Name);
                                    WriteAttributes(reader, content);
                                    content.AppendFormat(">");
                                    increaseIndentAfter = true;
                                }
                                else
                                {
                                    content.AppendFormat("<{0}>", reader.Name);
                                    increaseIndentAfter = true;
                                }
                                break;
                            case XmlNodeType.Text:
                                content.AppendFormat(reader.Value);
                                break;
                            case XmlNodeType.CDATA:
                                content.AppendFormat("<![CDATA[{0}]]>", reader.Value);
                                break;
                            case XmlNodeType.ProcessingInstruction:
                                content.AppendFormat("<?{0} {1}?>", reader.Name, reader.Value);
                                break;
                            case XmlNodeType.Comment:
                                content.AppendFormat("<!--{0}-->", reader.Value);
                                break;
                            case XmlNodeType.XmlDeclaration:
                                content.AppendFormat("<?xml version=\"1.0\"?>"); // TODO: reader.Value instead?
                                break;
                            case XmlNodeType.Document:
                                break;
                            case XmlNodeType.DocumentType:
                                content.AppendFormat("<!DOCTYPE {0} [{1}]", reader.Name, reader.Value);
                                break;
                            case XmlNodeType.EntityReference:
                                content.AppendFormat(reader.Name);
                                break;
                            case XmlNodeType.EndElement:
                                decreaseIndentBefore = true;
                                content.AppendFormat("</{0}>", reader.Name);
                                break;
                            case XmlNodeType.Whitespace:
                                continue;
                        }

                        if (decreaseIndentBefore)
                        {
                            _indentation--;
                        }

                        //Console.Write("{0}{1}{2}{3}", _indentation, Indent(), content, Environment.NewLine);
                        output.AppendFormat("{0}{1}{2}", Indent(), content, Environment.NewLine);

                        if (increaseIndentAfter)
                        {
                            _indentation++;
                        }
                    }
                }
            }

            // Normalize line endings: http://stackoverflow.com/questions/841396/what-is-a-quick-way-to-force-crlf-in-c-sharp-net
            output.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
            return output.ToString();
        }

        private static string LinarizeXml(string xmlValue)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.MaxCharactersFromEntities = 1024;
            xmlReaderSettings.XmlResolver = null;
            xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;

            XmlDocument xmlDocument = new XmlDocument();
            using (StringReader stringReader = new StringReader(xmlValue))
            {
                using (XmlReader reader = XmlReader.Create(stringReader, xmlReaderSettings))
                {
                    xmlDocument.Load(reader);
                }
            }

            StringWriter sw = new StringWriter(new StringBuilder());
            XmlWriterSettings writerSettings = new XmlWriterSettings
            {
                Indent = false,
                NewLineOnAttributes = false
            };
            XmlWriter w = XmlWriter.Create(sw, writerSettings);
            XmlWrappingWriter xmlFormatter = new XmlWrappingWriter(w);

            xmlDocument.WriteContentTo(xmlFormatter);
            xmlFormatter.Close();
            return sw.ToString();
        }

        private static void WriteAttributesStraight(XmlReader reader, StringBuilder content)
        {
            WriteAttributes(reader, content, false);
        }

        private static void WriteAttributes(XmlReader reader, StringBuilder content, bool newLineAfterFirstAttribute = true)
        {
            string elementName = reader.Name;
            reader.MoveToFirstAttribute();
            content.AppendFormat(" {0}=\"{1}\"", reader.Name, reader.Value);

            while (reader.MoveToNextAttribute())
            {
                if (newLineAfterFirstAttribute)
                {
                    content.AppendFormat("{0}{1}", Environment.NewLine, IndentAttribute(elementName));
                }
                content.AppendFormat(" {0}=\"{1}\"", reader.Name, reader.Value);
            }
        }

        private static string IndentAttribute(string elementName)
        {
            StringBuilder output = new StringBuilder();
            output.Append(Indent());

            for (int i = 0; i <= elementName.Length; i++)
            {
                output.Append(" ");
            }

            return output.ToString();
        }

        private static string Indent()
        {
            if (_indentation == 0)
            {
                return string.Empty;
            }

            StringBuilder output = new StringBuilder();
            for (int i = 1; i <= _indentation; i++)
            {
                output.Append(_settings.IndentString);
            }

            return output.ToString();
        }
    }

  public class FormatSettings
  {
    /// <summary>
    /// String that will be used when indenting for child elements.
    /// </summary>
    public string IndentString = "  ";

    /// <summary>
    /// When there is more that one attribute on an element, all attribute but first is on new line.
    /// <element first="attribute"
    ///          second="attribute" />
    /// </summary>
    public bool NewLineWhenManyAttributes = true;

    /// <summary>
    /// Ignore <see cref="NewLineWhenManyAttributes"/> for elements in this list.
    /// </summary>
    public List<string> IgnoreNewLineWhenManyAttributes = new List<string> { "section", "add" };
    
    /// <summary>
    /// 
    /// </summary>
    public List<string> KeepElementOnOneLine = new List<string> { "AttributeValue" };

    // TODO: Encoding on formatted file as setting?
    // TODO: ToLower on all strings in settings
  }
}