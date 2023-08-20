/*
    Simple Network Time Protocol (SNTP) Version 4 for IPv4, IPv6 and OSI
    https://datatracker.ietf.org/doc/html/rfc4330
*/
/*
      NTP Packet Header
                           1                   2                   3
       0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9  0  1
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |LI | VN  |Mode |    Stratum    |     Poll      |   Precision    |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                          Root  Delay                           |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                       Root  Dispersion                         |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                     Reference Identifier                       |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                                                                |
      |                    Reference Timestamp (64)                    |
      |                                                                |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                                                                |
      |                    Originate Timestamp (64)                    |
      |                                                                |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                                                                |
      |                     Receive Timestamp (64)                     |
      |                                                                |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                                                                |
      |                     Transmit Timestamp (64)                    |
      |                                                                |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                 Key Identifier (optional) (32)                 |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                                                                |
      |                                                                |
      |                 Message Digest (optional) (128)                |
      |                                                                |
      |                                                                |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
*/

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace iTime;

public class SNTPClient
{
    private const byte SNTPDataLength = 48;
    private readonly byte[] SNTPData = new byte[SNTPDataLength];
    private const byte offReferenceID = 12;
    private const byte offReferenceTimestamp = 16;
    private const byte offOriginateTimestamp = 24;
    private const byte offReceiveTimestamp = 32;
    private const byte offTransmitTimestamp = 40;

    public enum SNTPLeapIndicator
    {
        NoWarning,		// 0 - No warning
        LastMinute61,	// 1 - Last minute has 61 seconds
        LastMinute59,	// 2 - Last minute has 59 seconds
        Alarm			// 3 - Alarm condition (clock not synchronized)
    }

    public enum SNTPMode
    {
        SymmetricActive,	// 1 - Symmetric active
        SymmetricPassive,	// 2 - Symmetric pasive
        Client,				// 3 - Client
        Server,				// 4 - Server
        Broadcast,			// 5 - Broadcast
        Unknown				// 0, 6, 7 - Reserved
    }

    public enum SNTPStratum
    {
        Unspecified,			// 0 - kiss-o'-death message
        PrimaryReference,		// 1 - primary reference (e.g. radio-clock)
        SecondaryReference,		// 2-15 - secondary reference (via NTP or SNTP)
        Reserved				// 16-255 - reserved
    }

    // Warns of an impending leap second to be inserted/deleted in the last
    // minute of the current day. (See the SNTPLeapIndicator enum)
    public SNTPLeapIndicator LeapIndicator
    {
        get
        {
            // Isolate the two most significant bits
            byte val = (byte)(SNTPData[0] >> 6);
            switch (val)
            {
                case 0: return SNTPLeapIndicator.NoWarning;
                case 1: return SNTPLeapIndicator.LastMinute61;
                case 2: return SNTPLeapIndicator.LastMinute59;
                case 3: goto default;
                default:
                    return SNTPLeapIndicator.Alarm;
            }
        }
    }

    // Version number of the protocol (3 or 4).
    public byte VersionNumber
    {
        get
        {
            // Isolate bits 3 - 5
            byte val = (byte)((SNTPData[0] & 0x38) >> 3);
            return val;
        }
    }

    // Returns mode. (See the SNTPMode enum)
    public SNTPMode Mode
    {
        get
        {
            // Isolate bits 0 - 3
            byte val = (byte)(SNTPData[0] & 0x7);
            switch (val)
            {
                case 0: goto default;
                case 6: goto default;
                case 7: goto default;
                default:
                    return SNTPMode.Unknown;
                case 1:
                    return SNTPMode.SymmetricActive;
                case 2:
                    return SNTPMode.SymmetricPassive;
                case 3:
                    return SNTPMode.Client;
                case 4:
                    return SNTPMode.Server;
                case 5:
                    return SNTPMode.Broadcast;
            }
        }
    }

    // Stratum of the clock. (See the SNTPStratum enum)
    public SNTPStratum Stratum
    {
        get
        {
            byte val = (byte)SNTPData[1];
            if (val == 0) return SNTPStratum.Unspecified;
            else
                if (val == 1) return SNTPStratum.PrimaryReference;
                else
                    if (val <= 15) return SNTPStratum.SecondaryReference;
                    else
                        return SNTPStratum.Reserved;
        }
    }

    // Maximum interval (seconds) between successive messages
    public uint PollInterval
    {
        get
        {
            return (uint)(Math.Pow(2, (sbyte)SNTPData[2]));
        }
    }

