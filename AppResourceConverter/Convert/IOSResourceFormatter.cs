using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Resource.Convert
{
    // 重整 iOS project 中的 Localizable.strings 檔中, 移除掉重覆的key值 
    class IOSResourceFormatter
    {
        private const string SEPARATOR = "=";

        public bool Format(string filePath, out string newFilePath)
        {
            bool result = false;
            newFilePath = null;

            if (!File.Exists(filePath))
            {
                return false;
            }


            StreamReader reader = null;

            try
            {
                string line = null;
                int index = -1;
                Dictionary<string, PropertyValue> properties = new Dictionary<string, PropertyValue>();
                reader = File.OpenText(filePath);

                do
                {
                    line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }


                    if (
                       line.Length > 0 &&
                       (index = line.IndexOf(SEPARATOR)) != -1
                       )
                    {
                        try
                        {
                            line = line.Trim();

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
                                Console.WriteLine("\nduplicated key: " + key);
                            }
                        }
                        catch
                        {
                            // ignore one line error
                        }
                    }
                    else
                    {
                        // append others
                        PropertyValue value = new PropertyValue();
                        value.Type = PropertyType.OTHERS;
                        value.Content = line;

                        properties.Add(this.GenerateGUID(), value);
                    }

                } while (true);


                FileInfo file = new FileInfo(filePath);
                newFilePath = Path.Combine(file.DirectoryName, "new_" + file.Name);
                this.ExportFile(properties, newFilePath);

                result = true;
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

            return result;
        }

        private void ExportFile(Dictionary<string, PropertyValue> properties, string filePath)
        {
            string[] new_contents = new string[properties.Count];
            int index = 0;

            foreach (KeyValuePair<string, PropertyValue> pair in properties)
            {
                if (pair.Value.Type == PropertyType.RESOURCE)
                {
                    // resouce string
                    new_contents[index] = pair.Key + SEPARATOR + pair.Value.Content;
                }
                else
                {
                    // others
                    new_contents[index] = pair.Value.Content;
                }

                index++;
            }

            File.WriteAllLines(filePath, new_contents, Encoding.UTF8);
        }

        public string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }
    }


    enum PropertyType : ushort
    {
        RESOURCE = 0,
        OTHERS
    }

    struct PropertyValue
    {
        public PropertyType Type;
        public string Content;
    }
}
