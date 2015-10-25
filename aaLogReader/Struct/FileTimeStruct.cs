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
                ulong lDt;

                lDt = (((ulong)dwHighDateTime) << 32) | dwLowDateTime;

                return lDt;
            }
        }
    }
}