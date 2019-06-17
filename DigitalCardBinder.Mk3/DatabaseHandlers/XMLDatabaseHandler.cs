using CardElementClassLibrary;
using DigitalCardBinder.Mk3.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalCardBinder.Mk3.DatabaseHandlers
{
    public static class XMLDatabaseHandler
    {


        public static CardElement GetCard(string type, string name)
        {
            try
            {
                string nm = name;
                string pic = "";
                string copies = "";
                XmlDocument xd = new XmlDocument();
                FileInfo[] fi = GetDatabasePages(type);
                foreach (FileInfo ff in fi)
                {
                    FileStream file = new FileStream("database/" + type + "/" + ff.Name, FileMode.Open);
                    xd.Load(file);
                    XmlNodeList list = xd.GetElementsByTagName("Card");

                    for (int i = 0; i < list.Count; i++)
                    {
                        XmlElement c1 = (XmlElement)xd.GetElementsByTagName("Slot")[i];
                        XmlElement c2 = (XmlElement)xd.GetElementsByTagName("Card")[i];
                        XmlElement c3 = (XmlElement)xd.GetElementsByTagName("Picture")[i];
                        XmlElement c4 = (XmlElement)xd.GetElementsByTagName("Copies")[i];
                        nm = c2.GetAttribute("Name");

                        Console.WriteLine(Simplify(nm) + ", " + Simplify(name) + ", " + ff.Name);

                        if (Simplify(nm).Equals(Simplify(name)))
                        {
                            pic = c3.InnerText;
                            copies = "" + c4.InnerText;
                            string pg = ff.Name.Substring(4);
                            pg = pg.Remove(pg.IndexOf(".xml"));
                            Console.WriteLine("Page: " + pg);
                            file.Close();

                            //Using var to get whatever type the CardElement is
                            var cardElement = new MonsterElement();

                            return cardElement;
                        }
                    }
                    file.Close();
                }
                return new MonsterElement();
            }
            catch (IOException e)
            {
                e.ToString();
                //Console.WriteLine("BAKKAKAKAKAKA");
                return new MonsterElement();
            }
        }

        private static FileInfo[] GetDatabasePages(string type)
        {
            throw new NotImplementedException();
        }

        public static string Simplify(string s)
        {
            string name = s.ToLower();
            name = new string(name.Select(c => char.IsPunctuation(c) ? '/' : c).ToArray());
            name = name.Replace("/", " ");

            //Console.WriteLine("BE" + name.Substring(0, 1) + "END");
            if (name.Substring(0, 1).Equals(" "))
            {
                name = name.Substring(1);
            }

            return name;
        }
    }


}
