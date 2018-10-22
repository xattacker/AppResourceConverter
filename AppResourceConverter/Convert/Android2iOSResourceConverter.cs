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

                    if (line.Length > 0 && line.StartsWith("<string"))
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

                            PropertyValue value = new PropertyValue();
                            value.Type = PropertyType.COMMENTS;
                            value.Content = line.Substring(4, end_index - 4);

                            properties.Add(UtilTool.GenerateGUID(), value);
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
                            value.Content = "\"" + node.InnerText.Trim() + "\";";
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

        private void ExportToiOSResourceFile(Dictionary<string, PropertyValue> properties, string aExportedPath)
        {
            string[] new_contents = new string[properties.Count];
            int index = 0;

            foreach (KeyValuePair<string, PropertyValue> pair in properties)
            {
                // resouce string
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

                            new_contents[index] = pair.Key + IOS_SEPARATOR + new_content;
                        }
                        break;


                    case PropertyType.COMMENTS:
                        new_contents[index] = "//" + pair.Value.Content;
                        break;

                    case PropertyType.EMPTY_LINE:
                        new_contents[index] = string.Empty;
                        break;
                }

                index++;
            }

            File.WriteAllLines(aExportedPath, new_contents, Encoding.UTF8);
        }
    }
}
