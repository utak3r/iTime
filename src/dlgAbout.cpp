#include <windows.h>
#include "dlgAbout.h"

BOOL CALLBACK dlgAboutProc (HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message) {
        case WM_INITDIALOG:
            ShowWindow (hDlg, SW_SHOW);          
            break;
        case WM_COMMAND:
            switch (LOWORD(wParam)) {
                case IDOK:
                    EndDialog(hDlg, TRUE);
                    break;
                case IDCANCEL:
                    EndDialog(hDlg, FALSE);
                    break;
            }
        break;
        default:
            return FALSE;
    }
    return TRUE;
}

