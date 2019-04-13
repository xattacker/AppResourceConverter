using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Resource.Convert
{
    // 將 Android project 的 string.xml 檔 轉成 iOS project 中的 Localizable.strings 檔
    class Android2iOSResourceConverter : ResourceConverter
    {
        private CommentBlock commentTemp;

        public override bool Convert(string fromPath, out string toPath, out List<string> duplicated)
        {
            bool result = false;
            toPath = null;
            duplicated = null;

            if (!File.Exists(fromPath))
            {
                return false;
            }


            this.duplicateds.Clear();

            Dictionary<string, PropertyValue> properties = this.LoadByReadLine(fromPath, ref this.duplicateds);
            if (properties != null && properties.Count > 0)
            {
                if (properties.Count > 0)
                {
                    FileInfo file = new FileInfo(fromPath);
                    toPath = Path.Combine(file.DirectoryName, "Localizable.strings");

                    if (File.Exists(toPath))
                    {
                        File.Delete(toPath);
                    }

                    this.ExportToiOSResourceFile(properties, toPath);
                }

                duplicated = this.duplicateds;
                result = true;
            }

            return result;
        }

        private Dictionary<string, PropertyValue> LoadByReadLine(string filePath, ref List<string> duplicated)
        {
            Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();
            StreamReader reader = null;

            try
            {
                string line = null;
                reader = File.OpenText(filePath);

                do
                {
                    line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }


                    line = line.Trim();

                    if (line.Length > 0 && line.StartsWith("<string") && commentTemp == null)
                    {
                        try
                        {
                            int key_start_index = line.IndexOf("=\"");
                            int key_end_index = line.IndexOf("\">");
                            string key = line.Substring(key_start_index + 2, key_end_index - key_start_index - 2);

                            if (!properties.ContainsKey(key))
                            {
                                int value_start_index = line.IndexOf("</string>");
                                string content = line.Substring(key_end_index + 2, value_start_index - key_end_index - 2).UnescapeXml();
                                content = content.Replace("\"", "\\\"");

                                PropertyValue value = new PropertyValue();
                                value.Type = PropertyType.RESOURCE;
                                value.Content = "\"" + content + "\";";
                                properties.Add("\"" + key + "\"", value);
                            }
                            else
                            {
                                duplicateds.Add(key);
                            }
                        }
                        catch
                        {
                            // ignore one line error
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(line))
                        {
                            // append empty line
                            PropertyValue value = new PropertyValue();
                            value.Type = PropertyType.EMPTY_LINE;
                            value.Content = line;

                            properties.Add(UtilTool.GenerateGUID(), value);
                        }
                        else if (line.StartsWith("<!--"))
                        {
                            // append comments
                            int end_index = line.LastIndexOf("-->");
                            if (end_index >= 4)
                            {
                                PropertyValue value = new PropertyValue();
                                value.Type = PropertyType.COMMENTS;
                                value.Content = line.Substring(4, end_index - 4);
                                properties.Add(UtilTool.GenerateGUID(), value);

                                commentTemp = null;
                            }
                            else
                            {
                                commentTemp = new CommentBlock();
                                commentTemp.comment = line.Substring(4);
                            }
                        }
                        else if (line.EndsWith("-->"))
                        {
                            int end_index = line.LastIndexOf("-->");
                            string temp = line.Substring(0, end_index);
                            commentTemp.comment += temp.Trim();

                            PropertyValue value = new PropertyValue();
                            value.Type = PropertyType.COMMENTS;
                            value.Content = commentTemp.comment;
                            properties.Add(UtilTool.GenerateGUID(), value);
                            commentTemp = null;
                        }
                        else if (commentTemp != null)
                        {
                            commentTemp.comment += line + "\n";
                        }
                    }

                } while (true);
            }
            catch
            {
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }

            return properties;
        }


        #region unused

        private Dictionary<string, PropertyValue> LoadByXmlDocument(string filePath, ref List<string> duplicated)
        {
            Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(filePath));

            XmlElement root = doc.DocumentElement;
            XmlNodeList list = root.ChildNodes; //.SelectNodes("string");

            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    if (node is XmlElement)
                    {
                        string id = ((XmlElement)node).GetAttribute("name").Trim();

                        if (!properties.ContainsKey(id))
                        {
                            PropertyValue value = new PropertyValue();
                            value.Type = PropertyType.RESOURCE;

                            string content = node.InnerText.Trim();
                            content = content.Replace("\"", "\\\"");
                            value.Content = "\"" + content + "\";";

                            properties.Add("\"" + id + "\"", value);
                        }
                        else
                        {
                            duplicateds.Add(id);
                        }
                    }
                    else if (node is XmlComment)
                    {
                        XmlComment comment = (XmlComment)node;

                        PropertyValue value = new PropertyValue();
                        value.Type = PropertyType.COMMENTS;
                        value.Content = comment.Data;

                        properties.Add(UtilTool.GenerateGUID(), value);
                    }
                    else if (node is XmlWhitespace)
                    {
                        PropertyValue value = new PropertyValue();
                        value.Type = PropertyType.EMPTY_LINE;

                        properties.Add(UtilTool.GenerateGUID(), value);
                    }
                }
            }

            return properties;
        }

        #endregion


        private void ExportToiOSResourceFile(Dictionary<string, PropertyValue> properties, string aExportedPath)
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, PropertyValue> pair in properties)
            {
                switch (pair.Value.Type)
                {
                    case PropertyType.RESOURCE:
                        {
                            // convert format args
                            string new_content = pair.Value.Content.Replace("%s", "%@");
                            for (int i = 1; i < 9; i++)
                            {
                                new_content = new_content.Replace("%" + i + "s", "%" + i + "@");
                            }

                            new_content = new_content.Replace("%S", "%@");
                            for (int i = 1; i < 9; i++)
                            {
                                new_content = new_content.Replace("%" + i + "S", "%" + i + "@");
                            }

                            builder.Append(pair.Key + IOS_SEPARATOR + new_content);
                            builder.Append("\n");
                        }
                        break;


                    case PropertyType.COMMENTS:
                        {
                            string[] keyword = { "\n" };
                            string[] array = pair.Value.Content.Split(keyword, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string line in array)
                            {
                                builder.Append("//" + line);
                                builder.Append("\n");
                            }
                        }
                        break;

                    case PropertyType.EMPTY_LINE:
                        builder.Append(string.Empty);
                        builder.Append("\n");
                        break;
                }
            }

            File.WriteAllText(aExportedPath, builder.ToString(), Encoding.UTF8);
        }
    }


    class CommentBlock
    {
        public string comment;
    }
}
