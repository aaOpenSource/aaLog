using System;
namespace aaLogReader
{
    interface ILogHeader
    {
        string ComputerName { get; set; }
        DateTime EndDateTime { get; set; }
        string HostFQDN { get; set; }
        ulong MsgCount { get; set; }
        ulong MsgLastNumber { get; set; }
        ulong MsgStartingNumber { get; set; }
        int OffsetFirstRecord { get; set; }
        int OffsetLastRecord { get; set; }
        string PrevFileName { get; set; }
        ReturnCodeStruct ReturnCode { get; set; }
        string Session { get; set; }
        DateTime StartDateTime { get; set; }
        string ToCSV();
        string ToDelimitedString(char Delimiter = ',');
        string ToJSON();
        string ToKVP();
        string ToTSV();
    }
}
