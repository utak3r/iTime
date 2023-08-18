#include "sntp.h"

int count, attempts, delay;
double dispersion;
double outgoing[2*COUNT_MAX];
double minerr;

void ReportError(int, const char *);
void display_data (ntp_data *data);
void display_packet (unsigned char *packet, int length);
void pack_ntp (unsigned char *packet, int length, ntp_data *data);
void unpack_ntp (ntp_data *data, unsigned char *packet, int length);
void make_packet (ntp_data *data, int mode);
int read_packet (SOCKET isocket, ntp_data *data, double *off, double *err);
int receive_sntp (SOCKET isocket, sntp_time *recv_time);

int GetSNTPDateTime(const char *ntpserver, int port, sntp_time &recv_time)
{
    WORD sockVersion;
	WSADATA wsaData;
	int nret;

	sockVersion = MAKEWORD(1, 1);
	WSAStartup(sockVersion, &wsaData);

	LPHOSTENT hostEntry;
/*
IP address version
    in_addr iaHost;
    iaHost.s_addr = inet_addr("150.254.183.15");
    hostEntry = gethostbyaddr((const char *)&iaHost, sizeof(struct in_addr), AF_INET);
*/	
	hostEntry = gethostbyname(ntpserver); /* vega.cbk.poznan.pl */
	if (!hostEntry) {
		nret = WSAGetLastError();
		ReportError(nret, "gethostbyname()");
		WSACleanup();
		return NETWORK_ERROR;
	}

	// Utworzenie gniazda bezpo³¹czeniowego 
	// w protokole UDP/IP
	SOCKET theSocket;
	theSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (theSocket == INVALID_SOCKET) {
		nret = WSAGetLastError();
		ReportError(nret, "socket()");
		WSACleanup();
		return NETWORK_ERROR;
	}

	SOCKADDR_IN serverInfo;
	serverInfo.sin_family = AF_INET;
	serverInfo.sin_addr = *((LPIN_ADDR)*hostEntry->h_addr_list);
	serverInfo.sin_port = htons(port);

	nret = connect(theSocket, (LPSOCKADDR)&serverInfo, sizeof(struct sockaddr));
	if (nret == SOCKET_ERROR) {
		nret = WSAGetLastError();
		ReportError(nret, "connect()");
		WSACleanup();
		return NETWORK_ERROR;
	}

	// Pod³¹czony! :)
	count = COUNT_MAX;
	delay = DELAY;
	dispersion = 0.0;
	minerr = 0.1;
    if (receive_sntp(theSocket, &recv_time) == SNTP_ERROR) {
    	closesocket(theSocket);
    	WSACleanup();	
        return SNTP_ERROR;
    }

	closesocket(theSocket);
	WSACleanup();	
	return NETWORK_OK;
}

void ReportError(int errorCode, const char *whichFunc) 
{
    char errorMsg[92];
    ZeroMemory(errorMsg, 92);
    sprintf(errorMsg, "Call to %s returned error %d!", (char *)whichFunc, errorCode);
    MessageBox(NULL, errorMsg, "socketIndication", MB_OK);
}

// Pobierz aktualny czas UTC w sekundach od Epoki
// plus offset (zazwyczaj czas od pocz¹tku stulecia do Epoki)
double current_time (double offset) 
{
    _timeb current;
    _ftime(&current);
    return offset + current.time + 1.0e-6 * current.millitm;
    /* ctime(&(current.time)); */
}

// Konwersja czasu do postaci ANSI C
time_t convert_time (double value, int *millisecs) 
{
    time_t result = (time_t)value;
    if ((*millisecs = (int)(1000.0*(value-result))) >= 1000) {
        *millisecs = 0;
        ++result;
    }
    return result;
} 

void display_data (ntp_data *data) 
{
    char text[256];
    sprintf(text, "sta=%d ver=%d mod=%d str=%d pol=%d dis=%.6f ref=%.6f\n",
        data->status,data->version,data->mode,data->stratum,data->polling,
        data->dispersion,data->reference);
    MessageBox(NULL, text, "time results debugging", MB_OK);
    sprintf(text, "ori=%.6f rec=%.6f\n",data->originate,data->receive);
    MessageBox(NULL, text, "time results debugging", MB_OK);
    sprintf(text, "tra=%.6f cur=%.6f\n",data->transmit,data->current);
    MessageBox(NULL, text, "time results debugging", MB_OK);
}

void display_packet (unsigned char *packet, int length) 
{
    int i;
    if (length < NTP_PACKET_MIN || length > NTP_PACKET_MAX) return;
    for (i = 0; i < length; ++i) {
        if (i != 0 && i%32 == 0)
            fprintf(stderr,"\n");
         else if (i != 0 && i%4 == 0)
             fprintf(stderr," ");
         fprintf(stderr,"%.2x",packet[i]);
    }
    fprintf(stderr,"\n");
}

