using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace XmlFormat
{
    public class XmlFormatJsonConfig
    {
        /// <summary>
        /// Ignore <see cref="NewLineWhenManyAttributes"/> for elements in this list.
        /// </summary>
        public IList<string> IgnoreNewLineWhenManyAttributes { get; set; }

        public IList<string> KeepElementOnOneLine { get; set; }

        /// <summary>
        /// String that will be used when indenting child elements.
        /// </summary>
        public string IndentString { get; set; }

        /// <summary>
        /// When there is more that one attribute on an element, all attribute but first is on new line.
        /// <element first="attribute"
        ///          second="attribute" />
        /// </summary>
        public bool NewLineWhenManyAttributes { get; set; }

        public static XmlFormatJsonConfig FromJsonFile(string file)
        {
            return JsonConvert.DeserializeObject<XmlFormatJsonConfig>(File.ReadAllText(file));
        }
    }
}
