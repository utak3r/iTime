using System.Windows.Forms;

namespace iTime;

class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());
        SetupNotifyIcon();
        Application.Run();
    }

    static void SetupNotifyIcon()
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        notifyIcon.Icon = new Icon("satelitte.ico");
        ContextMenuStrip notifyContextMenu = new ContextMenuStrip();

        // Exit
        ToolStripMenuItem menuItemExit = new ToolStripMenuItem();
        menuItemExit.Text = "E&xit";
        menuItemExit.Click += new EventHandler((sender, e) =>
        {
            notifyIcon.Visible = false;
            Application.Exit();
        });

        // About
        ToolStripMenuItem menuItemAbout = new ToolStripMenuItem();
        menuItemAbout.Text = "Ab&out";
        menuItemAbout.Click += new EventHandler((sender, e) =>
        {
            About aboutForm = new About();
            aboutForm.ShowDialog();
        });

        // SNTP time
        ToolStripMenuItem menuItemSNTP = new ToolStripMenuItem();
        menuItemSNTP.Text = "SN&TP";
        menuItemSNTP.Click += new EventHandler((sender, e) =>
        {
            SNTP sntpForm = new SNTP();
            sntpForm.ShowDialog();
        });

        notifyContextMenu.Items.AddRange(new ToolStripMenuItem[] { menuItemSNTP, menuItemAbout, menuItemExit });

        notifyIcon.ContextMenuStrip = notifyContextMenu;
        notifyIcon.Visible = true;
    }

}