// Upakuj najwa¿niejsze dane do pakietu NTP, pomijaj¹c ca³¹ strukturê
// i u³o¿enie bajtów (problemy z endianami ;) )
// S³owem, ignorujemy dane niezwi¹zane z SNTP.
void pack_ntp (unsigned char *packet, int length, ntp_data *data) 
{
    int i, k;
    double d;

    memset(packet,0,(size_t)length);
    packet[0] = (data->status<<6) | (data->version<<3) | data->mode;
    packet[1] = data->stratum;
    packet[2] = data->polling;
    packet[3] = data->precision;
    d = data->originate/NTP_SCALE;
    for (i = 0; i < 8; ++i) {
        if ((k = (int)(d *= 256.0)) >= 256) k = 255;
        packet[NTP_ORIGINATE+i] = k;
        d -= k;
    }
    d = data->receive/NTP_SCALE;
    for (i = 0; i < 8; ++i) {
        if ((k = (int)(d *= 256.0)) >= 256) k = 255;
        packet[NTP_RECEIVE+i] = k;
        d -= k;
    }
    d = data->transmit/NTP_SCALE;
    for (i = 0; i < 8; ++i) {
        if ((k = (int)(d *= 256.0)) >= 256) k = 255;
        packet[NTP_TRANSMIT+i] = k;
        d -= k;
    }
}

// Rozpakuj dane z pakietu NTP, pomijaj¹c ca³¹ strukturê
// i u³o¿enie bajtów (problemy z endianami ;) )
// S³owem, ignorujemy dane niezwi¹zane z SNTP.
void unpack_ntp (ntp_data *data, unsigned char *packet, int length) 
{
    int i;
    double d;

    data->current = current_time(JAN_1970);    /* Best to come first */
    data->status = (packet[0] >> 6);
    data->version = (packet[0] >> 3)&0x07;
    data->mode = packet[0]&0x07;
    data->stratum = packet[1];
    data->polling = packet[2];
    data->precision = packet[3];
    d = 0.0;
    for (i = 0; i < 4; ++i) d = 256.0*d+packet[NTP_DISP_FIELD+i];
    data->dispersion = d/65536.0;
    d = 0.0;
    for (i = 0; i < 8; ++i) d = 256.0*d+packet[NTP_REFERENCE+i];
    data->reference = d/NTP_SCALE;
    d = 0.0;
    for (i = 0; i < 8; ++i) d = 256.0*d+packet[NTP_ORIGINATE+i];
    data->originate = d/NTP_SCALE;
    d = 0.0;
    for (i = 0; i < 8; ++i) d = 256.0*d+packet[NTP_RECEIVE+i];
    data->receive = d/NTP_SCALE;
    d = 0.0;
    for (i = 0; i < 8; ++i) d = 256.0*d+packet[NTP_TRANSMIT+i];
    data->transmit = d/NTP_SCALE;
}

// Utwórz wychodz¹cy pakiet NTP, albo od zera, albo wychodz¹c
// z ¿¹dania klienta.
void make_packet (ntp_data *data, int mode) 
{
    data->status = NTP_LI_FUDGE << 6;
    data->stratum = NTP_STRATUM;
    data->reference = data->dispersion = 0.0;
    if (mode == NTP_SERVER) {
        data->mode = (data->mode == NTP_CLIENT ? NTP_SERVER : NTP_PASSIVE);
        data->originate = data->transmit;
        data->receive = data->current;
    } else {
        data->version = NTP_VERSION;
        data->mode = mode;
        data->polling = NTP_POLLING;
        data->precision = NTP_PRECISION;
        data->receive = data->originate = 0.0;
    }
    data->current = data->transmit = current_time(JAN_1970);
}

// Sprawdzamy pakiet i zwracamy przesuniêcie i ewentualnie b³¹d.
// Testów jest ciut wiêcej, ni¿ normalnie zawiera SNTP.
// Zwraca 0 gdy OK, 1 gdy b³¹d i 2 dla ignorowanego pakietu broadcastowego.
// Uwaga: funkcja nie mo¿e zmieniæ argumentów w przypadku b³êdu!
int read_packet (SOCKET isocket, ntp_data *data, double *off, double *err) 
{
    unsigned char receive[NTP_PACKET_MAX+1];
    double delay1, delay2, x, y;
    int response = 0, failed, length, i, k, nret;

    nret = recv(isocket, (char *)receive, NTP_PACKET_MAX+1, 0);
    if (nret == SOCKET_ERROR)
        return 1;
    length = nret;
    if (length < NTP_PACKET_MIN || length > NTP_PACKET_MAX)
        return 1;
    //fprintf(stderr, "Incoming packet on socket\n");
    //display_packet(receive, length);
    unpack_ntp(data, receive, length);
    //display_data(data);

    failed = (data->mode != NTP_SERVER && data->mode != NTP_PASSIVE);
    response = 1;
    if (failed || data->status != 0 || data->version < 1 ||
            data->version > NTP_VERSION_MAX ||
            data->stratum > NTP_STRATUM_MAX) {
        return 1;
    }

    delay1 = data->transmit-data->receive;
    delay2 = data->current-data->originate;
    failed = ((data->stratum != 0 && data->stratum != NTP_STRATUM_MAX &&
                data->reference == 0.0) || data->transmit == 0.0);
    if (response &&
            (data->originate == 0.0 || data->receive == 0.0 ||
                (data->reference != 0.0 && data->receive < data->reference) ||
                delay1 < 0.0 || delay1 > NTP_INSANITY || delay2 < 0.0 ||
                data->dispersion > NTP_INSANITY))
        failed = 1;
    if (failed) {
        return 1;
    }

    if (dispersion < data->dispersion) dispersion = data->dispersion;
    x = data->receive-data->originate;
    y = (data->transmit == 0.0 ? 0.0 : data->transmit-data->current);
    *off = 0.5*(x+y);
    *err = x-y;
    x = data->current-data->originate;
    if (0.5*x > *err) *err = 0.5*x;
    return 0;
}

