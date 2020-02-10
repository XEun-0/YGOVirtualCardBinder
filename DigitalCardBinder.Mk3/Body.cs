using DigitalCardBinder.Mk3.ComponentHandlers;
using DigitalCardBinder.Mk3.Components;
using DigitalCardBinder.Mk3.DatabaseHandlers;
using CardElementClassLibrary;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using VisualEffects;
using VisualEffects.Animations.Effects;
using VisualEffects.Easing;

namespace DigitalCardBinder.Mk3
{
    public partial class Body : MetroForm
    {
        int MouseX;
        int MouseY;
        PictureBox ghostCard;
        Boolean ghostCardMove = false;
        Boolean mouseInCCView = false;
        Boolean mouseInLP = false;
        Boolean mouseInRP = false;
        PanelHandler panelHandler;

        //Utility Variables
        private Boolean MoveSelected = false;
        private static Card swapCard = null;

        private static CancellationTokenSource tokenSource;

        //Navigation Variables
        private static Card SelectedCard;
        private PictureBox SelectedSlot = null;
        private static int currentPage = 1;

        //Drop Panel Variables
        //DropPanelThings
        int hideHeight;
        Panel dropped;

        public Body()
        {
            panelHandler = new PanelHandler();
            InitializeComponent();
            ActivateCards();
            BindList(currentType, DirectoryDatabaseManagement.RefreshCombobox());
            PopulateDisplay();
            addCardCopiesIn.Text = "" + 1;
            addCardSlotText.Text = "";
            addCardPageText.Text = "" + currentPage;
            addPageCB.Text = (string)currentType.SelectedItem;

            //Setup Function Icons
            trashIcon.Visible = false;
            changePictureIcon.Visible = false;

            //DropPanel things
            hideHeight = panelDA.Height;
            panelDA.Height = AButton.Height + 1;

            //Console.WriteLine(this.Controls.Count);

            //MonsterElement ce = new MonsterElement();
            //MonsterElement ce2 = new MonsterElement("ai6", "hi", "hi", "hi", "hi", "hi", 1);
            //MonsterElement ce3 = new MonsterElement("ai67", "hi", "hi", "hi", "hi", "hi", 1);
            //MonsterElement ce4 = (MonsterElement)ce3.GetCopy();

            //Console.WriteLine(ce3.GetName() + " < " + ce2.GetName() + " : " + (ce3 < ce2));
            //Console.WriteLine( "ai6".CompareTo("hi3"));
            //Console.WriteLine(" ce4 : " + ce4.GetName());
            
            //Threading ending for splash screen
        }

        //Method for card area click response
        private void AddCardButton_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("TEMPLATE: " + template.Picture + " " + template.PictureLink);
            if (SelectedSlot != null)
            {
                Card template;
                if (addCardNameIn.Text != "")
                {
                    template = new Card((string)currentType.SelectedItem,
                    "" + currentPage,
                    SelectedSlot.Name,
                    addCardNameIn.Text,
                    CardDatabaseManagement.Simplify(addCardNameIn.Text) + ".jpeg",
                    addCardCopiesIn.Text,
                    addCardPicIn.Text);
                }
                else
                {
                    template = new Card(0, SelectedSlot.Name, ""+ currentPage);
                }
                //Console.WriteLine("BEG" + CardDatabaseManagement.Simplify(addCardNameIn.Text) + ".jpeg");
                if (!CardDatabaseManagement.CardExists(template))
                {
                    if (!CardDatabaseManagement.CardExistsInSlot(SelectedSlot.Name, (string)currentType.SelectedItem, currentPage))
                    {
                        try
                        {
                            CardDatabaseManagement.AddCard(template);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            Console.WriteLine("weird");
                            //Form f = new Dialog("Error: \n[Not connected to the internet]\nor\n[Spelled the name incorrectly]\nor\n[Card does not exist]");
                            //DialogResult dr = new DialogResult();
                            //f.ShowDialog();
                            return;
                        }

                        Card cc = CardDatabaseManagement.GetCard((string)currentType.SelectedItem, SelectedSlot.Name, currentPage + "");
                        //Console.WriteLine(cc.Count);
                        if (cc.Count != 1)
                        {
                            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
                                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                                    if (pb.Name == cc.Slot)
                                        pb.Load("database/" + (string)currentType.SelectedItem + "/images/" + cc.Picture);

                            seeCard.Visible = true;
                            seeCard.Load("database/" + (string)currentType.SelectedItem + "/images/" + cc.Picture);
                        }
                    }
                    else
                    {
                        Console.WriteLine("EXISTS IN SLOT");
                        //Form f = new Dialog("Error: \n[Card Exists in Slot]");
                        //f.ShowDialog();
                    }
                }
                else
                {
                    Console.WriteLine("EXIST SAME");
                    //Error Dialog for Card already Existing
                    //Thus press goto to goto the card

                    //Form f = new Dialog("Error: \n[Card Already Exists]");
                    //f.ShowDialog();
                }
            }
            else
            {
                Console.WriteLine("SLECT SLOT");
                //Form f = new Dialog("Error: \n[Select a Slot]");
                //f.ShowDialog();
            }