    // Precision (in seconds) of the clock
    public double Precision
    {
        get
        {
            return (Math.Pow(2, (sbyte)SNTPData[3]));
        }
    }

    // Round trip time (in milliseconds) to the primary reference source.
    public double RootDelay
    {
        get
        {
            int temp = 0;
            temp = 256 * (256 * (256 * SNTPData[4] + SNTPData[5]) + SNTPData[6]) + SNTPData[7];
            return 1000 * (((double)temp) / 0x10000);
        }
    }

    // Nominal error (in milliseconds) relative to the primary reference source.
    public double RootDispersion
    {
        get
        {
            int temp = 0;
            temp = 256 * (256 * (256 * SNTPData[8] + SNTPData[9]) + SNTPData[10]) + SNTPData[11];
            return 1000 * (((double)temp) / 0x10000);
        }
    }

    // Reference identifier (either a 4 character string or an IP address)
    public string ReferenceID
    {
        get
        {
            string val = "";
            switch (Stratum)
            {
                case SNTPStratum.Unspecified:
                    goto case SNTPStratum.PrimaryReference;
                case SNTPStratum.PrimaryReference:
                    val += (char)SNTPData[offReferenceID + 0];
                    val += (char)SNTPData[offReferenceID + 1];
                    val += (char)SNTPData[offReferenceID + 2];
                    val += (char)SNTPData[offReferenceID + 3];
                    break;
                case SNTPStratum.SecondaryReference:
                    switch (VersionNumber)
                    {
                        case 3:	// Version 3, Reference ID is an IPv4 address
                            string Address = SNTPData[offReferenceID + 0].ToString() + "." +
                                                SNTPData[offReferenceID + 1].ToString() + "." +
                                                SNTPData[offReferenceID + 2].ToString() + "." +
                                                SNTPData[offReferenceID + 3].ToString();
                            try
                            {
                                IPHostEntry Host = Dns.GetHostEntry(Address);
                                val = Host.HostName + " (" + Address + ")";
                            }
                            catch (Exception)
                            {
                                val = "N/A";
                            }
                            break;
                        case 4: // Version 4, Reference ID is the timestamp of last update
                            DateTime time = ComputeDate(GetMilliSeconds(offReferenceID));
                            // Take care of the time zone                                
                            TimeSpan offspan = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                            val = (time + offspan).ToString();
                            break;
                        default:
                            val = "N/A";
                            break;
                    }
                    break;
            }

            return val;
        }
    }


    // The time at which the clock was last set or corrected
    public DateTime ReferenceTimestamp
    {
        get
        {
            DateTime time = ComputeDate(GetMilliSeconds(offReferenceTimestamp));
            // Take care of the time zone
            TimeSpan offspan = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return time + offspan;
        }
    }

    // T1 - time request sent by client
    public DateTime OriginateTimestamp
    {
        get
        {
            return ComputeDate(GetMilliSeconds(offOriginateTimestamp));
        }
    }

    // T2 - time request received by server
    public DateTime ReceiveTimestamp
    {
        get
        {
            DateTime time = ComputeDate(GetMilliSeconds(offReceiveTimestamp));
            // Take care of the time zone
            TimeSpan offspan = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return time + offspan;
        }
    }

    // T3 - time reply sent by server
    public DateTime TransmitTimestamp
    {
        get
        {
            DateTime time = ComputeDate(GetMilliSeconds(offTransmitTimestamp));
            // Take care of the time zone
            TimeSpan offspan = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return time + offspan;
        }
        set
        {
            SetDate(offTransmitTimestamp, value);
        }
    }

    // T4 - time reply received by client
    public DateTime DestinationTimestamp = DateTime.Now;


    /*
    To calculate the roundtrip delay d and system clock offset t relative
    to the server, the client sets the Transmit Timestamp field in the
    request to the time of day according to the client clock in NTP
    timestamp format.  For this purpose, the clock need not be
    synchronized.  The server copies this field to the Originate
    Timestamp in the reply and sets the Receive Timestamp and Transmit
    Timestamp fields to the time of day according to the server clock in
    NTP timestamp format.
    */
    /*
    The roundtrip delay d and system clock offset t are defined as:
    d = (T4 - T1) - (T3 - T2)     t = ((T2 - T1) + (T3 - T4)) / 2.
    */
    public double RoundTripDelay
    {
        get
        {
            TimeSpan span = (DestinationTimestamp - OriginateTimestamp) - (ReceiveTimestamp - TransmitTimestamp);
            return span.TotalMilliseconds;
        }
    }

