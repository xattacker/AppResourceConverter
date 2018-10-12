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


            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(fromPath));

            XmlElement root = doc.DocumentElement;
            XmlNodeList list = root.ChildNodes; //.SelectNodes("string");

            Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();

            this.duplicateds.Clear();

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
                            this.duplicateds.Add(id);
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
                        new_contents[index] = "";
                        break;
                }

                index++;
            }

            File.WriteAllLines(aExportedPath, new_contents, Encoding.UTF8);
        }
    }
}
