using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Resource.Convert
{
    // 將 iOS project 中的 Localizable.strings 檔 轉成 Android project 的 string.xml 檔
    class IOS2AndroidResourceConverter : ResourceConverter
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


            Dictionary<string, PropertyValue> properties = this.LoadiOSResource(fromPath);
            if (properties != null && properties.Count > 0)
            {
                FileInfo file = new FileInfo(fromPath);
                toPath = Path.Combine(file.DirectoryName, "strings.xml");
                if (File.Exists(toPath))
                {
                    File.Delete(toPath);
                }

                this.ExportToAndroidResourceFile(properties, toPath);
                duplicated = this.duplicateds;
                result = true;
            }

            return result;
        }

        private Dictionary<string, PropertyValue> LoadiOSResource(string aResourcePath)
        {
            Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();
            StreamReader reader = null;

            this.duplicateds.Clear();

            try
            {
                string line = null;
                int index = -1;
                reader = File.OpenText(aResourcePath);

                do
                {
                    line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    if (
                       line.Length > 0 &&
                       (index = line.IndexOf(IOS_SEPARATOR)) != -1
                       )
                    {
                        try
                        {
                            line = line.Trim();
                            line = line.Replace("\"", string.Empty);
                            line = line.Replace(";", string.Empty);
                            index = line.IndexOf(IOS_SEPARATOR);

                            string key = line.Substring(0, index);
                            if (!properties.ContainsKey(key))
                            {
                                PropertyValue value = new PropertyValue();
                                value.Type = PropertyType.RESOURCE;
                                value.Content = line.Substring(index + 1);

                                properties.Add(key, value);
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
                        line = line.Trim();

                        if (String.IsNullOrEmpty(line))
                        {
                            // append empty line
                            PropertyValue value = new PropertyValue();
                            value.Type = PropertyType.EMPTY_LINE;
                            value.Content = line;

                            properties.Add(UtilTool.GenerateGUID(), value);
                        }
                        else if (line.StartsWith("//"))
                        {
                            // append comments
                            PropertyValue value = new PropertyValue();
                            value.Type = PropertyType.COMMENTS;
                            value.Content = line.Substring(2);

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

        private void ExportToAndroidResourceFile(Dictionary<string, PropertyValue> properties, string aExportedPath)
        {
            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement res_elem = doc.CreateElement(string.Empty, "resources", string.Empty);
            doc.AppendChild(res_elem);

            foreach (KeyValuePair<string, PropertyValue> pair in properties)
            {
                switch (pair.Value.Type)
                {
                    case PropertyType.RESOURCE:
                        {
                            //string value = pair.Key + IOS_SEPARATOR + pair.Value;

                            XmlElement element = doc.CreateElement(string.Empty, "string", string.Empty);

                            XmlAttribute attr = doc.CreateAttribute("name");
                            attr.Value = pair.Key;
                            element.Attributes.Append(attr);

                            // convert format args
                            string new_content = pair.Value.Content.Replace("%@", "%s");
                            for (int i = 1; i < 9; i++)
                            {
                                new_content = new_content.Replace("%" + i + "$@", "%" + i + "$s");
                            }

                            XmlText text = doc.CreateTextNode(new_content);
                            element.AppendChild(text);

                            res_elem.AppendChild(element);
                        }
                        break;

                    //case PropertyType.EMPTY_LINE:
                    //    {
                    //        XmlWhitespace space = doc.CreateWhitespace("\n");
                    //        res_elem.AppendChild(space);
                    //    }
                    //    break;

                    case PropertyType.COMMENTS:
                        {
                            XmlComment comment =  doc.CreateComment(pair.Value.Content);
                            res_elem.AppendChild(comment);
                        }
                        break;
                }

                XmlWhitespace space = doc.CreateWhitespace("\n");
                res_elem.AppendChild(space);
            }

            doc.Save(aExportedPath);
        }
    }
}
