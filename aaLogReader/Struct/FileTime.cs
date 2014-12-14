using System;
using Newtonsoft.Json;


namespace aaLogReader
{
    public struct FileTime
    {
        [JsonIgnore]
        public uint dwLowDateTime;

        [JsonIgnore]
        public uint dwHighDateTime;

        public ulong @value
        {
            get
            {
                return checked(checked((ulong)Math.Round((double)((float)this.dwHighDateTime) * 4294967296)) + (ulong)this.dwLowDateTime);
            }
        }
    }
}