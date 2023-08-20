namespace iTime;

public class About : Form
{
    private System.Windows.Forms.PictureBox? IconBox;
    private System.Windows.Forms.Label? lblTitle;
    private System.Windows.Forms.Label? lblCopyright;
    private System.Windows.Forms.Button? btnOk;

    public About()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.IconBox = new System.Windows.Forms.PictureBox();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblCopyright = new System.Windows.Forms.Label();
        this.btnOk = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
        this.SuspendLayout();

        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox =  false;
        this.MinimizeBox =  false;
        this.Size = new System.Drawing.Size(424,220);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Icon = new Icon(Settings.IconFilename);
        this.Text =  "About";
        //
        this.IconBox.Text =  "PictureBox0";
        this.IconBox.Location = new System.Drawing.Point(28,28);
        this.IconBox.Size = new System.Drawing.Size(128,128);
        this.IconBox.SizeMode = PictureBoxSizeMode.StretchImage;
        Icon logo = new Icon(Settings.IconFilename, 128, 128);
        this.IconBox.Image = (Image)logo.ToBitmap();
        //
        this.lblTitle.AutoSize =  true;
        this.lblTitle.Text =  Settings.AppName + " " + Settings.AppVersion;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.Location = new System.Drawing.Point(216,40);
        this.lblTitle.Size = new System.Drawing.Size(127,30);
        this.lblTitle.TabIndex = 1;
        //
        this.lblCopyright.AutoSize =  true;
        this.lblCopyright.Text =  Settings.Copyright;
        this.lblCopyright.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblCopyright.Location = new System.Drawing.Point(176,76);
        this.lblCopyright.Size = new System.Drawing.Size(197,21);
        this.lblCopyright.TabIndex = 2;
        //
        this.btnOk.Text =  "Ok";
        this.btnOk.Location = new System.Drawing.Point(204,108);
        this.btnOk.Size = new System.Drawing.Size(144,23);
        this.btnOk.TabIndex = 3;
        this.btnOk.Click += (sender, e) => { Close(); };
        //
        this.Controls.AddRange(new Control[]
        {
            this.btnOk, this.IconBox, this.lblTitle, this.lblCopyright
        });
        ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
        this.ResumeLayout(false);
    } 

    public void ButtonOkClicked(object sender, EventArgs e)
    {
        Close();
    }
}
