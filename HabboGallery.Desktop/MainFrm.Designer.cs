namespace HabboGallery.Desktop
{
    partial class MainFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RememberMeBx = new System.Windows.Forms.CheckBox();
            this.LoginErrorLbl = new System.Windows.Forms.Label();
            this.LoginSubmitBtn = new System.Windows.Forms.Button();
            this.LoginTitleLbl = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LoginPasswordTxt = new System.Windows.Forms.TextBox();
            this.LoginEmailTxt = new System.Windows.Forms.TextBox();
            this.LoginPnl = new System.Windows.Forms.Panel();
            this.ExportCertificateBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.StatusLbl = new System.Windows.Forms.Label();
            this.CloseBtn = new System.Windows.Forms.Panel();
            this.HotelExtensionDownBtn = new System.Windows.Forms.Panel();
            this.HotelExtensionUpBtn = new System.Windows.Forms.Panel();
            this.ZoomLbl = new System.Windows.Forms.Label();
            this.IndexDisplayLbl = new System.Windows.Forms.Label();
            this.DragPnl = new System.Windows.Forms.Panel();
            this.BuyBtn = new System.Windows.Forms.Panel();
            this.NextBtn = new System.Windows.Forms.Panel();
            this.PreviousBtn = new System.Windows.Forms.Panel();
            this.SearchBtn = new System.Windows.Forms.Panel();
            this.PublishBtn = new System.Windows.Forms.Panel();
            this.DescriptionLbl = new System.Windows.Forms.Label();
            this.PhotoPreviewBx = new System.Windows.Forms.PictureBox();
            this.LoginPnl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PhotoPreviewBx)).BeginInit();
            this.SuspendLayout();
            // 
            // RememberMeBx
            // 
            this.RememberMeBx.AutoSize = true;
            this.RememberMeBx.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RememberMeBx.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RememberMeBx.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.RememberMeBx.Location = new System.Drawing.Point(154, 111);
            this.RememberMeBx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.RememberMeBx.Name = "RememberMeBx";
            this.RememberMeBx.Size = new System.Drawing.Size(28, 19);
            this.RememberMeBx.TabIndex = 45;
            this.RememberMeBx.Text = "º";
            this.RememberMeBx.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.RememberMeBx.UseVisualStyleBackColor = true;
            this.RememberMeBx.CheckedChanged += new System.EventHandler(this.RememberMeBx_CheckedChanged);
            // 
            // LoginErrorLbl
            // 
            this.LoginErrorLbl.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.LoginErrorLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LoginErrorLbl.ForeColor = System.Drawing.Color.Red;
            this.LoginErrorLbl.Location = new System.Drawing.Point(71, 102);
            this.LoginErrorLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LoginErrorLbl.Name = "LoginErrorLbl";
            this.LoginErrorLbl.Size = new System.Drawing.Size(70, 30);
            this.LoginErrorLbl.TabIndex = 44;
            this.LoginErrorLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoginErrorLbl.UseCompatibleTextRendering = true;
            // 
            // LoginSubmitBtn
            // 
            this.LoginSubmitBtn.BackColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.LoginSubmitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LoginSubmitBtn.ForeColor = System.Drawing.Color.Black;
            this.LoginSubmitBtn.Location = new System.Drawing.Point(5, 105);
            this.LoginSubmitBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoginSubmitBtn.Name = "LoginSubmitBtn";
            this.LoginSubmitBtn.Size = new System.Drawing.Size(64, 27);
            this.LoginSubmitBtn.TabIndex = 43;
            this.LoginSubmitBtn.Text = "Log in";
            this.LoginSubmitBtn.UseVisualStyleBackColor = false;
            this.LoginSubmitBtn.Click += new System.EventHandler(this.LoginSubmitBtn_Click);
            // 
            // LoginTitleLbl
            // 
            this.LoginTitleLbl.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.LoginTitleLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
            this.LoginTitleLbl.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.LoginTitleLbl.Location = new System.Drawing.Point(110, 3);
            this.LoginTitleLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LoginTitleLbl.Name = "LoginTitleLbl";
            this.LoginTitleLbl.Size = new System.Drawing.Size(105, 18);
            this.LoginTitleLbl.TabIndex = 42;
            this.LoginTitleLbl.Text = "Create account";
            this.LoginTitleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoginTitleLbl.UseCompatibleTextRendering = true;
            this.LoginTitleLbl.Click += new System.EventHandler(this.LoginTitleLbl_Click);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.label2.Location = new System.Drawing.Point(5, 52);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 18);
            this.label2.TabIndex = 41;
            this.label2.Text = "Password";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.label2.UseCompatibleTextRendering = true;
            // 
            // LoginPasswordTxt
            // 
            this.LoginPasswordTxt.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.LoginPasswordTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LoginPasswordTxt.ForeColor = System.Drawing.Color.White;
            this.LoginPasswordTxt.Location = new System.Drawing.Point(5, 74);
            this.LoginPasswordTxt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoginPasswordTxt.Name = "LoginPasswordTxt";
            this.LoginPasswordTxt.PasswordChar = '•';
            this.LoginPasswordTxt.Size = new System.Drawing.Size(179, 23);
            this.LoginPasswordTxt.TabIndex = 40;
            // 
            // LoginEmailTxt
            // 
            this.LoginEmailTxt.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.LoginEmailTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LoginEmailTxt.ForeColor = System.Drawing.Color.White;
            this.LoginEmailTxt.Location = new System.Drawing.Point(5, 27);
            this.LoginEmailTxt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoginEmailTxt.Name = "LoginEmailTxt";
            this.LoginEmailTxt.Size = new System.Drawing.Size(179, 23);
            this.LoginEmailTxt.TabIndex = 0;
            // 
            // LoginPnl
            // 
            this.LoginPnl.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.LoginPnl.Controls.Add(this.ExportCertificateBtn);
            this.LoginPnl.Controls.Add(this.RememberMeBx);
            this.LoginPnl.Controls.Add(this.LoginErrorLbl);
            this.LoginPnl.Controls.Add(this.LoginSubmitBtn);
            this.LoginPnl.Controls.Add(this.label2);
            this.LoginPnl.Controls.Add(this.LoginPasswordTxt);
            this.LoginPnl.Controls.Add(this.label1);
            this.LoginPnl.Controls.Add(this.LoginEmailTxt);
            this.LoginPnl.Controls.Add(this.LoginTitleLbl);
            this.LoginPnl.Location = new System.Drawing.Point(34, 27);
            this.LoginPnl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoginPnl.Name = "LoginPnl";
            this.LoginPnl.Size = new System.Drawing.Size(190, 136);
            this.LoginPnl.TabIndex = 53;
            // 
            // ExportCertificateBtn
            // 
            this.ExportCertificateBtn.BackColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.ExportCertificateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExportCertificateBtn.ForeColor = System.Drawing.Color.Black;
            this.ExportCertificateBtn.Location = new System.Drawing.Point(76, 105);
            this.ExportCertificateBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ExportCertificateBtn.Name = "ExportCertificateBtn";
            this.ExportCertificateBtn.Size = new System.Drawing.Size(65, 27);
            this.ExportCertificateBtn.TabIndex = 46;
            this.ExportCertificateBtn.Text = "Cert";
            this.ExportCertificateBtn.UseVisualStyleBackColor = false;
            this.ExportCertificateBtn.Click += new System.EventHandler(this.ExportCertificateBtn_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 18);
            this.label1.TabIndex = 39;
            this.label1.Text = "E-mail address";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.label1.UseCompatibleTextRendering = true;
            // 
            // StatusLbl
            // 
            this.StatusLbl.BackColor = System.Drawing.Color.Transparent;
            this.StatusLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.StatusLbl.ForeColor = System.Drawing.Color.FromArgb(221, 221, 221);
            this.StatusLbl.Location = new System.Drawing.Point(14, 329);
            this.StatusLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.StatusLbl.Name = "StatusLbl";
            this.StatusLbl.Size = new System.Drawing.Size(230, 18);
            this.StatusLbl.TabIndex = 52;
            this.StatusLbl.Text = "Standby";
            this.StatusLbl.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.StatusLbl.UseCompatibleTextRendering = true;
            // 
            // CloseBtn
            // 
            this.CloseBtn.BackColor = System.Drawing.Color.Transparent;
            this.CloseBtn.Location = new System.Drawing.Point(212, 6);
            this.CloseBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(18, 17);
            this.CloseBtn.TabIndex = 51;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // HotelExtensionDownBtn
            // 
            this.HotelExtensionDownBtn.BackColor = System.Drawing.Color.Transparent;
            this.HotelExtensionDownBtn.Location = new System.Drawing.Point(198, 303);
            this.HotelExtensionDownBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.HotelExtensionDownBtn.Name = "HotelExtensionDownBtn";
            this.HotelExtensionDownBtn.Size = new System.Drawing.Size(28, 18);
            this.HotelExtensionDownBtn.TabIndex = 50;
            this.HotelExtensionDownBtn.Click += new System.EventHandler(this.HotelExtensionDownBtn_Click);
            // 
            // HotelExtensionUpBtn
            // 
            this.HotelExtensionUpBtn.BackColor = System.Drawing.Color.Transparent;
            this.HotelExtensionUpBtn.Location = new System.Drawing.Point(198, 268);
            this.HotelExtensionUpBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.HotelExtensionUpBtn.Name = "HotelExtensionUpBtn";
            this.HotelExtensionUpBtn.Size = new System.Drawing.Size(28, 18);
            this.HotelExtensionUpBtn.TabIndex = 49;
            this.HotelExtensionUpBtn.Click += new System.EventHandler(this.HotelExtensionUpBtn_Click);
            // 
            // ZoomLbl
            // 
            this.ZoomLbl.BackColor = System.Drawing.Color.Transparent;
            this.ZoomLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            this.ZoomLbl.ForeColor = System.Drawing.Color.FromArgb(221, 221, 221);
            this.ZoomLbl.Location = new System.Drawing.Point(203, 290);
            this.ZoomLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ZoomLbl.Name = "ZoomLbl";
            this.ZoomLbl.Size = new System.Drawing.Size(41, 17);
            this.ZoomLbl.TabIndex = 48;
            this.ZoomLbl.Text = "2X";
            // 
            // IndexDisplayLbl
            // 
            this.IndexDisplayLbl.BackColor = System.Drawing.Color.FromArgb(68, 68, 68);
            this.IndexDisplayLbl.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.IndexDisplayLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.IndexDisplayLbl.Location = new System.Drawing.Point(93, 5);
            this.IndexDisplayLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IndexDisplayLbl.Name = "IndexDisplayLbl";
            this.IndexDisplayLbl.Size = new System.Drawing.Size(64, 18);
            this.IndexDisplayLbl.TabIndex = 41;
            this.IndexDisplayLbl.Text = "0/0";
            this.IndexDisplayLbl.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.IndexDisplayLbl.UseCompatibleTextRendering = true;
            // 
            // DragPnl
            // 
            this.DragPnl.BackColor = System.Drawing.Color.Transparent;
            this.DragPnl.Location = new System.Drawing.Point(19, 2);
            this.DragPnl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DragPnl.Name = "DragPnl";
            this.DragPnl.Size = new System.Drawing.Size(190, 23);
            this.DragPnl.TabIndex = 47;
            // 
            // BuyPhotoBtn
            // 
            this.BuyBtn.BackColor = System.Drawing.Color.Transparent;
            this.BuyBtn.Enabled = false;
            this.BuyBtn.Location = new System.Drawing.Point(145, 235);
            this.BuyBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BuyBtn.Name = "BuyPhotoBtn";
            this.BuyBtn.Size = new System.Drawing.Size(77, 25);
            this.BuyBtn.TabIndex = 46;
            this.BuyBtn.Click += new System.EventHandler(this.BuyPhotoBtn_Click);
            // 
            // NextPhotoBtn
            // 
            this.NextBtn.BackColor = System.Drawing.Color.Transparent;
            this.NextBtn.Enabled = false;
            this.NextBtn.Location = new System.Drawing.Point(175, 202);
            this.NextBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.NextBtn.Name = "NextPhotoBtn";
            this.NextBtn.Size = new System.Drawing.Size(50, 33);
            this.NextBtn.TabIndex = 45;
            this.NextBtn.Click += new System.EventHandler(this.NextPhotoBtn_Click);
            // 
            // PreviousPhotoBtn
            // 
            this.PreviousBtn.BackColor = System.Drawing.Color.Transparent;
            this.PreviousBtn.Enabled = false;
            this.PreviousBtn.Location = new System.Drawing.Point(128, 202);
            this.PreviousBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PreviousBtn.Name = "PreviousPhotoBtn";
            this.PreviousBtn.Size = new System.Drawing.Size(47, 33);
            this.PreviousBtn.TabIndex = 44;
            this.PreviousBtn.Click += new System.EventHandler(this.PreviousPhotoBtn_Click);
            // 
            // SearchBtn
            // 
            this.SearchBtn.BackColor = System.Drawing.Color.Transparent;
            this.SearchBtn.Enabled = false;
            this.SearchBtn.Location = new System.Drawing.Point(77, 202);
            this.SearchBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SearchBtn.Name = "SearchBtn";
            this.SearchBtn.Size = new System.Drawing.Size(51, 33);
            this.SearchBtn.TabIndex = 43;
            this.SearchBtn.Click += new System.EventHandler(this.SearchBtn_Click);
            // 
            // PublishToWebBtn
            // 
            this.PublishBtn.BackColor = System.Drawing.Color.Transparent;
            this.PublishBtn.Enabled = false;
            this.PublishBtn.Location = new System.Drawing.Point(30, 202);
            this.PublishBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PublishBtn.Name = "PublishToWebBtn";
            this.PublishBtn.Size = new System.Drawing.Size(47, 33);
            this.PublishBtn.TabIndex = 42;
            this.PublishBtn.Click += new System.EventHandler(this.PublishToWebBtn_Click);
            // 
            // DescriptionLbl
            // 
            this.DescriptionLbl.BackColor = System.Drawing.Color.Transparent;
            this.DescriptionLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.DescriptionLbl.ForeColor = System.Drawing.Color.White;
            this.DescriptionLbl.Location = new System.Drawing.Point(35, 273);
            this.DescriptionLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DescriptionLbl.Name = "DescriptionLbl";
            this.DescriptionLbl.Size = new System.Drawing.Size(156, 45);
            this.DescriptionLbl.TabIndex = 40;
            this.DescriptionLbl.UseCompatibleTextRendering = true;
            // 
            // PhotoPreviewBx
            // 
            this.PhotoPreviewBx.BackColor = System.Drawing.Color.FromArgb(255, 255, 254);
            this.PhotoPreviewBx.Location = new System.Drawing.Point(34, 27);
            this.PhotoPreviewBx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PhotoPreviewBx.Name = "PhotoPreviewBx";
            this.PhotoPreviewBx.Size = new System.Drawing.Size(190, 136);
            this.PhotoPreviewBx.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PhotoPreviewBx.TabIndex = 39;
            this.PhotoPreviewBx.TabStop = false;
            // 
            // MainFrm
            // 
            this.Font = Program.DefaultFont;
            this.BackgroundImage = Resources.GetImageResource("Background.png");

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 377);
            this.Controls.Add(this.LoginPnl);
            this.Controls.Add(this.StatusLbl);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.HotelExtensionDownBtn);
            this.Controls.Add(this.HotelExtensionUpBtn);
            this.Controls.Add(this.ZoomLbl);
            this.Controls.Add(this.IndexDisplayLbl);
            this.Controls.Add(this.DragPnl);
            this.Controls.Add(this.BuyBtn);
            this.Controls.Add(this.NextBtn);
            this.Controls.Add(this.PreviousBtn);
            this.Controls.Add(this.SearchBtn);
            this.Controls.Add(this.PublishBtn);
            this.Controls.Add(this.DescriptionLbl);
            this.Controls.Add(this.PhotoPreviewBx);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainFrm";
            this.Text = "HabboGallery Desktop";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(255, 255, 254);
            this.BackColor = System.Drawing.Color.FromArgb(255, 255, 254);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFrm_FormClosing);
            this.Load += new System.EventHandler(this.MainFrm_Load);
            this.LoginPnl.ResumeLayout(false);
            this.LoginPnl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PhotoPreviewBx)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.CheckBox RememberMeBx;
        public System.Windows.Forms.Label LoginErrorLbl;
        public System.Windows.Forms.Button LoginSubmitBtn;
        public System.Windows.Forms.Label LoginTitleLbl;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox LoginPasswordTxt;
        public System.Windows.Forms.TextBox LoginEmailTxt;
        public System.Windows.Forms.Panel LoginPnl;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label StatusLbl;
        public System.Windows.Forms.Panel CloseBtn;
        public System.Windows.Forms.Panel HotelExtensionDownBtn;
        public System.Windows.Forms.Panel HotelExtensionUpBtn;
        public System.Windows.Forms.Label ZoomLbl;
        public System.Windows.Forms.Label IndexDisplayLbl;
        public System.Windows.Forms.Panel DragPnl;
        public System.Windows.Forms.Panel BuyBtn;
        public System.Windows.Forms.Panel NextBtn;
        public System.Windows.Forms.Panel PreviousBtn;
        public System.Windows.Forms.Panel SearchBtn;
        public System.Windows.Forms.Panel PublishBtn;
        public System.Windows.Forms.Label DescriptionLbl;
        public System.Windows.Forms.PictureBox PhotoPreviewBx;
        public System.Windows.Forms.Button ExportCertificateBtn;
    }
}