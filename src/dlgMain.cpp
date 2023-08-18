#include "dlgMain.h"
#include "sntp.h"

// vega.cbk.poznan.pl
// ntp.certum.pl
// swisstime.ethz.ch
// ntp0.fau.de
// ntps1-0.cs.tu-berlin.de
// tempo.cstv.to.cnr.it
// ntp1.nl.net

sntp_time servertime;
char sntpserver[] = "ntp0.fau.de";
int sntpport = 123;
double offset;
HWND hDlgWnd;
void GetServerTime(HWND hDlg);
void CorrectSystemTime(double offset);
time_t ConvertTime(double value, int *millisecs);
DWORD WINAPI SNTPThread(LPVOID arg);

BOOL CALLBACK dlgMainProc (HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    hDlgWnd = hDlg;
    switch (message) {
        case WM_INITDIALOG:
            GetServerTime(hDlg);
            ShowWindow (hDlg, SW_SHOW);          
            break;
        case WM_COMMAND:
            switch (LOWORD(wParam)) {
                case IDB_MAIN_CLOSE:
                    EndDialog(hDlg, TRUE);
                    break;
                case IDB_MAIN_SETTIME:
                    CorrectSystemTime(offset);
                    break;
            }
        break;
        default:
            return FALSE;
    }
    return TRUE;
}

void dlgMainCreate()
{
    DialogBox(hAppInstance, MAKEINTRESOURCE(IDD_DLGMAIN), hwnd, (DLGPROC)dlgMainProc);
}

void GetServerTime(HWND hDlg)
{
    DWORD SNTPThreadId;
    HANDLE hSNTPThread;

    SetDlgItemText(hDlgWnd, IDT_MAIN_DATE, "working ...");
    SetDlgItemText(hDlgWnd, IDT_MAIN_OFFSET, "working ...");
    SetDlgItemText(hDlgWnd, IDT_MAIN_ERROR, "working ...");

    hSNTPThread = CreateThread(0, 0, SNTPThread, 0, 0, &SNTPThreadId);
}


DWORD WINAPI SNTPThread(LPVOID arg)
{
    struct tm *data;
    char napis[100];
    static const char *months[] = {
        "Sty", "Lut", "Mar", "Kwi", "Maj", "Cze",
        "Lip", "Sie", "Wrz", "PaŸ", "Lis", "Gru"
    };

    if (GetSNTPDateTime(sntpserver, sntpport, servertime) != NETWORK_OK) {
        SetDlgItemText(hDlgWnd, IDT_MAIN_DATE, "B³¹d sieci");
        SetDlgItemText(hDlgWnd, IDT_MAIN_OFFSET, "B³¹d sieci");
        SetDlgItemText(hDlgWnd, IDT_MAIN_ERROR, "B³¹d sieci");
        ExitThread(1);
    }

    data = &servertime.time;
    sprintf(napis, "%.2d %s %.4d %.2d:%.2d:%.2d.%.3d", 
            data->tm_mday, months[data->tm_mon], data->tm_year+1900,
            data->tm_hour, data->tm_min, data->tm_sec, servertime.usec);
    SetDlgItemText(hDlgWnd, IDT_MAIN_DATE, napis);
    sprintf(napis, "%c %.3f", (servertime.offset > 0.0 ? '-' : '+'), 
            (servertime.offset > 0.0 ? servertime.offset : -servertime.offset));
    SetDlgItemText(hDlgWnd, IDT_MAIN_OFFSET, napis);
    sprintf(napis, "+/- %.3f secs", servertime.dispersion + servertime.error);
    SetDlgItemText(hDlgWnd, IDT_MAIN_ERROR, napis);
    offset = servertime.offset;

    ExitThread(0);
}


time_t ConvertTime(double value, int *millisecs)
{
    time_t result = (time_t)value;
    if ((*millisecs = (int)(1000.0*(value-result))) >= 1000) {
        *millisecs = 0;
        ++result;
    }
    return result;
} 

void CorrectSystemTime(double offset)
{
    double currmilli;
    int milli;
    struct tm *gmt;
    _timeb currtime;
    _ftime(&currtime);
    currmilli = offset + currtime.time + 1.0e-6 * currtime.millitm;
    time_t now = ConvertTime(currmilli, &milli);
    gmt = localtime(&now);

    SYSTEMTIME SystemTime;
    SystemTime.wYear = gmt->tm_year+1900;
    SystemTime.wMonth = gmt->tm_mon+1;
    SystemTime.wDay = gmt->tm_mday;
    SystemTime.wDayOfWeek = gmt->tm_wday;
    SystemTime.wHour = gmt->tm_hour;
    SystemTime.wMinute = gmt->tm_min;
    SystemTime.wSecond = gmt->tm_sec;
    SystemTime.wMilliseconds = milli; 
    SetLocalTime(&SystemTime); 
}

