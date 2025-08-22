namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            button1 = new Button();
            button2 = new Button();
            AMRPC = new NotifyIcon(components);
            pictureBox1 = new PictureBox();
            label2 = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            startToolStripMenuItem = new ToolStripMenuItem();
            stopToolStripMenuItem = new ToolStripMenuItem();
            restoreToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            button3 = new Button();
            linkLabel1 = new LinkLabel();
            linkLabel2 = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(46, 236);
            label1.Name = "label1";
            label1.Size = new Size(304, 50);
            label1.TabIndex = 0;
            label1.Text = "AMRichPresence";
            // 
            // button1
            // 
            button1.AccessibleName = "btnStart";
            button1.Location = new Point(43, 389);
            button1.Name = "button1";
            button1.Size = new Size(75, 34);
            button1.TabIndex = 1;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // button2
            // 
            button2.AccessibleName = "btnStop";
            button2.Location = new Point(124, 389);
            button2.Name = "button2";
            button2.Size = new Size(75, 34);
            button2.TabIndex = 2;
            button2.Text = "Stop";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // AMRPC
            // 
            AMRPC.BalloonTipIcon = ToolTipIcon.Info;
            AMRPC.BalloonTipTitle = "AMRichPresence";
            AMRPC.Icon = (Icon)resources.GetObject("AMRPC.Icon");
            AMRPC.Text = "AMRPC";
            AMRPC.Visible = true;
            AMRPC.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // pictureBox1
            // 
            pictureBox1.ImageLocation = "C:\\Users\\musta\\Downloads\\gurt.png";
            pictureBox1.Location = new Point(83, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(216, 210);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.Location = new Point(43, 314);
            label2.Name = "label2";
            label2.Size = new Size(326, 57);
            label2.TabIndex = 5;
            label2.Text = "AMRichPresence is a Program that allow's \r\nDiscord and Apple music to sync between platforms\r\n\r\n";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { startToolStripMenuItem, stopToolStripMenuItem, restoreToolStripMenuItem, exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(114, 92);
            // 
            // startToolStripMenuItem
            // 
            startToolStripMenuItem.Name = "startToolStripMenuItem";
            startToolStripMenuItem.Size = new Size(113, 22);
            startToolStripMenuItem.Text = "Start";
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.Size = new Size(113, 22);
            stopToolStripMenuItem.Text = "Stop";
            // 
            // restoreToolStripMenuItem
            // 
            restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            restoreToolStripMenuItem.Size = new Size(113, 22);
            restoreToolStripMenuItem.Text = "Restore";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(113, 22);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // button3
            // 
            button3.Location = new Point(205, 389);
            button3.Name = "button3";
            button3.Size = new Size(112, 34);
            button3.TabIndex = 6;
            button3.Text = "Minimize to tray";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click_2;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(95, 286);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(43, 15);
            linkLabel1.TabIndex = 7;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Github";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.Location = new Point(164, 286);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(111, 15);
            linkLabel2.TabIndex = 8;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "sponge104's Github";
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(386, 465);
            Controls.Add(linkLabel2);
            Controls.Add(linkLabel1);
            Controls.Add(button3);
            Controls.Add(label2);
            Controls.Add(pictureBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "AMRPC";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }



        // Add this method to match the event handler assigned in InitializeComponent
        private void button3_Click_1(object sender, EventArgs e)
        {
            // You can implement the minimize to tray logic here
            // For now, just hide the form as a placeholder
            this.Hide();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Label label1;
        private Button button1;
        private Button button2;
        private NotifyIcon AMRPC;
        private PictureBox pictureBox1;
        private Label label2;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem startToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem;
        private ToolStripMenuItem restoreToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private Button button3;
        private LinkLabel linkLabel1;
        private LinkLabel linkLabel2;
    }
}
