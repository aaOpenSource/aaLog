using System;
using System.Text;

namespace aaLogReader.Helpers
{
  /// <summary>
  /// Extension methods for reading different data types from byte arrays.
  /// </summary>
  public static class ByteArrayExtensions
  {
        /// <summary>
        /// Reads a 32-bit integer from the byte array starting at the given offset.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">The starting position within the array. Defaults to zero.</param>
        /// <returns>int</returns>
        public static int GetInt(this byte[] bytes, int offset = 0)
        {
          return BitConverter.ToInt32(bytes, offset);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the byte array starting at the given offset.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">The starting position within the array. Defaults to zero.</param>
        /// <returns>int</returns>
        public static uint GetUInt32(this byte[] bytes, int offset = 0)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }
        
        /// <summary>
        /// Reads an unsigned 64-bit integer from the byte array starting at the given offset. 
        /// It's assumed the value can be used to create a DateTime.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">The starting position within the array. Defaults to zero.</param>
        /// <returns>ulong</returns>
        public static ulong GetFileTime(this byte[] bytes, int offset = 0)
    {
      var localFileTimeStruct = new FileTimeStruct
      {
        dwLowDateTime = BitConverter.ToUInt32(bytes, offset),
        dwHighDateTime = BitConverter.ToUInt32(bytes, checked(offset + 4))
      };
     return localFileTimeStruct.value;
    }

        /// <summary>
        /// Reads a string from the byte array starting at the given offset. 
        /// It's assumed the string is terminated by two 0 bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">The starting position within the array</param>
        /// <param name="length">Number of bytes in the string</param>
        /// <returns>string</returns>
        public static string GetString(this byte[] bytes, int offset, out int length)
        {
          length = bytes.GetStringLength(offset);
          return Encoding.Unicode.GetString(bytes, offset, length);
        }

        /// <summary>
        /// Calculates the length of a string in the byte array starting at the given offset. 
        /// It's assumed the string is terminated by two 0 bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">The starting position within the array</param>
        /// <returns>int</returns>
        public static int GetStringLength(this byte[] bytes, int offset)
        {
          var length = 0;
          var index = offset;
          while (true)
          {
            var value = BitConverter.ToUInt16(bytes, index);
            if (value == 0) break;
            length += 2;
            index += 2;
          }
          return length;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the byte array starting at the given offset. 
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">The starting position within the array. Defaults to zero.</param>
        /// <returns>ulong</returns>
        public static ulong GetULong(this byte[] bytes, int offset)
        {
          return BitConverter.ToUInt64(bytes, offset);
        }

        /// <summary>
        /// Extract SessionID segments from a byte array
        /// </summary>
        /// <param name="bytes">Byte array containing lastRecordRead data</param>
        /// <param name="offset">Starting offset for the data field</param>
        /// <returns></returns>
        public static string GetSessionID(this byte[] bytes, int offset)
        {
            try
            {
                return string.Format("{0}.{1}.{2}.{3}", bytes[offset + 3], bytes[offset + 2], bytes[offset + 1], bytes[offset]);
            }
            catch
            {
                return "0.0.0.0";
            }
        }
    }
}