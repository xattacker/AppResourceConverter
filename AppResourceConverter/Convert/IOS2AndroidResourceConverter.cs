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


            Dictionary<string, string> properties = this.LoadiOSResource(fromPath);
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

        private Dictionary<string, string> LoadiOSResource(string aResourcePath)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
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
                                properties.Add(key, line.Substring(index + 1));
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

        private void ExportToAndroidResourceFile(Dictionary<string, string> properties, string aExportedPath)
        {
            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement res_elem = doc.CreateElement(string.Empty, "resources", string.Empty);
            doc.AppendChild(res_elem);

            foreach (KeyValuePair<string, string> pair in properties)
            {
               string value = pair.Key + IOS_SEPARATOR + pair.Value;

               XmlElement element = doc.CreateElement(string.Empty, "string", string.Empty);

               XmlAttribute attr = doc.CreateAttribute("name");
               attr.Value = pair.Key;
               element.Attributes.Append(attr);

               XmlText text = doc.CreateTextNode(pair.Value);
               element.AppendChild(text);

               res_elem.AppendChild(element);
            }

            doc.Save(aExportedPath);
        }
    }
}
