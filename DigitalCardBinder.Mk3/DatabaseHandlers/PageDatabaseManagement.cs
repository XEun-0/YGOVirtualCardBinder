using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCardBinder.Mk3.DatabaseHandlers
{
    static class PageDatabaseManagement
    {
        public static int GetPageNum(string path)
        {
            DirectoryInfo d = new DirectoryInfo("database/" + path + "/");
            FileInfo[] f = d.GetFiles("*.xml");

            return f.Count();
        }
    }
}
