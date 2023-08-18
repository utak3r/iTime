#include "main.h"
#include "settings.h"
#include "dlgMain.h"
#include "dlgAbout.h"

LRESULT CALLBACK WindowProcedure (HWND, UINT, WPARAM, LPARAM);

char szClassName[ ] = "iTimeApp";
HICON hiMainIcon;
HMENU hmTrayMenu;
HWND hwnd;
HINSTANCE hAppInstance;

char *pszdlgMainNapis;

//
// Utworzenie g³ównego okna aplikacji/
// Uwaga: je¿eli korzystasz z systray'owej
// wersji aplikacji, okno to nigdy nie
// bêdzie wyœwietlone! S³u¿y w tym wypadku
// tylko do obs³ugi kolejki komunikatów
// na potrzeby ikonki w systray'u.
//
int CreateMainWnd ()
{
    WNDCLASSEX wincl;

    wincl.hInstance = hAppInstance;
    wincl.lpszClassName = szClassName;
    wincl.lpfnWndProc = WindowProcedure;
    wincl.style = CS_DBLCLKS;
    wincl.cbSize = sizeof (WNDCLASSEX);

    wincl.hIcon = LoadIcon (NULL, MAKEINTRESOURCE(IDI_MAINICON));
    wincl.hIconSm = LoadIcon (NULL, IDI_APPLICATION);
    wincl.hCursor = LoadCursor (NULL, IDC_ARROW);
    wincl.lpszMenuName = NULL;
    wincl.cbClsExtra = 0;
    wincl.cbWndExtra = 0;
    wincl.hbrBackground = (HBRUSH) COLOR_BACKGROUND;

    if (!RegisterClassEx (&wincl))
        return 0;

    hwnd = CreateWindowEx (
           0,                   /* Extended possibilites for variation */
           szClassName,         /* Classname */
           "utak3r App",       /* Title Text */
           WS_OVERLAPPEDWINDOW, /* default window */
           CW_USEDEFAULT,       /* Windows decides the position */
           CW_USEDEFAULT,       /* where the window ends up on the screen */
           544,                 /* The programs width */
           375,                 /* and height in pixels */
           HWND_DESKTOP,        /* The window is a child-window to desktop */
           NULL,                /* No menu */
           hAppInstance,        /* Program Instance handler */
           NULL                 /* No Window Creation data */
           );

    return 1;
}

//
// Wstawienie ikonki do systray'a
//
int CreateTrayIcon()
{
    NOTIFYICONDATA tnid;
    char lpszTip[16] = "iTime\0";

    tnid.cbSize = sizeof(NOTIFYICONDATA);
    tnid.hWnd = hwnd;
    tnid.uID = IDI_MAINICON;
    tnid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
    tnid.uCallbackMessage = WM_TRAYNOTIFY;
    tnid.hIcon = hiMainIcon;
    lstrcpyn(tnid.szTip, lpszTip, sizeof(tnid.szTip));
    Shell_NotifyIcon(NIM_ADD, &tnid);
    return 1;
}

//
// Usuniêcie ikonki z systray'a
//
int DestroyTrayIcon()
{
    NOTIFYICONDATA tnid;
    tnid.cbSize = sizeof(NOTIFYICONDATA);
    tnid.hWnd = hwnd;
    tnid.uID = IDI_MAINICON;
    Shell_NotifyIcon(NIM_DELETE, &tnid);
    return 1;
}

//
// Obs³uga klikniêcia myszk¹ na ikonce w systray'u
//
void RespondTrayIcon(LPARAM lParam)
{
    HMENU submenu;

    if (lParam == WM_RBUTTONUP) {
        submenu = GetSubMenu(hmTrayMenu, 0);
        if (submenu == NULL)
            return;
        SetMenuDefaultItem(submenu, IDM_EXIT, FALSE);
        POINT mouse;
        GetCursorPos(&mouse);
        SetForegroundWindow(hwnd);
        TrackPopupMenu(submenu, 0, mouse.x, mouse.y, 0, hwnd, NULL);
    }
    if (lParam == WM_LBUTTONDBLCLK) {              
        //DialogBox(hAppInstance, MAKEINTRESOURCE(IDD_DLGMAIN), hwnd, dlgMainProc);
        dlgMainCreate();
        return;
    }
}

int ReadSettings()
{
    /*
    pszdlgMainNapis = new char [1024];
    SetRegRootKey(HKEY_LOCAL_MACHINE);
    if (!OpenRegKey("SOFTWARE\\utak3r\\FileCipher"))
        return 0;
    strcpy (pszdlgMainNapis, ReadRegString("Napis"));
    */
    return 1;
}

//
// Inicjalizacja aplikacji
//
int InitApp()
{
    if (CreateMainWnd() == 0)
        return 0;
    if ((hiMainIcon = LoadIcon (hAppInstance, MAKEINTRESOURCE(IDI_MAINICON))) == NULL)
        return 0;
    if ((hmTrayMenu = LoadMenu (hAppInstance, MAKEINTRESOURCE(IDM_TRAYMENU))) == NULL)
        return 0;
    if (!ReadSettings())
        return 0;
        
        
    return 1;
}

//
// Porz¹dki przy zamykaniu aplikacji
//
void ShutdownApp()
{
    delete [] pszdlgMainNapis;
    DestroyTrayIcon();
    DestroyMenu(hmTrayMenu);
    DestroyIcon(hiMainIcon);
}

//
// Pêtla obs³ugi komunikatów
//
LRESULT CALLBACK WindowProcedure (HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message) {
        case WM_DESTROY:
            PostQuitMessage(WM_CLOSE);
            return 0;
        case WM_CLOSE:
            DestroyWindow(hwnd);
        case WM_TRAYNOTIFY:
            RespondTrayIcon(lParam);
            return 0;
        case WM_COMMAND:
            switch (wParam) {
                case IDM_EXIT:
                    SendMessage(hwnd, WM_CLOSE, 0, 0L);
                    return 0;
                case IDM_ABOUT:
                    DialogBox(hAppInstance, MAKEINTRESOURCE(DLG_ABOUT), hwnd, (DLGPROC)dlgAboutProc);
                    return 0;
                case IDM_MAINDLG:
                    //DialogBox(hAppInstance, MAKEINTRESOURCE(IDD_DLGMAIN), hwnd, (DLGPROC)dlgMainProc);
                    dlgMainCreate();
                    return 0;
                }
            break;
        default:                  
            return DefWindowProc (hwnd, message, wParam, lParam);
    }
    return 0;
}

//
// WinMain
//
int WINAPI WinMain (HINSTANCE hThisInstance,
                    HINSTANCE hPrevInstance,
                    LPSTR lpszArgument,
                    int nFunsterStil)

{
    MSG messages;

    hAppInstance = hThisInstance;
    if (InitApp() == 0)
        return 0;
    
    //ShowWindow (hwnd, nFunsterStil);
    //SetMenu (hwnd, hmTrayMenu);
    CreateTrayIcon();

    while (GetMessage (&messages, NULL, 0, 0)) {
        TranslateMessage(&messages);
        DispatchMessage(&messages);
    }
    ShutdownApp();
    return messages.wParam;
}

