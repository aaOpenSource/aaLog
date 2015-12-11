using System.IO;

namespace aaLogReader.Helpers
{
  /// <summary>
  /// Extension methods for reading different data types from streams.
  /// </summary>
  public static class StreamExtensions
  {
    /// <summary>
    /// Reads a 32-bit integer from the Stream starting at the given offset.
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="offset">The starting position within the Stream</param>
    /// <returns>int</returns>
    public static int GetInt(this Stream stream, int offset)
    {
      var bytes = new byte[4];
      stream.Seek(offset, SeekOrigin.Begin);
      stream.Read(bytes, 0, 4);
      return bytes.GetInt();
    }



        /// <summary>
        /// Reads an unsigned 64-bit integer from the Stream starting at the given offset. 
        /// It's assumed the value can be used to create a DateTime.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="offset">The starting position within the Stream</param>
        /// <returns>ulong</returns>
        public static ulong GetFileTime(this Stream stream, int offset)
    {
      var bytes = new byte[8];
      stream.Seek(offset, SeekOrigin.Begin);
      stream.Read(bytes, 0, 8);
      return bytes.GetFileTime();
    }
  }
}
