#ifndef SNTP_H
#define SNTP_H

#include <windows.h>
#include <winsock.h>
#include <sys/types.h>
#include <sys/time.h>
#include <sys/timeb.h>
#include <math.h>
#include <stdio.h>


#define COUNT_MAX            25        /* NIE zwiêkszaæ! */
#define JAN_1970   2208988800.0        /* 1970 - 1900 w sekundach */
#define NTP_SCALE  4294967296.0        /* 2^32, rzecz jasna ;) */

#define NTP_PACKET_MIN       48        /* Bez uwierzytelniania */
#define NTP_PACKET_MAX       68        /* Z uwierzytelnianiem (ignorowane) */
#define NTP_DISP_FIELD        8        /* Offset pola dyspersji */
#define NTP_REFERENCE        16        /* Offset timestampa referencyjnego */
#define NTP_ORIGINATE        24        /* Offset timestampa oryginalnego */
#define NTP_RECEIVE          32        /* Offset timestampa otrzymanego */
#define NTP_TRANSMIT         40        /* Offset timestampa transmitowanego */

#define NTP_LI_FUDGE          0        /* The current 'status' */
#define NTP_VERSION           3        /* The current version */
#define NTP_VERSION_MAX       4        /* The maximum valid version */
#define NTP_STRATUM          15        /* The current stratum as a server */
#define NTP_STRATUM_MAX      15        /* The maximum valid stratum */
#define NTP_POLLING           8        /* The current 'polling interval' */
#define NTP_PRECISION         0        /* The current 'precision' - 1 sec. */

#define NTP_ACTIVE            1        /* NTP symmetric active request */
#define NTP_PASSIVE           2        /* NTP symmetric passive response */
#define NTP_CLIENT            3        /* NTP client request */
#define NTP_SERVER            4        /* NTP server response */
#define NTP_BROADCAST         5        /* NTP server broadcast */

#define NTP_INSANITY     3600.0        /* Errors beyond this are hopeless */
#define RESET_MIN            15        /* Minimum period between resets */
#define ABSCISSA            3.0        /* Scale factor for standard errors */
#define DELAY               300        /* Default time-out */ 

#define NETWORK_ERROR        -1
#define NETWORK_OK            0
#define SNTP_ERROR           -2
#define SNTP_OK               0

/* the unpacked NTP data structure */
typedef struct NTP_DATA {
    unsigned char status, version, mode, stratum, polling, precision;
    double dispersion, reference, originate, receive, transmit, current;
} ntp_data;

typedef struct SNTP_TIME {
    struct tm time;
    double offset, error, dispersion;
    int usec;
} sntp_time;

int GetSNTPDateTime(const char *ntpserver, int port, sntp_time &recv_time);

#endif // SNTP_H

