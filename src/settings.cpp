
#include "settings.h"

HKEY hkRegCurrKey;
HKEY hkRegRoot;


void SetRegRootKey(HKEY rootkey)
{
    hkRegRoot = rootkey;
}

int OpenRegKey(const char *key)
{
    if (RegOpenKeyEx(hkRegRoot, key, 0, KEY_ALL_ACCESS, &hkRegCurrKey) != ERROR_SUCCESS)
        return 0;
    else
        return 1;
}

void CloseRegKey()
{
    RegCloseKey(hkRegCurrKey);
}

char *ReadRegString(const char *value)
{
    DWORD stringsize = 1024, typ = REG_SZ;
    char string[stringsize];
    if (RegQueryValueEx(hkRegCurrKey, value, NULL, &typ, (LPBYTE)string, &stringsize) != ERROR_SUCCESS)
        strcpy(string, "");
    //MessageBox(NULL, string, "debug", MB_OK);
    return string;
}

int ReadRegInteger(const char *value)
{
    DWORD intsize = sizeof(int), typ = REG_DWORD;
    int liczba;
    RegQueryValueEx(hkRegCurrKey, value, NULL, &typ, (LPBYTE)&liczba, &intsize);
    return liczba;
}

