using System.Windows.Forms;

namespace iTime;

public class Settings
{
    public static String AppName = "iTime";
    public static String AppVersion = "3.0.1";
    public static String Copyright = "(c) 2001 - 2023 Piotr Borys";
    public static String IconFilename = "iTime_icon.ico";
    public static String NTPServerName = "ntp0.fau.de";
}

class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        SetupNotifyIcon();
        Application.Run();
    }

    static void SetupNotifyIcon()
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        notifyIcon.Icon = new Icon(Settings.IconFilename);
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
