using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Resource.Convert
{
    // 將 Android project 的 string.xml 檔 轉成 iOS project 中的 Localizable.strings 檔
    class Android2iOSResourceConverter
    {
        private const string SEPARATOR = "=";

        public bool Convert(string fromPath, out string toPath)
        {
            bool result = false;
            toPath = null;

            if (!File.Exists(fromPath))
            {
                return false;
            }


            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(fromPath));

            XmlElement root = doc.DocumentElement;
            XmlNodeList list = root.SelectNodes("string");

            Dictionary<string, string> properties = new Dictionary<string, string>();
            List<string> duplicated = new List<string>();

            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    string id = ((XmlElement)node).GetAttribute("name").Trim();

                    if (!properties.ContainsKey(id))
                    {
                        properties.Add("\"" + id + "\"", "\"" + node.InnerText.Trim() + "\"");
                    }
                    else
                    {
                        duplicated.Add(id);
                    }
                }

                if (duplicated.Count > 0)
                {
                    Console.WriteLine("\nduplicated key: ");

                    foreach (string key in duplicated)
                    {
                        Console.WriteLine(key);
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

                result = true;
            }

            return result;
        }

        private void ExportToiOSResourceFile(Dictionary<string, string> properties, string aExportedPath)
        {
            string[] new_contents = new string[properties.Count];
            int index = 0;

            foreach (KeyValuePair<string, string> pair in properties)
            {
                 // resouce string
                 new_contents[index] = pair.Key + SEPARATOR + pair.Value;

                index++;
            }

            File.WriteAllLines(aExportedPath, new_contents, Encoding.UTF8);
        }
    }
}
