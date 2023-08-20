using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace iTime;

public class SNTP : Form
{
        private System.Windows.Forms.Label? lblDateTimeLabel;
        private System.Windows.Forms.Label? lblDateTimeValueDate;
        private System.Windows.Forms.Label? lblDateTimeValueTime;
        private System.Windows.Forms.Label? lblShiftLabel;
        private System.Windows.Forms.Label? lblShiftValue;
        private System.Windows.Forms.Button? btnClose;
        private System.Windows.Forms.Button? btnSetTime;
        
        private SNTPClient? sntpClient = null;
        private TimeSpan sntpToLocalShift = new TimeSpan();

    public SNTP()
    {
        InitializeComponent();
        GetTimeFromSNTP();
    }

    private void InitializeComponent()
    {
        this.lblDateTimeLabel = new System.Windows.Forms.Label();
        this.lblDateTimeValueDate = new System.Windows.Forms.Label();
        this.lblDateTimeValueTime = new System.Windows.Forms.Label();
        this.lblShiftLabel = new System.Windows.Forms.Label();
        this.lblShiftValue = new System.Windows.Forms.Label();
        this.btnClose = new System.Windows.Forms.Button();
        this.btnSetTime = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        this.MaximizeBox =  false;
        this.MinimizeBox =  false;
        this.Size = new System.Drawing.Size(436,180);
        this.Text =  "Pobieranie czasu z serwera NTP";
        this.Icon = new Icon("satelitte.ico");
        //
        this.btnClose.Text =  "Zamknij okno";
        this.btnClose.Location = new System.Drawing.Point(276,24);
        this.btnClose.Size = new System.Drawing.Size(116,23);
        this.btnClose.TabIndex = 0;
        this.btnClose.Click += (sender, e) => { Close(); };
        //
        this.btnSetTime.Text =  "Ustaw czas";
        this.btnSetTime.Location = new System.Drawing.Point(276,60);
        this.btnSetTime.Size = new System.Drawing.Size(116,23);
        this.btnSetTime.TabIndex = 1;
        this.btnSetTime.Click += (sender, e) => { FixSystemTime(); };
        //
        this.lblDateTimeLabel.AutoSize =  true;
        this.lblDateTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblDateTimeLabel.Text =  "Data i godzina:";
        this.lblDateTimeLabel.Location = new System.Drawing.Point(20,24);
        this.lblDateTimeLabel.Size = new System.Drawing.Size(85,15);
        //
        this.lblDateTimeValueDate.AutoSize =  true;
        this.lblDateTimeValueDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblDateTimeValueDate.Text =  "28 październik 2023";
        this.lblDateTimeValueDate.Location = new System.Drawing.Point(124,24);
        this.lblDateTimeValueDate.Size = new System.Drawing.Size(108,15);
        //
        this.lblDateTimeValueTime.AutoSize =  true;
        this.lblDateTimeValueTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblDateTimeValueTime.Text =  "23:58:09.458";
        this.lblDateTimeValueTime.Location = new System.Drawing.Point(124,48);
        this.lblDateTimeValueTime.Size = new System.Drawing.Size(70,15);
        //
        this.lblShiftLabel.AutoSize =  true;
        this.lblShiftLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblShiftLabel.Text =  "Przesunięcie:";
        this.lblShiftLabel.Location = new System.Drawing.Point(20,80);
        this.lblShiftLabel.Size = new System.Drawing.Size(75,15);
        //
        this.lblShiftValue.AutoSize =  true;
        this.lblShiftValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblShiftValue.Text =  "0";
        this.lblShiftValue.Location = new System.Drawing.Point(120,80);
        this.lblShiftValue.Size = new System.Drawing.Size(51,15);

        this.Controls.AddRange(new Control[]
            {
            btnClose, btnSetTime, 
            lblDateTimeLabel, lblDateTimeValueDate, lblDateTimeValueTime, 
            lblShiftLabel, lblShiftValue
            });
        this.ResumeLayout(false);
    }

    private void SetDateTimeLabels(DateTime dateTime)
    {
        if (this.lblDateTimeValueDate != null && this.lblDateTimeValueTime != null)
        {
            this.lblDateTimeValueDate.Text = dateTime.ToString("dddd, dd MMMM yyyy");
            this.lblDateTimeValueTime.Text = dateTime.ToString("HH:mm:ss.fff");
        }
    }

    private void SetShiftLabel(TimeSpan shift)
    {
        if (this.lblShiftValue != null)
        {
            this.lblShiftValue.Text = Math.Floor(shift.TotalSeconds) + "." + Math.Abs(shift.Milliseconds) + " s.";
        }
    }

    private void GetTimeFromSNTP()
    {
        DateTime localDateTime = DateTime.Now;
        SetDateTimeLabels(localDateTime);

        sntpClient = new SNTPClient();
        sntpClient.Connect("ntp0.fau.de", 5000);
        sntpToLocalShift = sntpClient.LocalClockOffset;

        SetDateTimeLabels(sntpClient.ReceiveTimestamp);
        SetShiftLabel(sntpToLocalShift);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME 
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLocalTime", SetLastError = true)]
    public extern static bool SetLocalTime(ref SYSTEMTIME sysDate);

    public void FixSystemTime()
    {
        DateTime localDateTime = DateTime.Now;
        DateTime correctedTime = localDateTime + sntpToLocalShift;

        SYSTEMTIME st = new SYSTEMTIME();
        st.wYear = Convert.ToInt16(correctedTime.Year);
        st.wMonth = Convert.ToInt16(correctedTime.Month);
        st.wDay = Convert.ToInt16(correctedTime.Day);
        st.wHour = Convert.ToInt16(correctedTime.Hour);
        st.wMinute = Convert.ToInt16(correctedTime.Minute);
        st.wSecond = Convert.ToInt16(correctedTime.Second);
        st.wMilliseconds = Convert.ToInt16(correctedTime.Millisecond);
        SetLocalTime(ref st);
    }


}
