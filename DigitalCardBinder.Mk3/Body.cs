using DigitalCardBinder.Mk3.ComponentHandlers;
using DigitalCardBinder.Mk3.Components;
using DigitalCardBinder.Mk3.DatabaseHandlers;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
//using VisualEffects;
//using VisualEffects.Animations.Effects;
//using VisualEffects.Easing;

namespace DigitalCardBinder.Mk3
{
    public partial class Body : MetroForm
    {

        //Database Handlers
        static CardDatabaseManagement cdm = new CardDatabaseManagement();
        static PageDatabaseManagement pdm = new PageDatabaseManagement();
        static DirectoryDatabaseManagement ddm = new DirectoryDatabaseManagement();

        int MouseX;
        int MouseY;
        PictureBox ghostCard;
        Boolean ghostCardMove = false;
        Boolean mouseInCCView = false;
        Boolean mouseInLP = false;
        Boolean mouseInRP = false;
        //Boolean GhostCardInitCanceled = false;
        PanelHandler panelHandler;

        //Utility Variables
        private Boolean MoveSelected = false;
        private static Card swapCard = new Card();

        private static CancellationTokenSource tokenSource;

        //Navigation Variables
        private static Card SelectedCard;
        private PictureBox SelectedSlot = null;
        private int currentPage = 1;

        //Drop Panel Variables
        //DropPanelThings
        int hideHeight;
        Panel dropped;

        public Body()
        {
            panelHandler = new PanelHandler();
            InitializeComponent();
            ActivateCards();
            BindList(currentType, ddm.RefreshCombobox());
            //currentType.SelectedIndex = 0;
            
            PopulateDisplay();
            Console.WriteLine("BREJLFSAJFDSJ;LF");
            addCardCopiesIn.Text = "" + 1;
            addCardSlotText.Text = "";
            addCardPageText.Text = "" + currentPage;
            addPageCB.Text = (string)currentType.SelectedItem;

            //Setup Trash Function
            trashIcon.Visible = false;

            //DropPanel things
            hideHeight = panelDA.Height;
            panelDA.Height = AButton.Height + 1;
        }

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
                    cdm.Simplify(addCardNameIn.Text) + ".jpeg",
                    addCardCopiesIn.Text,
                    addCardPicIn.Text);
                }
                else
                {
                    template = new Card(0, SelectedSlot.Name, ""+ currentPage);
                }
                
                if (!cdm.CardExists(template))
                {
                    if (!cdm.CardExistsInSlot(SelectedSlot.Name, (string)currentType.SelectedItem, currentPage))
                    {
                        try
                        {
                            cdm.AddCard(template);
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

                        Card cc = cdm.GetCard((string)currentType.SelectedItem, SelectedSlot.Name, currentPage + "");
                        //Console.WriteLine(cc.Count);
                        if(cc.Count != 1)
                        {
                            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
                                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                                    if (pb.Name == cc.Slot)
                                        pb.Load("database/" + currentType.SelectedItem + "/images/" + cc.Picture);

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
                SelectedCard = cdm.GetCard((string)currentType.SelectedItem, p.Name, currentPage + "");
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
            //Console.WriteLine(currentType.SelectedValue + " BREAK 1");
            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>()) { 
                //Console.WriteLine(currentType.SelectedValue + " BREAK 2");
                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                {
                    Card s = cdm.GetCard((string)currentType.SelectedItem, pb.Name, currentPage + "");

                    if (s.Count > 2)
                    {
                        //Console.WriteLine("database/" + (string)currentType.SelectedItem + "/images/" + s.Picture);
                        pb.Load("database/" + (string)currentType.SelectedItem + "/images/" + s.Picture);
                    }
                    else
                    {
                        pb.Image = null;
                    }
                }
            }
            MAX_PAGE_LABEL.Text = "" + pdm.getPageNum((string)currentType.SelectedItem);
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

                Card t = cdm.GetCard((string)currentType.SelectedItem, SelectedSlot.Name, currentPage + "");
                if (t.Count != 0)
                {
                    swapCard = t;
                }

                //Console.WriteLine(t.Name);
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
                if (panelHandler.getComponent(pb, mx, my) == turnPageR)
                {
                    //Console.WriteLine(panelHandler.getComponent(pb, mx, my).Name);
                    TurnPageR_Click(null, null);
                }
                else if (panelHandler.getComponent(pb, mx, my) == turnPageL)
                {
                    //Console.WriteLine(panelHandler.getComponent(pb, mx, my).Name);
                    TurnPageL_Click(null, null);
                }
                else if (panelHandler.getComponent(pb, mx, my) == trashIcon)
                {
                    //Console.WriteLine("BOORYT;JKF;D");
                    if(trashIcon.Visible ==  true)
                    {
                        cdm.RemoveCard(swapCard, (string)currentType.SelectedItem);
                        DisposeGhostCard();
                        PopulateDisplay();
                    }
                    else
                    {
                        DisposeGhostCard();
                    }
                }
                else if (panelHandler.getComponent(pb, mx, my).GetType() == typeof(PictureBox))
                {
                    
                    Control c = panelHandler.getComponent(pb, mx, my);
                    //Console.WriteLine("Slot: " + c.Name + ", Page: " + currentPage);
                    Card destinationCard = cdm.GetCard((string)currentType.SelectedItem, c.Name, currentPage + "");
                    //Console.WriteLine("c1: " + swapCard.Count + ", c2: " + destinationCard.Count);
                    //swap card
                    cdm.SwapCard(swapCard, destinationCard);
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

            return ghostCard;
        }

        private void ActivateCards()
        {
            foreach (Panel c in cardCollectionView.Controls.OfType<Panel>())
                foreach (PictureBox pb in c.Controls.OfType<PictureBox>())
                {
                    //pb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Card_MouseMove);
                    pb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Card_MouseDown);
                    pb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Card_MouseUp);
                }
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
            if(currentPage < pdm.getPageNum((string)currentType.SelectedItem))
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
            ddm.AddPage(GetNewPage());
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
            MAX_PAGE_LABEL.Text = "" + pdm.getPageNum((string)currentType.SelectedItem);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(cdm.Simplify(addTypeText.Text));
        }
    }
}
