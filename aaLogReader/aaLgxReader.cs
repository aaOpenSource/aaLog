using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using aaLogReader.Helpers;

namespace aaLogReader
{
  /// <summary>
  /// Handles reading an aaLGX file that was exported from the System Management Console.
  /// </summary>
// ReSharper disable once InconsistentNaming
  public static class aaLgxReader
  {
    private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Reads the header section from the aaLGX file at the given path.
    /// </summary>
    /// <param name="filePath">Full path to the aaLGX file to load.</param>
    /// <returns>LogHeader</returns>
    /// <remarks>
    /// The header in an aaLGX file does not contain all of the fields that the header 
    /// in an aaLOG file. The following fields on the LogHeader will not be set:
    /// StartMsgNumber
    /// EndMsgNumber
    /// Session
    /// PrevFileName
    /// </remarks>
    public static LogHeader ReadLogHeader(string filePath)
    {
      LOG.InfoFormat("Reading log header: {0}", filePath);
      using (var stream = OpenFileStream(filePath))
      {
        return ReadLogHeader(stream, filePath);
      }
    }

    /// <summary>
    /// Reads the log records from the aaLGX file at the given path. 
    /// The records are streamed from the file as the results are enumerated.
    /// </summary>
    /// <param name="filePath">Full path to the aaLGX file to load.</param>
    /// <returns>IEnumerable of LogRecord</returns>
    public static IEnumerable<LogRecord> ReadLogRecords(string filePath)
    {
      LOG.InfoFormat("Reading log records: {0}", filePath);
      using (var stream = OpenFileStream(filePath))
      {
        var header = ReadLogHeader(stream, filePath);
        var offset = header.OffsetFirstRecord;
        var maxOffset = header.OffsetLastRecord;
        while (offset <= maxOffset)
        {
          var record = ReadLogRecord(stream, offset, header);
          offset += record.RecordLength;
          yield return record;
        }
      }
    }

    private static Stream OpenFileStream(string filePath)
    {
      LOG.DebugFormat("filePath: {0}", filePath);
      if (string.IsNullOrWhiteSpace(filePath))
      {
        throw new aaLogReaderException("Attempted to open log file with blank path");
      }
      if (!File.Exists(filePath))
      {
        throw new aaLogReaderException("File does not exist: {0}", filePath);
      }
      LOG.DebugFormat("Opening log file: {0}", filePath);
      var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      if (!stream.CanRead || stream.Length == 0)
      {
        throw new aaLogReaderException(string.Format("Can not open log file: {0}", filePath));
      }
      LOG.DebugFormat("Opened log file: {0}", filePath);
      return stream;
    }

    private static LogHeader ReadLogHeader(Stream stream, string filePath)
    {
      // ReSharper disable once UseObjectOrCollectionInitializer
      var header = new LogHeader();
      header.LogFilePath = filePath;
      // There are only 4 bytes for this in an aaLGX file.
      var headerLength = stream.GetInt(8);
      header.MsgCount = (ulong)stream.GetInt(12);
      header.StartFileTime = stream.GetFileTime(16);
      header.EndFileTime = stream.GetFileTime(24);
      header.OffsetFirstRecord = stream.GetInt(32);
      header.OffsetLastRecord = stream.GetInt(40);
      header.ComputerName = GetComputerName(stream, headerLength);
      header.HostFQDN = GetFqdn();
      return header;
    }

    private static string GetComputerName(Stream stream, int headerLength)
    {
      var bytes = new byte[headerLength];
      stream.Seek(0, SeekOrigin.Begin);
      stream.Read(bytes, 0, headerLength);
      var length = bytes.GetStringLength(48);
      return length > 0
        ? Encoding.Unicode.GetString(bytes, 48, length)
        : "";
    }

    private static LogRecord ReadLogRecord(Stream stream, int offset, LogHeader header)
    {
      LOG.DebugFormat("offset: {0}", offset);
// ReSharper disable once UseObjectOrCollectionInitializer
      var record = new LogRecord();
      record.RecordLength = stream.GetInt(offset + 4);

      stream.Seek(offset, SeekOrigin.Begin);
      var bytes = new byte[record.RecordLength];
      stream.Read(bytes, 0, record.RecordLength);

      record.MessageNumber = bytes.GetULong(16);
      record.SessionID = string.Format("{0}.{1}.{2}.{3}", bytes[27], bytes[26], bytes[25], bytes[24]);
      record.ProcessID = (uint)bytes.GetInt(28);
      record.ThreadID = (uint)bytes.GetInt(32);
      record.EventFileTime = bytes.GetFileTime(36);

      var position = 44;
      int length;
      record.LogFlag = bytes.GetString(position, out length);

      position += length + 2;
      record.Component = bytes.GetString(position, out length);

      position += length + 2;
      record.Message = bytes.GetString(position, out length);

      position += length + 2;
      record.ProcessName = bytes.GetString(position, out length);

      record.HostFQDN = header.HostFQDN;

      return record;
    }

    private static string GetFqdn()
    {
      try
      {
        return Fqdn.GetFqdn();
      }
      catch (Exception ex)
      {
        LOG.Warn(ex);
      }
      return "";
    }
  }
}