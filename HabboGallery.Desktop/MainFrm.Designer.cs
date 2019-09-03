namespace HabboGallery
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            this.AutoLoginBx = new System.Windows.Forms.CheckBox();
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
            this.BuyPhotoBtn = new System.Windows.Forms.Panel();
            this.NextPhotoBtn = new System.Windows.Forms.Panel();
            this.PreviousPhotoBtn = new System.Windows.Forms.Panel();
            this.SearchBtn = new System.Windows.Forms.Panel();
            this.PublishToWebBtn = new System.Windows.Forms.Panel();
            this.DescriptionLbl = new System.Windows.Forms.Label();
            this.PhotoPreviewBx = new System.Windows.Forms.PictureBox();
            this.CertLocationDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.LoginPnl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PhotoPreviewBx)).BeginInit();
            this.SuspendLayout();
            // 
            // AutoLoginBx
            // 
            this.AutoLoginBx.AutoSize = true;
            this.AutoLoginBx.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AutoLoginBx.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AutoLoginBx.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.AutoLoginBx.Location = new System.Drawing.Point(132, 96);
            this.AutoLoginBx.Name = "AutoLoginBx";
            this.AutoLoginBx.Size = new System.Drawing.Size(27, 17);
            this.AutoLoginBx.TabIndex = 45;
            this.AutoLoginBx.Text = "º";
            this.AutoLoginBx.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.AutoLoginBx.UseVisualStyleBackColor = true;
            this.AutoLoginBx.CheckedChanged += new System.EventHandler(this.AutoLoginBx_CheckedChanged);
            // 
            // LoginErrorLbl
            // 
            this.LoginErrorLbl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.LoginErrorLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginErrorLbl.ForeColor = System.Drawing.Color.Red;
            this.LoginErrorLbl.Location = new System.Drawing.Point(61, 88);
            this.LoginErrorLbl.Name = "LoginErrorLbl";
            this.LoginErrorLbl.Size = new System.Drawing.Size(60, 26);
            this.LoginErrorLbl.TabIndex = 44;
            this.LoginErrorLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoginErrorLbl.UseCompatibleTextRendering = true;
            // 
            // LoginSubmitBtn
            // 
            this.LoginSubmitBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.LoginSubmitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LoginSubmitBtn.ForeColor = System.Drawing.Color.Black;
            this.LoginSubmitBtn.Location = new System.Drawing.Point(4, 91);
            this.LoginSubmitBtn.Name = "LoginSubmitBtn";
            this.LoginSubmitBtn.Size = new System.Drawing.Size(55, 23);
            this.LoginSubmitBtn.TabIndex = 43;
            this.LoginSubmitBtn.Text = "Log in";
            this.LoginSubmitBtn.UseVisualStyleBackColor = false;
            this.LoginSubmitBtn.Click += new System.EventHandler(this.LoginSubmitBtn_Click);
            // 
            // LoginTitleLbl
            // 
            this.LoginTitleLbl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.LoginTitleLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginTitleLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.LoginTitleLbl.Location = new System.Drawing.Point(94, 3);
            this.LoginTitleLbl.Name = "LoginTitleLbl";
            this.LoginTitleLbl.Size = new System.Drawing.Size(90, 16);
            this.LoginTitleLbl.TabIndex = 42;
            this.LoginTitleLbl.Text = "Create account";
            this.LoginTitleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoginTitleLbl.UseCompatibleTextRendering = true;
            this.LoginTitleLbl.Click += new System.EventHandler(this.LoginTitleLbl_Click);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.label2.Location = new System.Drawing.Point(4, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 16);
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
            this.LoginPasswordTxt.Location = new System.Drawing.Point(4, 64);
            this.LoginPasswordTxt.Name = "LoginPasswordTxt";
            this.LoginPasswordTxt.PasswordChar = '•';
            this.LoginPasswordTxt.Size = new System.Drawing.Size(154, 20);
            this.LoginPasswordTxt.TabIndex = 40;
            // 
            // LoginEmailTxt
            // 
            this.LoginEmailTxt.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.LoginEmailTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LoginEmailTxt.ForeColor = System.Drawing.Color.White;
            this.LoginEmailTxt.Location = new System.Drawing.Point(4, 23);
            this.LoginEmailTxt.Name = "LoginEmailTxt";
            this.LoginEmailTxt.Size = new System.Drawing.Size(154, 20);
            this.LoginEmailTxt.TabIndex = 0;
            // 
            // LoginPnl
            // 
            this.LoginPnl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.LoginPnl.Controls.Add(this.ExportCertificateBtn);
            this.LoginPnl.Controls.Add(this.AutoLoginBx);
            this.LoginPnl.Controls.Add(this.LoginErrorLbl);
            this.LoginPnl.Controls.Add(this.LoginSubmitBtn);
            this.LoginPnl.Controls.Add(this.label2);
            this.LoginPnl.Controls.Add(this.LoginPasswordTxt);
            this.LoginPnl.Controls.Add(this.label1);
            this.LoginPnl.Controls.Add(this.LoginEmailTxt);
            this.LoginPnl.Controls.Add(this.LoginTitleLbl);
            this.LoginPnl.Location = new System.Drawing.Point(29, 23);
            this.LoginPnl.Name = "LoginPnl";
            this.LoginPnl.Size = new System.Drawing.Size(163, 118);
            this.LoginPnl.TabIndex = 53;
            // 
            // ExportCertificateBtn
            // 
            this.ExportCertificateBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.ExportCertificateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExportCertificateBtn.ForeColor = System.Drawing.Color.Black;
            this.ExportCertificateBtn.Location = new System.Drawing.Point(65, 91);
            this.ExportCertificateBtn.Name = "ExportCertificateBtn";
            this.ExportCertificateBtn.Size = new System.Drawing.Size(56, 23);
            this.ExportCertificateBtn.TabIndex = 46;
            this.ExportCertificateBtn.Text = "Cert";
            this.ExportCertificateBtn.UseVisualStyleBackColor = false;
            this.ExportCertificateBtn.Click += new System.EventHandler(this.ExportCertificateBtn_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 16);
            this.label1.TabIndex = 39;
            this.label1.Text = "E-mail address";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.label1.UseCompatibleTextRendering = true;
            // 
            // StatusLbl
            // 
            this.StatusLbl.BackColor = System.Drawing.Color.Transparent;
            this.StatusLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.StatusLbl.Location = new System.Drawing.Point(12, 285);
            this.StatusLbl.Name = "StatusLbl";
            this.StatusLbl.Size = new System.Drawing.Size(197, 16);
            this.StatusLbl.TabIndex = 52;
            this.StatusLbl.Text = "Standby";
            this.StatusLbl.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.StatusLbl.UseCompatibleTextRendering = true;
            // 
            // CloseBtn
            // 
            this.CloseBtn.BackColor = System.Drawing.Color.Transparent;
            this.CloseBtn.Location = new System.Drawing.Point(182, 5);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(15, 15);
            this.CloseBtn.TabIndex = 51;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // HotelExtensionDownBtn
            // 
            this.HotelExtensionDownBtn.BackColor = System.Drawing.Color.Transparent;
            this.HotelExtensionDownBtn.Location = new System.Drawing.Point(170, 263);
            this.HotelExtensionDownBtn.Name = "HotelExtensionDownBtn";
            this.HotelExtensionDownBtn.Size = new System.Drawing.Size(24, 16);
            this.HotelExtensionDownBtn.TabIndex = 50;
            this.HotelExtensionDownBtn.Click += new System.EventHandler(this.HotelExtensionDownBtn_Click);
            // 
            // HotelExtensionUpBtn
            // 
            this.HotelExtensionUpBtn.BackColor = System.Drawing.Color.Transparent;
            this.HotelExtensionUpBtn.Location = new System.Drawing.Point(170, 232);
            this.HotelExtensionUpBtn.Name = "HotelExtensionUpBtn";
            this.HotelExtensionUpBtn.Size = new System.Drawing.Size(24, 16);
            this.HotelExtensionUpBtn.TabIndex = 49;
            this.HotelExtensionUpBtn.Click += new System.EventHandler(this.HotelExtensionUpBtn_Click);
            // 
            // HotelLbl
            // 
            this.ZoomLbl.BackColor = System.Drawing.Color.Transparent;
            this.ZoomLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZoomLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.ZoomLbl.Location = new System.Drawing.Point(174, 251);
            this.ZoomLbl.Name = "HotelLbl";
            this.ZoomLbl.Size = new System.Drawing.Size(35, 15);
            this.ZoomLbl.TabIndex = 48;
            this.ZoomLbl.Text = "2X";
            // 
            // IndexDisplayLbl
            // 
            this.IndexDisplayLbl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.IndexDisplayLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IndexDisplayLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.IndexDisplayLbl.Location = new System.Drawing.Point(80, 4);
            this.IndexDisplayLbl.Name = "IndexDisplayLbl";
            this.IndexDisplayLbl.Size = new System.Drawing.Size(55, 16);
            this.IndexDisplayLbl.TabIndex = 41;
            this.IndexDisplayLbl.Text = "0/0";
            this.IndexDisplayLbl.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.IndexDisplayLbl.UseCompatibleTextRendering = true;
            // 
            // DragPnl
            // 
            this.DragPnl.BackColor = System.Drawing.Color.Transparent;
            this.DragPnl.Location = new System.Drawing.Point(16, 2);
            this.DragPnl.Name = "DragPnl";
            this.DragPnl.Size = new System.Drawing.Size(163, 20);
            this.DragPnl.TabIndex = 47;
            // 
            // BuyPhotoBtn
            // 
            this.BuyPhotoBtn.BackColor = System.Drawing.Color.Transparent;
            this.BuyPhotoBtn.BackgroundImage = global::HabboGallery.Properties.Resources.DisabledBuyButton;
            this.BuyPhotoBtn.Enabled = false;
            this.BuyPhotoBtn.Location = new System.Drawing.Point(124, 204);
            this.BuyPhotoBtn.Name = "BuyPhotoBtn";
            this.BuyPhotoBtn.Size = new System.Drawing.Size(66, 22);
            this.BuyPhotoBtn.TabIndex = 46;
            this.BuyPhotoBtn.Click += new System.EventHandler(this.BuyPhotoBtn_Click);
            // 
            // NextPhotoBtn
            // 
            this.NextPhotoBtn.BackColor = System.Drawing.Color.Transparent;
            this.NextPhotoBtn.BackgroundImage = global::HabboGallery.Properties.Resources.DisabledForwardButton;
            this.NextPhotoBtn.Enabled = false;
            this.NextPhotoBtn.Location = new System.Drawing.Point(150, 175);
            this.NextPhotoBtn.Name = "NextPhotoBtn";
            this.NextPhotoBtn.Size = new System.Drawing.Size(43, 29);
            this.NextPhotoBtn.TabIndex = 45;
            this.NextPhotoBtn.Click += new System.EventHandler(this.NextPhotoBtn_Click);
            // 
            // PreviousPhotoBtn
            // 
            this.PreviousPhotoBtn.BackColor = System.Drawing.Color.Transparent;
            this.PreviousPhotoBtn.BackgroundImage = global::HabboGallery.Properties.Resources.DisabledPreviousButton;
            this.PreviousPhotoBtn.Enabled = false;
            this.PreviousPhotoBtn.Location = new System.Drawing.Point(110, 175);
            this.PreviousPhotoBtn.Name = "PreviousPhotoBtn";
            this.PreviousPhotoBtn.Size = new System.Drawing.Size(40, 29);
            this.PreviousPhotoBtn.TabIndex = 44;
            this.PreviousPhotoBtn.Click += new System.EventHandler(this.PreviousPhotoBtn_Click);
            // 
            // SearchBtn
            // 
            this.SearchBtn.BackColor = System.Drawing.Color.Transparent;
            this.SearchBtn.BackgroundImage = global::HabboGallery.Properties.Resources.DisabledSearchButton;
            this.SearchBtn.Enabled = false;
            this.SearchBtn.Location = new System.Drawing.Point(66, 175);
            this.SearchBtn.Name = "SearchBtn";
            this.SearchBtn.Size = new System.Drawing.Size(44, 29);
            this.SearchBtn.TabIndex = 43;
            this.SearchBtn.Click += new System.EventHandler(this.SearchBtn_Click);
            // 
            // PublishToWebBtn
            // 
            this.PublishToWebBtn.BackColor = System.Drawing.Color.Transparent;
            this.PublishToWebBtn.BackgroundImage = global::HabboGallery.Properties.Resources.DisabledPublishButton;
            this.PublishToWebBtn.Enabled = false;
            this.PublishToWebBtn.Location = new System.Drawing.Point(26, 175);
            this.PublishToWebBtn.Name = "PublishToWebBtn";
            this.PublishToWebBtn.Size = new System.Drawing.Size(40, 29);
            this.PublishToWebBtn.TabIndex = 42;
            this.PublishToWebBtn.Click += new System.EventHandler(this.PublishToWebBtn_Click);
            // 
            // DescriptionLbl
            // 
            this.DescriptionLbl.BackColor = System.Drawing.Color.Transparent;
            this.DescriptionLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DescriptionLbl.ForeColor = System.Drawing.Color.White;
            this.DescriptionLbl.Location = new System.Drawing.Point(30, 237);
            this.DescriptionLbl.Name = "DescriptionLbl";
            this.DescriptionLbl.Size = new System.Drawing.Size(134, 39);
            this.DescriptionLbl.TabIndex = 40;
            this.DescriptionLbl.UseCompatibleTextRendering = true;
            // 
            // PhotoPreviewBx
            // 
            this.PhotoPreviewBx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(254)))));
            this.PhotoPreviewBx.BackgroundImage = global::HabboGallery.Properties.Resources.Placeholder;
            this.PhotoPreviewBx.Location = new System.Drawing.Point(29, 23);
            this.PhotoPreviewBx.Name = "PhotoPreviewBx";
            this.PhotoPreviewBx.Size = new System.Drawing.Size(163, 118);
            this.PhotoPreviewBx.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PhotoPreviewBx.TabIndex = 39;
            this.PhotoPreviewBx.TabStop = false;
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(254)))));
            this.BackgroundImage = global::HabboGallery.Properties.Resources.ApplicationBackground;
            this.ClientSize = new System.Drawing.Size(220, 327);
            this.Controls.Add(this.LoginPnl);
            this.Controls.Add(this.StatusLbl);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.HotelExtensionDownBtn);
            this.Controls.Add(this.HotelExtensionUpBtn);
            this.Controls.Add(this.ZoomLbl);
            this.Controls.Add(this.IndexDisplayLbl);
            this.Controls.Add(this.DragPnl);
            this.Controls.Add(this.BuyPhotoBtn);
            this.Controls.Add(this.NextPhotoBtn);
            this.Controls.Add(this.PreviousPhotoBtn);
            this.Controls.Add(this.SearchBtn);
            this.Controls.Add(this.PublishToWebBtn);
            this.Controls.Add(this.DescriptionLbl);
            this.Controls.Add(this.PhotoPreviewBx);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainFrm";
            this.Text = "HabboGallery Desktop";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(254)))));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFrm_FormClosing);
            this.Load += new System.EventHandler(this.MainFrm_Load);
            this.LoginPnl.ResumeLayout(false);
            this.LoginPnl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PhotoPreviewBx)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.CheckBox AutoLoginBx;
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
        public System.Windows.Forms.Panel BuyPhotoBtn;
        public System.Windows.Forms.Panel NextPhotoBtn;
        public System.Windows.Forms.Panel PreviousPhotoBtn;
        public System.Windows.Forms.Panel SearchBtn;
        public System.Windows.Forms.Panel PublishToWebBtn;
        public System.Windows.Forms.Label DescriptionLbl;
        public System.Windows.Forms.PictureBox PhotoPreviewBx;
        public System.Windows.Forms.Button ExportCertificateBtn;
        private System.Windows.Forms.FolderBrowserDialog CertLocationDlg;
    }
}