using DigitalCardBinder.Mk3.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CardElementClassLibrary;

namespace DigitalCardBinder.Mk3.DatabaseHandlers
{
    //xml file checking stuff
    static class CardDatabaseManagement
    {
        //private static Dictionary<int, string> dict = new Dictionary<int, string>();
        private static string nname;

        public static Card GetCard(string slot, int page)
        {
            return new Card(1, slot, "" + page);
        }

        #region Testing CardElements
        //Incorporating CardElements
        public static CardElement GetCard(string name, string type, string slot, string page)
        {
            try
            {
                string nname = "";
                string pic = "";
                string copies = "";
                XmlDocument xd = new XmlDocument();
                FileStream file = new FileStream("database/" + type + "/page" + page + ".xml", FileMode.Open);
                xd.Load(file);
                XmlNodeList list = xd.GetElementsByTagName("Card");

                for (int i = 0; i < list.Count; i++)
                {
                    XmlElement c1 = (XmlElement)xd.GetElementsByTagName("Slot")[i];
                    XmlElement c2 = (XmlElement)xd.GetElementsByTagName("Card")[i];
                    XmlElement c3 = (XmlElement)xd.GetElementsByTagName("Picture")[i];
                    XmlElement c4 = (XmlElement)xd.GetElementsByTagName("Copies")[i];
                    //Console.WriteLine(slot + " " + c3.InnerText);
                    if ((c1.InnerText).Equals(slot))
                    {
                        nname = c2.GetAttribute("Name");
                        pic = c3.InnerText;
                        copies = "" + c4.InnerText;
                        file.Close();
                        //return new Card(type, page, slot, name, Simplify(name) + ".jpeg", copies, pic);
                        return null;
                    }
                }
                file.Close();
                return null;
            }
            catch (IOException e)
            {
                e.ToString();
                //Console.WriteLine("BAKKAKAKAKAKA");
                return null;
            }
        }
        #endregion

        public static FileInfo[] GetDatabasePages(string type)
        {
            DirectoryInfo d = new DirectoryInfo("database/" + type + "/");
            FileInfo[] f = d.GetFiles();

            return f;
        }

        public static DirectoryInfo[] GetDatabasePages()
        {
            DirectoryInfo d = new DirectoryInfo("database/");
            DirectoryInfo[] dir = d.GetDirectories();

            return dir;
        }

        public static Card GetCard(string type, string slot, string page)
        {
            try
            {
                string name = "";
                string pic = "";
                string copies = "";
                XmlDocument xd = new XmlDocument();
                FileStream file = new FileStream("database/" + type + "/page" + page + ".xml", FileMode.Open);
                xd.Load(file);
                XmlNodeList list = xd.GetElementsByTagName("Card");

                for (int i = 0; i < list.Count; i++)
                {
                    XmlElement c1 = (XmlElement)xd.GetElementsByTagName("Slot")[i];
                    XmlElement c2 = (XmlElement)xd.GetElementsByTagName("Card")[i];
                    XmlElement c3 = (XmlElement)xd.GetElementsByTagName("Picture")[i];
                    XmlElement c4 = (XmlElement)xd.GetElementsByTagName("Copies")[i];
                    //Console.WriteLine(slot + " " + c3.InnerText);
                    if ((c1.InnerText).Equals(slot))
                    {
                        name = c2.GetAttribute("Name");
                        pic = c3.InnerText;
                        copies = "" + c4.InnerText;
                        file.Close();
                        return new Card(type, page, slot, name, Simplify(name) + ".jpeg", copies, pic);
                    }
                }
                file.Close();
                return new Card(1, slot, page);
            }
            catch (IOException e)
            {
                e.ToString();
                //Console.WriteLine("BAKKAKAKAKAKA");
                return new Card(slot);
            }
        }

