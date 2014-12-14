

using System;
using Newtonsoft.Json;

namespace aaLogReader
{
    public struct SessionIDSegments
    {
        [JsonIgnore]
        public byte Segment1;

        [JsonIgnore]
        public byte Segment2;

        [JsonIgnore]
        public byte Segment3;

        [JsonIgnore]
        public byte Segment4;

        public string SessionID
        {
            get
            {
                string returnValue;

                string[] str = new string[] { this.Segment1.ToString(), ".", this.Segment2.ToString(), ".", this.Segment3.ToString(), ".", this.Segment4.ToString() };

                if (this.Segment1 == 0 & this.Segment2 == 0 & this.Segment3 == 0 & this.Segment4 == 0)
                {
                    returnValue = "0";
                }
                else
                {
                    returnValue = string.Concat(str);
                }

                return returnValue;
            }
        }
    }
}