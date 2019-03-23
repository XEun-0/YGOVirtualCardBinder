using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCardBinder.Mk3.Components
{
    public struct Card
    {
        private string name;
        private string page;
        private string picture;
        private string slot;
        private string copies;
        private string pictureLink;

        private string type;

        private int count;

        public Card(int i, string islot, string ipage)
        {
            name = "";
            page = ipage;
            picture = "";
            slot = islot;
            copies = "";
            pictureLink = "";
            type = "";
            count = i;
        }

        public Card(string itype, string ipage, string islot, string iname, string ipicture, string icopies, string ipicLink)
        {
            type = itype;
            name = iname;
            page = ipage;
            slot = islot;
            picture = ipicture;
            copies = icopies;
            pictureLink = ipicLink;
            count = 7;
        }

        //Placeholder Card for switching
        public Card(string islot)
        {
            name = "";
            page = "";
            picture = "";
            slot = islot;
            copies = "";
            pictureLink = "";
            type = "";
            count = 0;
        }

        //Similar to an Array's Add method.
        //Specific to adding the type factor to a Card.
        public void Add(string n)
        {
            type = n;
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Picture
        {
            get { return picture; }
            set { picture = value; }
        }

        public string Slot
        {
            get { return slot; }
            set { slot = value; }
        }

        public int Page
        {
            get { return int.Parse(page); }
            set { page = "" + value; }
        }

        public string Copies
        {
            get { return copies; }
            set { copies = value; }
        }

        public string PictureLink
        {
            get { return pictureLink; }
            set { pictureLink = value; }
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        public Boolean Equals(Card c)
        {
            if (c.Name == Name
                && c.Page == Page
                && c.Type == Type
                && c.Slot == Slot
                && c.Picture == Picture)
                return true;
            return false;
        }
    }
}