        public static Card GetCard(string type, string name)
        {
            try
            {
                string nm = name;
                string pic = "";
                string copies = "";
                XmlDocument xd = new XmlDocument();
                FileInfo[] fi = GetDatabasePages(type);
                foreach(FileInfo ff in fi)
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
                            return new Card(type, pg, c1.InnerText, name, Simplify(name) + ".jpeg", copies, pic);
                        }
                    }
                    file.Close();
                }
                return new Card("NA");
            }
            catch (IOException e)
            {
                e.ToString();
                //Console.WriteLine("BAKKAKAKAKAKA");
                return new Card("NA");
            }
        }

        public static List<Card> GetCardsMatchingName(string iname, string type)
        {
            List<Card> c = new List<Card>();
            try
            {
                XmlDocument xd = new XmlDocument();

                foreach (FileInfo f in GetDatabasePages(type))
                {
                    FileStream file = new FileStream(f.DirectoryName + "/" + f.Name, FileMode.Open);
                    xd.Load(file);
                    XmlNodeList list = xd.GetElementsByTagName("Card");
                    for (int i = 0; i < list.Count; i++)
                    {
                        XmlElement c1 = (XmlElement)xd.GetElementsByTagName("Slot")[i];
                        XmlElement c2 = (XmlElement)xd.GetElementsByTagName("Card")[i];
                        XmlElement c3 = (XmlElement)xd.GetElementsByTagName("Picture")[i];
                        XmlElement c4 = (XmlElement)xd.GetElementsByTagName("Copies")[i];
                        //string page = f.Name.Substring(f.Name.Length - 5);

                        string page = f.Name.Remove(f.Name.IndexOf(".xml"));
                        page = page.Substring(page.IndexOf("page") + 4);
                        if(Simplify(c2.GetAttribute("Name")).Contains(Simplify(iname)))
                        {
                            Card temp = new Card(
                                type,
                                page,
                                c1.InnerText,
                                c2.GetAttribute("Name"),
                                c3.InnerText,
                                c4.InnerText,
                                ""
                            );
                            //Console.WriteLine(temp.Name);
                            c.Add(temp);
                        }
                    }
                    //Console.WriteLine(file.Name);
                    file.Close();
                }
                return c;
            }
            catch (IOException e)
            {
                e.ToString();
                return new List<Card> { }; ;
            }
        }

        //testing Comment for branches
        public static void AddCard(Card card)
        {
            bool cardExist = CardExists(card);
            //Console.WriteLine("TEMPLATE: " + card.Picture + ", " + card.PictureLink);
            //Console.WriteLine("PICTURE: " + PictureExist("Aqua", "psychic kappa.jpeg"));
            if (card.Count != 1)
            {
                if (!PictureExist(card.Type, card.Picture))
                {
                    //Console.WriteLine("NOT EXISTO");
                    if (card.PictureLink == "" || card.PictureLink == null)
                    {
                        WebClient webClient = new WebClient();

                        //Convert name to simpler String
                        string nn = Simplify(card.Name);

                        webClient.DownloadFile("http://yugiohprices.com/api/card_image/" + card.Name, "database/" + card.Type + "/images/" + card.Picture);
                        webClient.Dispose();
                    }
                    else
                    {
                        //string nn = Simplify(card.Name);

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(card.PictureLink, "database/" + card.Type + "/images/" + card.Picture);
                        }
                        GC.Collect();
                    }
                }

                if (!cardExist)
                {
                    XmlDocument xd = new XmlDocument();
                    FileStream file = new FileStream("database/" + card.Type + "/page" + card.Page + ".xml", FileMode.Open);
                    xd.Load(file);
                    XmlElement cl = xd.CreateElement("Card");
                    cl.SetAttribute("Name", card.Name);

                    XmlElement slt = xd.CreateElement("Slot");
                    XmlText natext = xd.CreateTextNode(card.Slot);
                    slt.AppendChild(natext);

                    XmlElement pic = xd.CreateElement("Picture");
                    XmlText ptext = xd.CreateTextNode(card.Picture);
                    pic.AppendChild(ptext);

                    XmlElement cp = xd.CreateElement("Copies");
                    XmlText ctext = xd.CreateTextNode(card.Copies);
                    cp.AppendChild(ctext);

                    cl.AppendChild(slt);
                    cl.AppendChild(pic);
                    cl.AppendChild(cp);
                    xd.DocumentElement.AppendChild(cl);
                    file.Close();
                    xd.Save("database/" + card.Type + "/page" + card.Page + ".xml");
                }
            }
            else
            {
                //Console.WriteLine("MONKEY BUTT");
            }
        }

        private static Boolean PictureExist(string type, string picture)
        {
            DirectoryInfo d = new DirectoryInfo("database/" + type + "/images/");
            FileInfo[] f = d.GetFiles("*.jpeg");
            foreach (FileInfo fi in f)
            {
                //Console.WriteLine(type + ", " + fi.Name + ", " + picture);
                if (Simplify(fi.Name).Equals(Simplify(picture))) //name should be url i think
                {
                    nname = fi.Name;
                    //Console.WriteLine("NAME: " + Simplify(fi.Name) + ", NAME2: " + Simplify(picture));
                    return true;
                }
            }
            return false;
        }

        public static void RemoveCard(Card c, string type)
        {
            if(c.Count != 1)
            {
                XmlDocument xd = new XmlDocument();

                FileStream file = new FileStream("database/" + type + "/page" + c.Page + ".xml", FileMode.Open);
                //Stream file = u.getStream("YugiohOrganizerApp.database." + type + ".page" + ls[1] + ".xml");
                xd.Load(file);
                XmlNodeList list = xd.GetElementsByTagName("Card");

                for (int i = 0; i < list.Count; i++)
                {
                    XmlElement c1 = (XmlElement)xd.GetElementsByTagName("Card")[i];
                    XmlElement c2 = (XmlElement)xd.GetElementsByTagName("Slot")[i];

                    if (c2.InnerText.Equals(c.Slot))
                    {
                        xd.DocumentElement.RemoveChild(c1);
                        break;
                    }
                }
                file.Close();
                xd.Save("database/" + type + "/page" + c.Page + ".xml");
            }   
        }

        public static Boolean CardExists(Card c)
        {
            DirectoryInfo d = new DirectoryInfo("database/" + c.Type + "/");
            FileInfo[] f = d.GetFiles("*.xml");
            //string[] f = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            XmlDocument xd = new XmlDocument();
            foreach (FileInfo fi in f)
            {
                FileStream file = new FileStream("database/" + c.Type + "/" + fi.Name, FileMode.Open);
                //Stream file = u.getStream("YugiohOrganizerApp.database." + type + "." + fi);
                xd.Load(file);
                XmlNodeList list = xd.GetElementsByTagName("Card");
                for (int i = 0; i < list.Count; i++)
                {
                    XmlElement c2 = (XmlElement)xd.GetElementsByTagName("Card")[i];
                    if (Simplify(c.Name) == Simplify(c2.GetAttribute("Name")))
                    {
                        file.Close();
                        return true;
                    }
                }
                file.Close();
            }
            return false;
        }

        public static Boolean CardExistsInSlot(string slot, string type, int page)
        {
            DirectoryInfo d = new DirectoryInfo("database/" + type + "/");
            FileInfo[] f = d.GetFiles("*.xml");
            //string[] f = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            XmlDocument xd = new XmlDocument();
            foreach (FileInfo fi in f)
            {
                if (fi.Name == "page" + page + ".xml")
                {
                    FileStream file = new FileStream("database/" + type + "/" + fi.Name, FileMode.Open);
                    //Stream file = u.getStream("YugiohOrganizerApp.database." + type + "." + fi);
                    xd.Load(file);
                    XmlNodeList list = xd.GetElementsByTagName("Card");
                    for (int i = 0; i < list.Count; i++)
                    {
                        XmlElement c1 = (XmlElement)xd.GetElementsByTagName("Slot")[i];

                        if (slot == c1.InnerText)
                        {
                            file.Close();
                            return true;
                        }
                    }
                    file.Close();
                }
            }
            return false;
        }

        public static void SwapCard(ref Card c1, ref Card c2)
        {
            //c1.SwapLocInfo(ref c2);
            //Console.WriteLine(c1.Slot + ", " + c1.Page + " : " + c2.Slot + ", " + c2.Page);

            if (!c1.Equals(c2))
            {
                //Console.WriteLine(c1.Slot + ", " + c1.Count + ", Page: " + c1.Page);
                //Console.WriteLine(c2.Slot + ", " + c2.Count + ", Page: " + c2.Page);
                string c1slot = c2.Slot;
                string c2slot = c1.Slot;
                int c1page = c2.Page;
                int c2page = c1.Page;
                RemoveCard(c1, c1.Type);
                RemoveCard(c2, c2.Type);
                //Console.WriteLine(CardExists(c1));
                //Console.WriteLine("SWAP: c1 " + c1slot + ", c2 " + c2slot);
                c1.Slot = c1slot;
                c2.Slot = c2slot;
                c1.Page = c1page;
                c2.Page = c2page;
                //Console.WriteLine("SWAP: c1 " + c1.Name + " : " + c1.Slot + ", c2 " + c2.Name + " : " + c2.Slot);
                AddCard(c1);
                AddCard(c2);
            }

            //if (!c1.Equals(c2))
            //{
            //    RemoveCard(c1, c1.Type);
            //    RemoveCard(c2, c2.Type);
            //    var temp = c1;
            //    var tempSlot = c1.Slot;
            //    var tempPage = c1.Page;
            //    c1 = c2;
            //    c1.Slot = c2.Slot;
            //    c1.Page = c2.Page;
                
            //    c2 = temp;
            //    c2.Slot = tempSlot;
            //    c2.Page = tempPage;

            //    AddCard(c1);
            //    AddCard(c2);
            //}
        }

        public static string Simplify(string s)
        {
            string name = s.ToLower();
            name = new string(name.Select(c => char.IsPunctuation(c) ? '/' : c).ToArray());
            name = name.Replace("/", " ");

            //Console.WriteLine("BE" + name.Substring(0, 1) + "END");
            if(name.Substring(0,1).Equals(" "))
            {
                name = name.Substring(1);
            }

            return name;
        }
    }
}
