using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Resource.Convert
{
    // convert resource file to unicode properties file
    class I18nResourceConverter
    {
        private const string SEPARATOR = "=";

        public bool Convert(string resPath)
        {
            bool result = false;
            StreamReader reader = null;
            StreamWriter writer = null;

            try
            {
                string line = null;
                int index = -1;

                FileInfo file = new FileInfo(resPath);
                string name = file.Name.Substring(0, file.Name.IndexOf(".") + 1) + "properties";
                string properties_path = Path.Combine(file.DirectoryName, name);

                // delete old file
                file = new FileInfo(properties_path);
                file.Delete();

                reader = File.OpenText(resPath);
                writer = new StreamWriter(properties_path, true);

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
                            line = this.ConvertToUniString(line.Trim());
                            //Console.WriteLine(line);

                            writer.WriteLine(line);
                            writer.Flush();
                        }
                        catch
                        {
                            // ignore one line error
                        }
                    }
                    else
                    {
                        writer.WriteLine(line);
                        writer.Flush();
                    }

                } while (true);

                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (reader != null)
                {
                    try
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    finally
                    {
                        reader = null;
                    }
                }

                if (writer != null)
                {
                    try
                    {
                        writer.Close();
                        writer.Dispose();
                    }
                    finally
                    {
                        writer = null;
                    }
                }
            }

            GC.Collect();

            return result;
        }

        private string ConvertToUniString(string value)
        {
            StringBuilder builder = new StringBuilder();

            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    builder.Append(encodedValue);
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