            addCardPicIn.Text = "";
        }

        private void Card_Click(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            addCardSlotText.Text = p.Name;

            //Console.WriteLine(p.Name);
            if (p.BorderStyle != System.Windows.Forms.BorderStyle.Fixed3D)
            {
                UpdateCardCollectionState(p);
                //Show Big Art
                if (p.Image != null)
                {
                    seeCard.Image = p.Image;
                    seeCard.Visible = true;
                }
                else
                {
                    seeCard.Image = null;
                    seeCard.Visible = false;
                }

                UpdateDisplay(p);

                //GhostCard Maint
                SelectedSlot = p;
                
            }
            else
            {
                p.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                SelectedSlot = null;
                addCardSlotText.Text = "";

                //Show Big Art
                seeCard.Image = null;
                seeCard.Visible = false;
                seeCardNameLabel.Text = "";

                //Show Big Art
                UpdateDisplay(null);

                //GhostCard Maint
                if (MoveSelected)
                {
                    if (ghostCardMove)
                    {
                        ghostCardMove = false;
                        ghostCard.Dispose();
                    }
                }
            }
        }

        private void UpdateDisplay(PictureBox p)
        {
            if (p != null)
            {
                SelectedCard = CardDatabaseManagement.GetCard((string)currentType.SelectedItem, p.Name, currentPage + "");
                seeCardNameLabel.Text = SelectedCard.Name;
            }
            else
            {
                seeCard.Image = null;
                seeCard.Visible = false;
            }
        }

        private void ResetDisplay()
        {
            seeCard.Image = null;
            seeCard.Visible = false;
            seeCardNameLabel.Text = "";
            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                    if (pb.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
                        pb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            GC.Collect();
        }

        private void PopulateDisplay()
        {
            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>()) {
                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                {
                    Card s = CardDatabaseManagement.GetCard((string)currentType.SelectedItem, pb.Name, currentPage + "");
                    if (s.Count > 2)
                    {
                        pb.Load("database/" + (string)currentType.SelectedItem + "/images/" + s.Picture);
                    }
                    else
                    {
                        pb.Image = null;
                    }
                }
            }
            MAX_PAGE_LABEL.Text = "" + PageDatabaseManagement.GetPageNum((string)currentType.SelectedItem);
            currentPageLabel.Text = "Page: " + currentPage;
            GC.Collect();
        }

        private void BindList(ComboBox cb, List<string> ls)
        {
            cb.DataSource = ls;
            cb.DisplayMember = "";
            cb.SelectedIndex = 3;
        }

        //MOUSE ACTIONS
        private void Card_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                PictureBox p = (PictureBox)sender;
                SelectedSlot = p;
                
