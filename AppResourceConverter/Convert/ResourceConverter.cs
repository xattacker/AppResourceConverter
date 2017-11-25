using System;
using System.Collections.Generic;

namespace Resource.Convert
{
    abstract class ResourceConverter
    {
        protected const string IOS_SEPARATOR = "=";
        protected List<string> duplicateds = new List<string>();

        public abstract bool Convert(string fromPath, out string toPath, out List<string> duplicated);
    }
}