// Formatuj stringa z czasem (kompletny czas, nie przesuniêcie)
void format_time (char *text, int length, double offset, 
                  double error, double drift, double drifterr) 
{
    int milli, len;
    time_t now;
    struct tm *gmt;
    static const char *months[] = {
        "Sty", "Lut", "Mar", "Kwi", "Maj", "Cze",
        "Lip", "Sie", "Wrz", "PaŸ", "Lis", "Gru"
    };

    now = convert_time(current_time(offset),&milli);
    errno = 0;
    if ((gmt = localtime(&now)) == NULL)
        return;
    len = 24;
    if (length <= len) return; // internal error calling format_time
    errno = 0;
    sprintf(text, "%.2d %s %.4d %.2d:%.2d:%.2d.%.3d", 
            gmt->tm_mday, months[gmt->tm_mon], gmt->tm_year+1900,
            gmt->tm_hour, gmt->tm_min, gmt->tm_sec, milli);
    if (strlen(text) != len)
        return;

    if (error >= 0.0) {
        if (length < len+30)
            return;
        errno = 0;
        sprintf(&text[len]," %c %.3f +/- %.3f sekund",(offset > 0.0 ? '-' : '+'),
                (offset > 0.0 ? offset : -offset),dispersion+error);
        if (strlen(&text[len]) < 22)
            return;
    }

    if (drifterr >= 0.0) {
        len = strlen(text);
        if (length < len+25)
            return;
        errno = 0;
        sprintf(&text[len]," %c %.1f +/- %.1f ppm",
                (drift > 0.0 ? '-' : '+'),1.0e6*fabs(drift),
                1.0e6*drifterr);
        if (strlen(&text[len]) < 17)
            return;
    }

    //MessageBox(NULL, text, "czas...", MB_OK);
    if (strlen(text) >= length)
        return;
}

int receive_sntp (SOCKET isocket, sntp_time *recv_time)
{
    double history[COUNT_MAX], guesses[COUNT_MAX];
    double offset, error, deadline;
    double a, b, x, y;
    int accepts = 0, rejects = 0, flushes = 0, replicates = 0, k;
    unsigned char transmit[NTP_PACKET_MIN];
    ntp_data data;
    char text[100];

    attempts = 0;
    deadline = current_time(JAN_1970) + delay;
    offset = 0.0;
    error = NTP_INSANITY;
    while (accepts < count && attempts < 2*count) {
        if (current_time(JAN_1970) > deadline)
            return -1;
        make_packet(&data,NTP_CLIENT);
        outgoing[attempts++] = data.transmit;
        pack_ntp(transmit, NTP_PACKET_MIN, &data);
        //flushes += flush_socket(isocket);
        send(isocket, (char *)transmit, NTP_PACKET_MIN, 0);
        if (read_packet(isocket, &data, &x, &y)) {
            if (++rejects > count)
                return SNTP_ERROR;
            else
                continue;
        } else
        ++accepts;

        if ((a = x - offset) < 0.0) a = -a;
        if (accepts <= 1) a = 0.0;
        b = error + y;
        if (y < error) {
            offset = x;
            error = y;
        }
        if (a > b)
            return SNTP_ERROR;
        if (error <= minerr) break;
    }

    if (accepts == 0) 
        return SNTP_ERROR;
    if (error > NTP_INSANITY) 
        return SNTP_ERROR;
    //format_time(text, 75, offset, error, 0.0, -1.0);
    //printf("%s\n", text);
    //MessageBox(NULL, text, "time results", MB_OK);

    // Wype³nij strukturê z wynikami
    int milli;
    time_t now = convert_time(current_time(offset), &milli);
    recv_time->time = *(localtime(&now));
    recv_time->offset = offset;
    recv_time->error = error;
    recv_time->dispersion = dispersion;
    recv_time->usec = milli;
    return SNTP_OK;
}