                if (p.BorderStyle != System.Windows.Forms.BorderStyle.Fixed3D)
                {
                    tokenSource = new CancellationTokenSource();
                    //tokenSource.Cancel(false);
                    //GhostCardInitCanceled = false;
                    //Console.WriteLine(GhostCardInitCanceled);
                    Task task = InitializeGhostCard(tokenSource.Token);
                } 
            }
        }

        private void Card_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //GhostCardInitCanceled = true;
                if(tokenSource != null) 
                    tokenSource.Cancel();
            }
            if (tokenSource != null)
                tokenSource.Dispose();
            tokenSource = null;
        }

        private async Task InitializeGhostCard(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            cancellationToken.ThrowIfCancellationRequested();
           
            if (!ghostCardMove)
            {
                ghostCard = Create_GhostCard(SelectedSlot);
                UpdateGhostCardLocation(SelectedSlot.Parent);
                ghostCardMove = true;
                ghostCard.Parent = SelectedSlot.Parent;
                ghostCard.BringToFront();

                Card t = CardDatabaseManagement.GetCard((string)currentType.SelectedItem, SelectedSlot.Name, currentPage + "");
                if (t.Count != 0)
                {
                    swapCard = t;
                }
            }
        }

        private void GhostCard_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            var mx = pb.Parent.PointToClient(Cursor.Position).X;
            var my = pb.Parent.PointToClient(Cursor.Position).Y;

            //if GhostCard is clicked on the right component, erase it and fill it
            try
            {
                if (panelHandler.GetComponent(pb, mx, my) == turnPageR)
                {
                    //Console.WriteLine(panelHandler.getComponent(pb, mx, my).Name);
                    TurnPageR_Click(null, null);
                }
                else if (panelHandler.GetComponent(pb, mx, my) == turnPageL)
                {
                    //Console.WriteLine(panelHandler.getComponent(pb, mx, my).Name);
                    TurnPageL_Click(null, null);
                }
                else if (panelHandler.GetComponent(pb, mx, my) == trashIcon)
                {
                    //Console.WriteLine("BOORYT;JKF;D");
                    if(trashIcon.Visible ==  true)
                    {
                        CardDatabaseManagement.RemoveCard(swapCard, (string)currentType.SelectedItem);
                        DisposeGhostCard();
                        PopulateDisplay();
                    }
                    else
                    {
                        DisposeGhostCard();
                    }
                }
                else if (panelHandler.GetComponent(pb, mx, my) == changePictureIcon)
                {
                    Console.WriteLine("database\\" + swapCard.Type + "\\images\\" + swapCard.Picture);
                    if (changePictureIcon.Visible == true)
                    {
                        //CardDatabaseManagement.RemoveCard(swapCard, (string)currentType.SelectedItem);
                        Process.Start("database\\" + swapCard.Type + "\\images\\");
                        //CardDatabaseManagement.RemoveCard(swapCard, (string)currentType.SelectedItem);
                        DisposeGhostCard();
                        PopulateDisplay();
                    }
                    else
                    {
                        DisposeGhostCard();
                    }
                }
                else if (panelHandler.GetComponent(pb, mx, my).GetType() == typeof(PictureBox))
                {
                    
                    Control c = panelHandler.GetComponent(pb, mx, my);
                    //Console.WriteLine("Slot: " + c.Name + ", Page: " + currentPage);
                    Card destinationCard = CardDatabaseManagement.GetCard((string)currentType.SelectedItem, c.Name, currentPage + "");
                    //Console.WriteLine("c1: " + swapCard.Count + ", c2: " + destinationCard.Count);
                    //swap card
                    CardDatabaseManagement.SwapCard(ref swapCard, ref destinationCard);
                    PopulateDisplay();
                    DisposeGhostCard();
                }               
            }
            catch (Exception ex)
            {
                ex.ToString();
                DisposeGhostCard();
            }
        }

        private void DisposeGhostCard()
        {
            ghostCard.Dispose();
            ghostCardMove = false;
            ghostCard = null;
            UpdateCardCollectionState(null);
            MoveSelected = false;
            trashIcon.Visible = false;
            changePictureIcon.Visible = false;
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("BOOP");
            if(ghostCardMove)
            {
                mouseInCCView = MouseIn(cardCollectionView);
                mouseInLP = MouseIn(LeftPage);
                mouseInRP = MouseIn(RightPage);
                if(mouseInCCView)
                {
                    if (mouseInLP)
                        UpdateGhostCardLocation(LeftPage);
                    else if (mouseInRP)
                        UpdateGhostCardLocation(RightPage);
                    else
                        UpdateGhostCardLocation(cardCollectionView);
                }

                //Console.WriteLine("mouse in: cc: " + mouseInCCView + ", lp: " + mouseInLP + ", rp: " + mouseInRP);
            }
        }


        //UTILITY METHODS
        private PictureBox Create_GhostCard(PictureBox p)
        {
            PictureBox ghostCard = new PictureBox
            {
                BackColor = System.Drawing.Color.Silver,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                InitialImage = p.InitialImage,
                Location = p.Location,
                Margin = new System.Windows.Forms.Padding(3, 2, 3, 2),
                Name = "GhostCard",
                Size = p.Size,
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage,
                TabIndex = 9,
                TabStop = false
            };
            ghostCard.Click += new System.EventHandler(this.GhostCard_Click);
            ghostCard.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Card_MouseMove);
            ghostCard.Image = p.Image;

            trashIcon.Visible = true;
            changePictureIcon.Visible = true;

            return ghostCard;
        }

        private void ActivateCards()
        {
            Panel p1 = RightPage;
            Panel p2 = LeftPage;

            //Console.WriteLine(p1.Controls[1].Name);

            for(int i = 0; i < p1.Controls.Count; i++)
            {
                p1.Controls[i].MouseDown += new System.Windows.Forms.MouseEventHandler(this.Card_MouseDown);
                p1.Controls[i].MouseUp += new System.Windows.Forms.MouseEventHandler(this.Card_MouseUp);

                p2.Controls[i].MouseDown += new System.Windows.Forms.MouseEventHandler(this.Card_MouseDown);
                p2.Controls[i].MouseUp += new System.Windows.Forms.MouseEventHandler(this.Card_MouseUp);
            }

            //foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
            //    foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
            //    { 
            //        pb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Card_MouseDown);
            //        pb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Card_MouseUp);
            //    }
        }

        private bool MouseIn(Panel p)
        {
            var mx = p.Parent.PointToClient(Cursor.Position).X;
            var my = p.Parent.PointToClient(Cursor.Position).Y;

            //if (p == null) return false;
            if (mx > p.Location.X && mx < p.Location.X + p.Width && my > p.Location.Y && my < p.Location.Y + p.Height)
            {
                return true;
            }
            return false;
        }

        private void UpdateGhostCardLocation(Control c)
        {
            MouseX = c.PointToClient(Cursor.Position).X;
            MouseY = c.PointToClient(Cursor.Position).Y;

            var difX = ghostCard.Width / 2;
            var difY = ghostCard.Height / 2;

            ghostCard.Parent = c;
            ghostCard.BringToFront();
            ghostCard.Location = new Point(MouseX - difX, MouseY - difY);
        }

        private void UpdateCardCollectionState(PictureBox p)
        {
            //Un3D everything
            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                {
                    if (pb != p)
                        pb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    else
                        p.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                    //Console.WriteLine("RUNTHROUGH: " + pb.Name);
                }
        }

        private void TurnPageR_Click(object sender, EventArgs e)
        {
            if(currentPage < PageDatabaseManagement.GetPageNum((string)currentType.SelectedItem))
                currentPage++;
            PopulateDisplay();
            ResetDisplay();
            addCardPageText.Text = "" + currentPage;
        }

        private void TurnPageL_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
                currentPage--;
            PopulateDisplay();
            ResetDisplay();
            addCardPageText.Text = "" + currentPage;
        }

        private void CurrentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ActiveControl = cardCollectionView;
            addCardTypeIn.Text = (string)currentType.SelectedItem;
            addPageCB.Text = (string)currentType.SelectedItem;
            searchCardCB.Text = (string)currentType.SelectedItem;

            //UpdateMaxPageLabel();

            currentPage = 1;
            currentPageLabel.Text = "Page: " + currentPage;
            //ClearAreas();
            //UpdateCB();
            PopulateDisplay();
            ResetDisplay();
        }

        private void DropPanel_Click(object sender, EventArgs e)
        {
            //bool comp = false;
            //updateCB();
            Button b = (Button)sender;
            foreach (Panel gb in ControlPanelCollection.Controls.OfType<Panel>().Where(gb => gb.Name.StartsWith("panelD")))
            {
                if (gb != dropped)
                {

                    //insert method for assigning accept button
                    AssignAcceptButton(b.Name.Substring(0, 1));
                    int maxheight = hideHeight;
                    if ("panelD" + b.Name.Substring(0, 1) == gb.Name)
                    {
                        foreach (GroupBox p in gb.Controls)
                        {
                            if (p.Name == "panel" + b.Name.Substring(0, 1))
                            {
                                maxheight = b.Height + p.Height;
                            }
                        }
                        dropped = gb;
                        
                        gb.Animate(new TopAnchoredHeightEffect(), EasingFunctions.Linear, maxheight + 4, 500, 0);
                    }
                    else
                    {
                        gb.Animate(new TopAnchoredHeightEffect(), EasingFunctions.Linear, 0, 500, 0);
                    }
                }
                else
                {
                    dropped = null;
                    gb.Animate(new TopAnchoredHeightEffect(), EasingFunctions.Linear, 0, 500, 0);
                }
                Invalidate();
            }
        }

        private void AssignAcceptButton(string s)
        {
            //Console.WriteLine(s);
            if (s.Equals("A"))
            {
                this.AcceptButton = addCardButton;
            }
            else if (s.Equals("B"))
            {
                this.AcceptButton = addPageButton;
            }
            else if (s.Equals("C"))
            {
                this.AcceptButton = searchButton;
            }
        }

        private void AddPageButton_Click(object sender, EventArgs e)
        {
            DirectoryDatabaseManagement.AddPage(GetNewPage());
            UpdateMaxPageLabel();
        }

        public string GetNewPage()
        {
            DirectoryInfo d = new DirectoryInfo("database/" + (string)addPageCB.Text + "/");
            int fcount = d.GetFiles("*.xml").Length;
            Directory.CreateDirectory("database/" + (string)addPageCB.Text + "/images");
            return "database/" + (string)addPageCB.Text + "/page" + (fcount + 1) + ".xml";
        }

        private void UpdateMaxPageLabel()
        {
            MAX_PAGE_LABEL.Text = "" + PageDatabaseManagement.GetPageNum((string)currentType.SelectedItem);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            List<Card> cList = new List<Card>();
            if(searchCardName.Text != "")
            {
                if (CheckAll.Checked)
                {
                    foreach (DirectoryInfo f in CardDatabaseManagement.GetDatabasePages())
                    {
                        var i = CardDatabaseManagement.GetCardsMatchingName(searchCardName.Text, f.Name);
                        foreach(Card cc in i)
                        {
                            cList.Add(cc);
                        }
                    }
                }
                else
                {
                    cList = CardDatabaseManagement.GetCardsMatchingName(searchCardName.Text, (string)currentType.SelectedItem);
                }

                DropPanel_Click(sender, null);
            }
            SetupListView(cList);      
        }

        void SetupListView(List<Card> s)
        {
            SearchCardView.Clear();

            ImageList il = new ImageList();

            foreach (Card c in s)
            {
                Image im = Image.FromFile("database/" + c.Type + "/images/" + c.Picture);
                il.Images.Add(im);
            }

            il.ImageSize = new Size(110, 170);
            int count = 0;
            SearchCardView.LargeImageList = il;

            foreach (Card c in s)
            {
                ListViewItem l = new ListViewItem
                {
                    Text = c.Name,
                    Tag = c.Type,
                    ImageIndex = count++
                };
                SearchCardView.Items.Add(l);
            }
        }

        private void SearchCardView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SearchCardView.SelectedIndices.Count > 0)
            {
                //Console.WriteLine();
                int i = SearchCardView.SelectedIndices[0];
                //var card = CardDatabaseManagement.GetCard(SearchCardView.SelectedItems[0].Text, (string)SearchCardView.SelectedItems[0].Tag);

                Console.WriteLine((string)SearchCardView.SelectedItems[0].Tag + ", " + SearchCardView.SelectedItems[0].Text);
                //type slot page
                var card = CardDatabaseManagement.GetCard((string)SearchCardView.SelectedItems[0].Tag, SearchCardView.SelectedItems[0].Text);
                if(card.Slot != "NA")
                {
                    Console.WriteLine(card.Type + ", " + card.Slot + ", PAGE: " + card.Page + ", NAME: " + card.Name);
                    //currentPage = card.Page;
                }
                currentType.SelectedItem = (string)SearchCardView.SelectedItems[0].Tag;
                currentPage = card.Page;
                PopulateDisplay();
                foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
                    foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                        if (pb.Name == card.Slot)
                            Card_Click(pb, null);
            }

            

            SearchCardView.Clear();
        }

        private void GOTO_BUTTON_Click(object sender, EventArgs e)
        {
            if (int.Parse(GOTO_TEXTBOX.Text) <= PageDatabaseManagement.GetPageNum((string)currentType.SelectedItem))
            {
                currentPage = int.Parse(GOTO_TEXTBOX.Text);
                PopulateDisplay();
                ResetDisplay();
                addCardPageText.Text = "" + currentPage;
            }
        }

        private void AddTypeFolder_Click(object sender, EventArgs e)
        {
            DirectoryDatabaseManagement.AddCardType(addTypeText.Text);
            BindList(currentType, DirectoryDatabaseManagement.RefreshCombobox());
        }
    }
}
