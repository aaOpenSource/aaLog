using System;
using Newtonsoft.Json;


namespace aaLogReader
{
    public struct FileTimeStruct
    {
        [JsonIgnore]
        public uint dwLowDateTime;

        [JsonIgnore]
        public uint dwHighDateTime;

        public ulong @value
        {
            get
            {               
               return checked(checked((ulong)Math.Round((double)((float)this.dwHighDateTime) * (Math.Pow(2,32)))) + (ulong)this.dwLowDateTime);
            }
        }
    }
}