using System;
namespace aaLogReader
{
    interface ILogHeader
    {
        string LogFilePath { get; set; }
        string ComputerName { get; set; }
        ulong EndFileTime { get; set; }
        DateTimeOffset EndDateTime { get; }
        DateTime EndDateTimeLocal { get; }
        DateTime EndDateTimeUtc { get; }
        string HostFQDN { get; set; }
        ulong MsgCount { get; set; }
        ulong EndMsgNumber { get; }
        ulong StartMsgNumber { get; set; }
        int OffsetFirstRecord { get; set; }
        int OffsetLastRecord { get; set; }
        string PrevFileName { get; set; }
        ReturnCodeStruct ReturnCode { get; set; }
        string Session { get; set; }
        ulong StartFileTime { get; set; }
        DateTimeOffset StartDateTime { get; }
        DateTime StartDateTimeLocal { get; }
        DateTime StartDateTimeUtc { get; }
        string ToCSV();
        string ToDelimitedString(char Delimiter = ',');
        string ToJSON();
        string ToKVP();
        string ToTSV();
    }
}
