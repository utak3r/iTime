#ifndef SETTINGS_H
#define SETTINGS_H

#include <windows.h>
#include <winreg.h>

extern HKEY hkRegCurrKey;
extern HKEY hkRegRoot;
/*
HKEY_CLASSES_ROOT
HKEY_CURRENT_USER
HKEY_LOCAL_MACHINE
HKEY_USERS
*/

void SetRegRootKey(HKEY rootkey);
int OpenRegKey(const char *key);
void CloseRegKey();
char *ReadRegString(const char *value);
int ReadRegInteger(const char *value);

#endif // SETTINGS_H

