using System.Windows.Forms;

namespace iTime;

public class SNTP : Form
{
    private System.Windows.Forms.Label lblSNTPTime;
    private System.Windows.Forms.Button btnOk;
    private SNTPClient sntpClient = null;

    public SNTP()
    {
        this.lblSNTPTime = new System.Windows.Forms.Label();
        this.btnOk = new System.Windows.Forms.Button();
        this.SuspendLayout();

        this.lblSNTPTime.AutoSize =  true;
        this.lblSNTPTime.Text =  "iTime 3.0.1";
        this.lblSNTPTime.Location = new System.Drawing.Point(84,24);
        this.lblSNTPTime.Size = new System.Drawing.Size(63,15);
        this.lblSNTPTime.TabIndex = 1;

        this.btnOk.Text =  "&Ok";
        this.btnOk.Location = new System.Drawing.Point(76,84);
        this.btnOk.TabIndex = 2;
        this.btnOk.Click += new System.EventHandler(ButtonOkClicked);

        this.Controls.AddRange(new Control[]{lblSNTPTime, btnOk});
        this.ResumeLayout(false);

        sntpClient = new SNTPClient();
    }

    public void ButtonOkClicked(object sender, EventArgs e)
    {
        Close();
    }


}