    // This is the most important data for user.
    // It calculates the difference between local system clock and reference clock.
    // Using this shift it's easy to properly adjust local clock.
    // See above comment for RoundTripDelay.
    public TimeSpan LocalClockOffset
    {
        get
        {
            // t = ((T2 - T1) + (T3 - T4)) / 2
            TimeSpan offset = ((ReceiveTimestamp - OriginateTimestamp) + (TransmitTimestamp - DestinationTimestamp)) / 2;
            return offset;
        }
    }

    // Compute date, given the number of milliseconds since January 1, 1900
    private DateTime ComputeDate(ulong milliseconds)
    {
        TimeSpan span = TimeSpan.FromMilliseconds((double)milliseconds);
        DateTime time = new DateTime(1900, 1, 1);
        time += span;
        return time;
    }

    // Compute the number of milliseconds, given the offset of a 8-byte array
    private ulong GetMilliSeconds(byte offset)
    {
        ulong intpart = 0, fractpart = 0;

        for (int i = 0; i <= 3; i++)
        {
            intpart = 256 * intpart + SNTPData[offset + i];
        }
        for (int i = 4; i <= 7; i++)
        {
            fractpart = 256 * fractpart + SNTPData[offset + i];
        }
        ulong milliseconds = intpart * 1000 + (fractpart * 1000) / 0x100000000L;
        return milliseconds;
    }

    // Set the date part of the SNTP data
    // <param name="offset">Offset at which the date part of the SNTP data is</param>
    // <param name="date">The date</param>
    private void SetDate(byte offset, DateTime date)
    {
        ulong intpart = 0, fractpart = 0;
        DateTime StartOfCentury = new DateTime(1900, 1, 1, 0, 0, 0);	// January 1, 1900 12:00 AM

        ulong milliseconds = (ulong)(date - StartOfCentury).TotalMilliseconds;
        intpart = milliseconds / 1000;
        fractpart = ((milliseconds % 1000) * 0x100000000L) / 1000;

        ulong temp = intpart;
        for (int i = 3; i >= 0; i--)
        {
            SNTPData[offset + i] = (byte)(temp % 256);
            temp /= 256;
        }

        temp = fractpart;
        for (int i = 7; i >= 4; i--)
        {
            SNTPData[offset + i] = (byte)(temp % 256);
            temp /= 256;
        }
    }

    // Returns true if received data is valid and if comes from a NTP-compliant time server.
    private bool IsResponseValid()
    {
        if (SNTPData.Length < SNTPDataLength || Mode != SNTPMode.Server)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    private DateTime GetCurrentTime() => DateTime.Now;

    public SNTPClient()
    {
        SNTPData[0] = 0x1B;
        for (int i = 1; i < 48; i++)
        {
            SNTPData[i] = 0;
        }
        TransmitTimestamp = GetCurrentTime();
    }

    public void GetSNTPDataFromServer(string Host, int TimeOut)
    {
        try
        {
            IPEndPoint listenEP = new IPEndPoint(IPAddress.Any, 123);
            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPHostEntry hostEntry = Dns.GetHostEntry(Host);
            IPEndPoint sendEP = new IPEndPoint(hostEntry.AddressList[0], 123);
            EndPoint epSendEP = (EndPoint)sendEP;

            int messageLength = 0;
            try
            {
                sendSocket.Bind(listenEP);

                bool messageReceived = false;
                int elapsedTime = 0;

                while (!messageReceived && (elapsedTime < TimeOut))
                {
                    sendSocket.SendTo(SNTPData, SNTPData.Length, SocketFlags.None, sendEP);
                    if (sendSocket.Available > 0)
                    {
                        messageLength = sendSocket.ReceiveFrom(SNTPData, ref epSendEP);
                        if (!IsResponseValid())
                        {
                            throw new Exception($"Host sent an invalid response.");
                        }
                        messageReceived = true;
                        break;
                    }
                    Thread.Sleep(500);
                    elapsedTime += 500;
                }
                if (!messageReceived)
                {
                    throw new TimeoutException($"Host did not respond.");
                }
            }
            catch (SocketException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                sendSocket.Close();
            }
        }
        catch (SocketException e)
        {
            throw new Exception(e.Message);
        }
    }


}
