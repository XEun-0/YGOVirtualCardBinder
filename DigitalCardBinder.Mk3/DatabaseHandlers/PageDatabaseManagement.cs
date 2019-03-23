using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCardBinder.Mk3.DatabaseHandlers
{
    class PageDatabaseManagement
    {
        public int getPageNum(string path)
        {
            DirectoryInfo d = new DirectoryInfo("database/" + path + "/");
            FileInfo[] f = d.GetFiles("*.xml");

            return f.Count();
        }
    }
}
