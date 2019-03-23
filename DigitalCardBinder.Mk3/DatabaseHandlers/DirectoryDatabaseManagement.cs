using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalCardBinder.Mk3.DatabaseHandlers
{
    class DirectoryDatabaseManagement
    {
        public List<string> RefreshCombobox()
        {
            List<string> l = new List<string>();

            FileStream file = new FileStream("database/cardList.xml", FileMode.Open);
            XmlDocument xd = new XmlDocument();
            xd.Load(file);

            XmlNodeList list = xd.GetElementsByTagName("cardTypeDatabase");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xd.GetElementsByTagName("cardTypeDatabase")[i];
                l.Add(cl.GetAttribute("CardType"));
            }
            file.Close();

            l.Sort();
            return l;
        }

        public void AddPage(string page)
        {
            XmlTextWriter xtw;
            //Console.WriteLine(page);
            xtw = new XmlTextWriter(page, Encoding.UTF8);
            xtw.WriteStartDocument();
            xtw.WriteStartElement("Cards");
            xtw.WriteEndElement();
            xtw.Close();
        }

        public void AddCardType(string name)
        {
            Boolean b = false;
            XmlDocument xd = new XmlDocument();
            FileStream file = new FileStream("database/cardList.xml", FileMode.Open);
            xd.Load(file);
            XmlNodeList list = xd.GetElementsByTagName("TypeDetails");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xd.GetElementsByTagName("TypeDetails")[i];
                if ((cl.GetAttribute("CardType")) == name)
                {
                    b = true;
                }
            }

            if (b == false)
            {
                XmlElement type = xd.CreateElement("cardTypeDatabase");
                type.SetAttribute("CardType", name);
                xd.DocumentElement.AppendChild(type);
            }

            file.Close();
            xd.Save("database/cardList.xml");
            RefreshDatabaseDirectory();
        }

        public void RefreshDatabaseDirectory()
        {
            DirectoryInfo d = new DirectoryInfo("database/");
            DirectoryInfo[] f = d.GetDirectories();

            FileStream file = new FileStream("database/cardList.xml", FileMode.Open);
            XmlDocument xd = new XmlDocument();
            xd.Load(file);
            XmlNodeList list = xd.GetElementsByTagName("cardTypeDatabase");
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement cl = (XmlElement)xd.GetElementsByTagName("cardTypeDatabase")[i];
                if (!Directory.Exists("database/" + (cl.GetAttribute("CardType"))))
                {
                    Directory.CreateDirectory("database/" + cl.GetAttribute("CardType"));
                    Directory.CreateDirectory("database/" + cl.GetAttribute("CardType") + "/images");
                    AddPage("database/" + cl.GetAttribute("CardType") + "/page1.xml");
                }
            }
            file.Close();
            xd.Save("database/cardList.xml");
        }
    }
}
