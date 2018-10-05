using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resource.Convert
{
    class UtilTool
    {
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
