namespace iTime
{
    partial class About
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
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // Label1
            //
            this.Label1.AutoSize =  true;
            this.Label1.Text =  "iTime 3.0.1";
            this.Label1.Location = new System.Drawing.Point(84,24);
            this.Label1.Size = new System.Drawing.Size(63,15);
            this.Label1.TabIndex = 1;
            //
            // Label1
            //
            this.Label2.AutoSize =  true;
            this.Label2.Text =  "(c) 2001 - 2023 Piotr Borys";
            this.Label2.Location = new System.Drawing.Point(44,48);
            this.Label2.Size = new System.Drawing.Size(143,15);
            this.Label2.TabIndex = 1;
            //
            // btnOk
            //
            this.btnOk.Text =  "&Ok";
            this.btnOk.Location = new System.Drawing.Point(76,84);
            this.btnOk.TabIndex = 2;
            this.btnOk.Click += new System.EventHandler(this.ButtonOkClicked);
         //
         // form
         //
            this.Size = new System.Drawing.Size(256,168);
            this.Text =  "About";
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.btnOk);
            this.ResumeLayout(false);
        } 

        #endregion 

        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Button btnOk;
    }
}

