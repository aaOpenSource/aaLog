using aaLogReader.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace aaLogReader
{
    public class aaLogReader : IDisposable
    {

        #region Globals

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private LogHeader _currentLogHeader;
        private List<LogHeader> _logHeaderIndex;
        private LogRecord _lastRecordRead;
        private ReturnCodeStruct _returnCloseValue;
        private FileStream _fileStream;
        private string _currentLogFilePath;
        private OptionsStruct _options;

        #endregion

        #region CTOR/DTOR

        /// <summary>
        /// Default constructor using default options
        /// </summary>
        public aaLogReader()
        {
            log.Debug("Create aaLogReader");

            // Initialize with default options
            Options = new OptionsStruct();

            this.Initialize();
        }


        /// <summary>
        /// Constructor using specificed options
        /// </summary>
        ///<param name="InitializationOptions">InitializationOptions passed as an OptionsStruct object </param>
        public aaLogReader(OptionsStruct InitializationOptions)
        {
            log.Debug("Create aaLogReader");
            log.Debug("Options - " + JsonConvert.SerializeObject(InitializationOptions));

            this.Options = InitializationOptions;
            this.Initialize();

        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~aaLogReader()
        {
            // Always close the file before exiting
            this.CloseCurrentLogFile();
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose object properly
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Perform detailed disposal functions in the middle of global dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_fileStream != null)
                {
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            }
            // free native resources if there are any.
        }

        #endregion

        #region Initilization

        /// <summary>
        /// Initialize the log reader
        /// </summary>
        private void Initialize()
        {
            log.Debug("");
            ReturnCodeStruct returnValue;

            /*
             * If the option to ignore the cache file on first read is set then we just delete
             * the current cache file and let it get written after the first record read
             */

            if (Options.IgnoreCacheFileOnFirstRead)
            {
                try
                {
                    System.IO.File.Delete(this.GetStatusCacheFilePath(Options.LogDirectory));
                }
                catch (Exception ex)
                {
                    log.Warn(ex);
                    //Do nothing if the file did not exist
                }
            }

            // Open current log file
            returnValue = this.OpenCurrentLogFile(Options.LogDirectory);

        }

        #endregion

        #region Properties

        public List<LogHeader> LogHeaderIndex
        {
            get
            {
                // Force a re-index any time this function is called to make sure we have most up to date data.
                this.IndexLogHeaders();
                return _logHeaderIndex;
            }

            private set { _logHeaderIndex = value; }
        }

        public LogHeader CurrentLogHeader
        {
            get { return _currentLogHeader; }
            private set { _currentLogHeader = value; }
        }

        public LogRecord LastRecordRead
        {
            get { return _lastRecordRead; }
            private set { _lastRecordRead = value; }
        }

        public ReturnCodeStruct ReturnCloseValue
        {
            get { return _returnCloseValue; }
            private set { _returnCloseValue = value; }
        }

        public string CurrentLogFilePath
        {
            get { return _currentLogFilePath; }
            private set { _currentLogFilePath = value; }
        }

        public OptionsStruct Options
        {
            get { return _options; }
            set { _options = value; }
        }

        #endregion

        #region File Management

        /// <summary>
        /// Open a log file specified by file path
        /// </summary>
        /// <param name="logFilePath">Complete file path to log file</param>
        /// <returns>ReturnCode Structure indicating success or failure with message</returns>
        public ReturnCodeStruct OpenLogFile(string logFilePath)
        {
            // TODO: This is not thread-safe. It updates _fileStream and _currentLogHeader. These should be moved to a return struct if possible.
            log.Debug("LogFilePath - " + logFilePath);

            ReturnCodeStruct localReturnCode;

            localReturnCode.Status = false;
            localReturnCode.Message = "";

            log.Debug("LogFilePath - " + logFilePath);

            // Verify we have a file path
            if (!string.IsNullOrEmpty(logFilePath))
            {
                log.Info("Opening log file " + logFilePath);

                // Save the log path
                this.CurrentLogFilePath = logFilePath;

                // Open up a filestream.  Make sure we access in read only and also allow others processes to read/write while we have it open
                var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                this._fileStream = fileStream;

                if ((fileStream.CanRead) && (fileStream.Length > 0))
                {
                    log.DebugFormat("Opened log file {0}", logFilePath);

                    // If opening the file was a success then go ahead and read in the header
                    var header = this.ReadLogHeader(fileStream);

                    // Since the _fileStream has changed, we need to update the CurrentLogHeader for the new file stream
                    CurrentLogHeader = header;

                    // Get the return code from the log header read
                    localReturnCode = header.ReturnCode;

                    log.Debug("logHeader - " + header.ToJSON());
                    log.Debug("localReturnCode - " + localReturnCode);

                }
                else
                {
                    throw new aaLogReaderException("Can not open log file " + logFilePath);
                }
            }
            else
            {
                throw new aaLogReaderException("Attempted to open log file with blank path");
            }

            return localReturnCode;
        }

        /// <summary>
        /// Open the latest log file in a specified directory or the default directory
        /// </summary>
        /// <param name="LogDirectory">Directory to inspect for latest log file</param>
        /// <returns>ReturnCode Structure indicating success or failure with message</returns>
        public ReturnCodeStruct OpenCurrentLogFile(string LogDirectory = "")
        {
            log.Debug("LogDirectory - " + LogDirectory);

            ReturnCodeStruct localReturnCode;

            try
            {
                if (string.IsNullOrEmpty(LogDirectory))
                {
                    LogDirectory = this.GetLogDirectory();
                }

                log.Debug("LogDirectory - " + LogDirectory);

                localReturnCode = this.OpenLogFile(this.LatestFileInPath(LogDirectory, "*.aalog"));

                log.Debug("localReturnCode - " + localReturnCode.ToString());

            }
            catch (Exception ex)
            {
                log.Error(ex);
                localReturnCode.Status = false;
                localReturnCode.Message = ex.ToString();
            }

            return localReturnCode;
        }

        /// <summary>
        /// Return the full file name of the latest file in the directory
        /// </summary>
        /// <param name="Path">Directory path</param>
        /// <param name="FileSearchPattern">Search file pattern</param>
        /// <returns></returns>
        private string LatestFileInPath(string Path, string FileSearchPattern)
        {
            log.Debug("Path - " + Path);
            log.Debug("FileSearchPattern - " + FileSearchPattern);

            string fullFileName = "";

            try
            {
                // Get directory info for the referenced directories
                DirectoryInfo localDirectoryInfo = new DirectoryInfo(Path);

                // Get the last written file in the directory
                fullFileName = localDirectoryInfo.GetFiles(FileSearchPattern).OrderByDescending(f => f.LastWriteTimeUtc).First().FullName;

            }
            catch (Exception ex)
            {
                log.Warn(ex);
                fullFileName = "";
            }

            return fullFileName;
        }

        /// <summary>
        /// Close the currently open log file
        /// </summary>
        /// <returns>ReturnCode Structure indicating success or failure with message</returns>
        public ReturnCodeStruct CloseCurrentLogFile()
        {
            log.Debug("");
            ReturnCodeStruct localReturnCode;

            try
            {
                // Close the global file stream to cleanup
                if (this._fileStream != null)
                {
                    this._fileStream.Close();
                    log.Info("Closed log file " + this.CurrentLogFilePath);
                }

                localReturnCode.Status = true;
                localReturnCode.Message = "";

            }
            catch (Exception ex)
            {
                log.Error(ex);
                localReturnCode.Status = false;
                localReturnCode.Message = ex.ToString();
            }
            return localReturnCode;
        }

        /// <summary>
        /// Close the currently open log file
        /// </summary>
        /// <returns>ReturnCode Structure indicating success or failure with message</returns>
        public ReturnCodeStruct CloseLogFile(FileStream fileStream)
        {
            log.Debug("");
            ReturnCodeStruct localReturnCode;

            try
            {
                // Close the global file stream to cleanup
                if (fileStream != null)
                {
                    fileStream.Close();
                    log.Info("Closed log file " + fileStream.Name);
                }

                localReturnCode.Status = true;
                localReturnCode.Message = "";
            }
            catch (Exception ex)
            {
                log.Error(ex);
                localReturnCode.Status = false;
                localReturnCode.Message = ex.ToString();
            }
            return localReturnCode;
        }

        #endregion

        #region Log Header Functions

        /// <summary>
        /// Read the log file header for the currently opened log file
        /// </summary>
        /// <returns>The log header for the currently opened log file</returns>
        public LogHeader ReadLogHeader()
        {
            log.Debug("");
            if (this.CurrentLogHeader != null)
            {
                return this.CurrentLogHeader;
            }
            else
            {
                return this.ReadLogHeader(this._fileStream);
            }
        }

        /// <summary>
        /// Read the log file header from the currently opened filestream
        /// </summary>
        /// <param name="logFileStream">Specific filestream to inspect and extract log header</param>
        /// <returns>The log header for the file stream</returns>
        public LogHeader ReadLogHeader(FileStream logFileStream)
        {
            log.DebugFormat("logFileStream length - ", logFileStream.Length);

            int readResult;
            LogHeader localHeader = new LogHeader();
            int workingOffset = 0;
            byte[] byteArray = new byte[1];
            int fieldLength;

            try
            {

                // Set our file pointer to the beginning
                logFileStream.Seek((long)0, SeekOrigin.Begin);

                //Recreate the Byte Array with the appropriate size
                byteArray = new byte[12];

                //Get first 12 byteArray into the byte array.  The header length is in the last 4 byteArray
                readResult = logFileStream.Read(byteArray, 0, 12);

                // Get the last4 byteArray, starting at byte 8 to get the length of the header
                int headerLength = BitConverter.ToInt32(byteArray, 8);

                logFileStream.Seek((long)0, SeekOrigin.Begin);

                // Redim the byte array to the size of the header
                byteArray = new byte[checked(headerLength + 1)];

                //Now read in the entire header, considering the header length from above
                readResult = logFileStream.Read(byteArray, 0, headerLength);

                // Start to pick out the values
                // Start Message Number
                localHeader.StartMsgNumber = byteArray.GetULong(20);

                // Message Count
                localHeader.MsgCount = (ulong)byteArray.GetInt(28);

                // Start and End FileTime
                localHeader.StartFileTime = byteArray.GetFileTime(32);
                localHeader.EndFileTime = byteArray.GetFileTime(40);

                // Offset for the first lastRecordRead
                localHeader.OffsetFirstRecord = byteArray.GetInt(48);

                // Offset for the last lastRecordRead
                localHeader.OffsetLastRecord = byteArray.GetInt(52);

                // Computer Name
                workingOffset = 56;
                localHeader.ComputerName = byteArray.GetString(56, out fieldLength);

                // Session
                workingOffset += fieldLength + 2;
                localHeader.Session = byteArray.GetString(workingOffset, out fieldLength);

                // Previous File Name
                workingOffset += fieldLength + 2;
                localHeader.PrevFileName = byteArray.GetString(workingOffset, out fieldLength);

                //HostFQDN
                localHeader.HostFQDN = Fqdn.GetFqdn();

                localHeader.ReturnCode = new ReturnCodeStruct { Status = true, Message = "" };
            }
            catch (Exception ex)
            {
                this.ReturnCloseValue = this.CloseLogFile(logFileStream);
                localHeader.ReturnCode = new ReturnCodeStruct { Status = false, Message = ex.Message };
                log.Error(ex);
            }
            finally
            {
                // Set the log header to this localheader we have calculated
                this.CurrentLogHeader = localHeader;
            }

            return localHeader;
        }

        /// <summary>
        /// Create an index of all log header information in the current log directory
        /// </summary>
        /// <returns>A list of objects of type LogHeader for all logs in the currently specified log directory</returns>
        private List<LogHeader> IndexLogHeaders()
        {
            log.DebugFormat("");
            return this.IndexLogHeaders(this.GetLogDirectory());
        }

        /// <summary>
        /// Create an index of all log header information in the specified directory
        /// </summary>
        /// <param name="LogDirectory">Path to Log File Directory</param>
        /// <returns>A list of objects of type LogHeader for all logs in the specified log directory</returns>
        private List<LogHeader> IndexLogHeaders(string LogDirectory)
        {
            log.Debug("LogDirectory - " + LogDirectory);

            string[] filePathList;
            LogHeader localLogHeader;
            FileStream localFileStream;
            List<LogHeader> localLogHeaderIndex;

            localLogHeaderIndex = new List<LogHeader>();

            //Verify the directory is valid
            if (!Directory.Exists(LogDirectory))
            {
                throw new aaLogReaderException(string.Format("Log directory {0} does not exist.", LogDirectory));
            }

            //Get all of the files in the directory
            filePathList = Directory.GetFiles(LogDirectory, "*.aaLog");

            foreach (string filePath in filePathList)
            {
                // Open up a filestream.  Make sure we access in read only and also allow others processes to read/write while we have it open
                localFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //Now read the log header
                localLogHeader = this.ReadLogHeader(localFileStream);

                if (localLogHeader.ReturnCode.Status)
                {
                    // Add the file path so we have more details about where this header lives
                    localLogHeader.LogFilePath = filePath;
                    localLogHeaderIndex.Add(localLogHeader);
                }
                else
                {
                    log.Error(string.Format("Error reading log header from {0}", filePath));
                }
            }

            //Perform error correction of known issues
            CorrectLogHeaderIndexErrors(ref localLogHeaderIndex);

            //Push to a global variable for persistence
            this.LogHeaderIndex = localLogHeaderIndex;

            return localLogHeaderIndex;
        }

        /// <summary>
        /// Correct any defects in log header index due to known issues with log file writes from the applications.
        /// </summary>
        /// <param name="LogHeaderIndex">Reference to log header Index</param>
        private void CorrectLogHeaderIndexErrors(ref List<LogHeader> LogHeaderIndex)
        {
            log.DebugFormat("LogHeaderIndex.Count - {0}", LogHeaderIndex.Count);

            for (int i = 0; i < LogHeaderIndex.Count; i++)
            {

                /* Phase 1
                 * Sometimes the start or end filetimes are not captured correctly so in this step we will scan for start or end times that are 0 and correct them with best available information
                 */

                if (LogHeaderIndex[i].StartFileTime == 0)
                {
                    if (i > 0)
                    {
                        LogHeaderIndex[i].StartFileTime = LogHeaderIndex[i - 1].EndFileTime + (ulong)1;
                    }
                }

                if (LogHeaderIndex[i].EndFileTime == 0)
                {
                    if (i < LogHeaderIndex.Count - 2)
                    {
                        LogHeaderIndex[i].EndFileTime = LogHeaderIndex[i + 1].StartFileTime - (ulong)1;
                    }
                }


                // Remove for now because this creates inconsistencies with the actual log files.
                // TODO: Update Log File Headers with consistent Data
                // * Phase 2
                // * Sometimes the last message number in one file overlaps with the first message number of the next file
                // */

                ////Only execute up to the next to the last index
                //if(i < LogHeaderIndex.Count-2 )
                //{
                //    //First identify if we have this situation
                //    if(LogHeaderIndex[i].EndMsgNumber >= LogHeaderIndex[i+1].StartMsgNumber)
                //    {
                //        /*
                //         * Ugghh, now we have to step through all of the start and end msgnumbers and shift them for constistency
                //         * But, if we do this right then we should only have to go through this once.
                //         */

                //        for(int k = i; k<= LogHeaderIndex.Count-2 ; k++)
                //        {
                //           //log.InfoFormat("k {0}/{1}", k, LogHeaderIndex.Count);

                //            // Correct the starting message for the next record
                //            //By looping through all subsequent records they should all be made consistent with each other
                //            //No need to rewrite EndMsgNumber because it is calculated on the fly
                //            LogHeaderIndex[k+1].StartMsgNumber = (LogHeaderIndex[k].EndMsgNumber + 1);
                //        }
                //    }
                //}
            }

        }

        #endregion

        #region Log Record Functions

        /// <summary>
        /// Read a log lastRecordRead that starts at the specified offset
        /// </summary>
        /// <param name="FileOffset">Offset for the current file stream</param>
        /// <param name="MessageNumber">Passed message number to set on the log lastRecordRead.  This should be calculated from external logic</param>
        /// <returns>A single log record</returns>
        private LogRecord ReadLogRecord(int FileOffset, ulong MessageNumber = 0)
        {
            log.Debug("");

            log.Debug("FileOffset - " + FileOffset.ToString());
            log.Debug("MessageNumber - " + MessageNumber.ToString());

            int recordLength = 0;
            LogRecord localRecord = new LogRecord();
            byte[] byteArray = new byte[1];
            int workingOffset = 0;
            int fieldLength;

            try
            {

                // Initialize the return status
                localRecord.ReturnCode.Status = false;
                localRecord.ReturnCode.Message = "";

                // Initialize working position
                workingOffset = 0;

                // Check to make sure we can even read from the file
                if (!_fileStream.CanSeek)
                {
                    throw new aaLogReaderException("Log file not open for reading");
                }

                // Go to the spot in the file stream specified by the offset
                this._fileStream.Seek((long)FileOffset, SeekOrigin.Begin);

                // Make sure we have at least 8 byteArray of data to read before hitting the end
                byteArray = new byte[8];
                if (this._fileStream.Read(byteArray, 0, 8) == 0)
                {
                    throw new aaLogReaderException("Attempt to read past End-Of-Log-File");
                }

                //Get the first 4 byteArray of data byte array that we just retrieved.
                // This tells us how long this lastRecordRead is.
                recordLength = BitConverter.ToInt32(byteArray, 4);

                // If the lastRecordRead length is not > 0 then bail on the function, returning an empty lastRecordRead with status code
                if (recordLength <= 0)
                {
                    throw new aaLogReaderException("Record Length is 0");
                }

                //Go back and reset to the specified offset
                this._fileStream.Seek((long)FileOffset, SeekOrigin.Begin);

                //Recreate the byte array with the proper length
                byteArray = new byte[checked(recordLength + 1)];

                //Now get the actual lastRecordRead data into the byte array for processing
                this._fileStream.Read(byteArray, 0, recordLength);

                // Record Length.  We've already calculated this so just use internal variable
                localRecord.RecordLength = recordLength;

                // Offset to Previous Record.
                localRecord.OffsetToPrevRecord = byteArray.GetInt(8);

                // Offset to Next Record
                localRecord.OffsetToNextRecord = checked((int)FileOffset + recordLength);

                // Session ID
                localRecord.SessionID = byteArray.GetSessionID(12);

                // Process ID
                localRecord.ProcessID = byteArray.GetUInt32(16);

                // Thread ID
                localRecord.ThreadID = byteArray.GetUInt32(20);

                // File Time
                localRecord.EventFileTime = byteArray.GetFileTime(24);

                // Log Flag
                workingOffset = 32;
                localRecord.LogFlag = byteArray.GetString(workingOffset, out fieldLength);

                /*
                 * Calc new working offset based on length of previously retrieved field.
                 * Can't forget that we're dealing with Unicode so we have to double the
                 * length to find the proper byte offset
                 */

                workingOffset += fieldLength + 2;
                localRecord.Component = byteArray.GetString(workingOffset, out fieldLength);

                workingOffset += fieldLength + 2;
                localRecord.Message = byteArray.GetString(workingOffset, out fieldLength);

                workingOffset += fieldLength + 2;
                localRecord.ProcessName = byteArray.GetString(workingOffset, out fieldLength);

                // Get the host from the header information
                localRecord.HostFQDN = ReadLogHeader().HostFQDN;

                localRecord.ReturnCode.Status = true;
                localRecord.ReturnCode.Message = "";
                // Set the message number on the lastRecordRead based on the value passed
                localRecord.MessageNumber = MessageNumber;

            }
            catch (System.ApplicationException saex)
            {
                // If this is a past the end of file message then handle gracefully
                if (saex.Message == "Attempt to read past End-Of-Log-File")
                {
                    this.ReturnCloseValue = this.CloseCurrentLogFile();

                    // Re-init the lastRecordRead to make sure it's totally blank.  Don't want to return a partial lastRecordRead
                    localRecord = new LogRecord();
                    localRecord.ReturnCode.Status = false;
                    localRecord.ReturnCode.Message = saex.Message;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

            // Set the last lastRecordRead read to this one.
            this.LastRecordRead = localRecord;

            // Return the working lastRecordRead
            return localRecord;
        }

        /// <summary>
        /// Get the first lastRecordRead in the log as specified by the OffsetFirstRecord in the header.
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetFirstRecord()
        {
            return GetFirstRecord(CurrentLogHeader);
        }

        /// <summary>
        /// Get the first lastRecordRead in the log as specified by the OffsetFirstRecord in the header.
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetFirstRecord(LogHeader logHeader)
        {
            log.Debug("");
            LogRecord localRecord = new LogRecord();

            if (logHeader.OffsetFirstRecord == 0)
            {
                this.LastRecordRead = new LogRecord();
            }
            else
            {
                localRecord = this.ReadLogRecord(logHeader.OffsetFirstRecord, logHeader.StartMsgNumber);
            }

            return localRecord;
        }

        /// <summary>
        /// Get the last lastRecordRead in the log as specified by the OffsetLastRecord in the header.
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetLastRecord()
        {
            return GetLastRecord(CurrentLogHeader);
        }

        /// <summary>
        /// Get the last lastRecordRead in the log as specified by the OffsetLastRecord in the header.
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetLastRecord(LogHeader logHeader)
        {
            log.Debug("");
            LogRecord localRecord = new LogRecord();

            if (logHeader.OffsetLastRecord == 0)
            {
                localRecord.ReturnCode.Status = false;
                localRecord.ReturnCode.Message = "Offset to Last Record is 0.  No record returned.";
            }
            else
            {
                localRecord = this.ReadLogRecord(logHeader.OffsetLastRecord, logHeader.EndMsgNumber);
            }

            return localRecord;
        }

        /// <summary>
        /// Get the next lastRecordRead in the log file
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetNextRecord()
        {
            return GetNextRecord(this.CurrentLogHeader, this.LastRecordRead);
        }

        /// <summary>
        /// Get the next lastRecordRead in the log file
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetNextRecord(LogHeader logHeader, LogRecord lastRecordRead)
        {
            log.Debug("");
            LogRecord localRecord;

            if (lastRecordRead.OffsetToNextRecord == 0)
            {
                // We haven't read any records yet so just get the first lastRecordRead
                return this.GetFirstRecord(logHeader);
            }
            else
            {
                // Cache the last message number
                ulong lastMessageNumber = lastRecordRead.MessageNumber;

                // If we are already at the end of the log file
                if (lastMessageNumber >= logHeader.EndMsgNumber)
                {
                    // First step is to get an updated list of index headers
                    List<LogHeader> localHeaderIndex = this.IndexLogHeaders();

                    //Now try to find the header for the next message number and is not this current log file
                    List<LogHeader> foundHeaders = localHeaderIndex.FindAll(x => x.StartMsgNumber > lastMessageNumber && x.LogFilePath != CurrentLogFilePath).OrderBy(x => x.StartMsgNumber).ToList<LogHeader>();

                    //If we did not find any headers that usually means we are at the end.  We return an empty record with status of false and let upstream code manage the results
                    if (foundHeaders == null)
                    {
                        return new LogRecord();
                    }

                    if (foundHeaders.Count <= 0)
                    {
                        return new LogRecord();
                    }

                    //TODO: Figure out how we want to loop through and find correct log file.
                    // If we make it this far then we should have a good log file to open
                    if (this.OpenLogFile(foundHeaders[0].LogFilePath).Status)
                    {
                        localRecord = this.GetFirstRecord();
                    }
                    else
                    {
                        throw new aaLogReaderException(string.Format("Error opening next log file after message number {0}", lastMessageNumber));
                    }
                }
                else
                {
                    // Read the lastRecordRead based off offset information from last lastRecordRead read
                    localRecord = this.ReadLogRecord(lastRecordRead.OffsetToNextRecord, Convert.ToUInt64(decimal.Add(new decimal(lastMessageNumber), decimal.One)));
                }
            }

            return localRecord;
        }

        /// <summary>
        /// Get the lastRecordRead immediately previous to the current lastRecordRead in the log file.  This call will swap to previous log files as required.
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetPrevRecord()
        {
            return GetPrevRecord(this.CurrentLogHeader, this.LastRecordRead);
        }

        /// <summary>
        /// Get the lastRecordRead immediately previous to the current lastRecordRead in the log file.  This call will swap to previous log files as required.
        /// </summary>
        /// <returns>A single log record</returns>
        public LogRecord GetPrevRecord(LogHeader logHeader, LogRecord lastRecordRead)
        {
            log.Debug("");
            ulong lastMessageNumber;
            LogRecord localRecord = new LogRecord();

            if (lastRecordRead.OffsetToPrevRecord != 0)
            {
                log.Debug("lastRecordRead.OffsetToPrevRecord != 0");

                // Cache the last message number
                lastMessageNumber = lastRecordRead.MessageNumber;

                // Read the lastRecordRead based off offset information from last lastRecordRead read

                localRecord = this.ReadLogRecord(lastRecordRead.OffsetToPrevRecord, lastMessageNumber - 1);
            }
            // Check to see if we are at the beginning of if there is another log file we can connect to
            else if (string.IsNullOrEmpty(logHeader.PrevFileName))
            {
                log.Debug("lastRecordRead.OffsetToPrevRecord == 0 AND logHeader.PrevFileName is empty");

                localRecord.ReturnCode.Status = false;
                // Beginning of Log
                localRecord.ReturnCode.Message = "BOL";
            }
            else
            {
                log.Debug("Close current log file");

                // Close the currently opened log file
                this._fileStream.Close();

                string newPreviousLogFile = Path.Combine(this.GetLogDirectory(), logHeader.PrevFileName);

                log.Debug("newPreviousLogFile - " + newPreviousLogFile);

                try
                {
                    if (this.OpenLogFile(newPreviousLogFile).Status)
                    {
                        localRecord = this.GetLastRecord();
                        log.Debug("localRecord.ReturnCode.Status - " + localRecord.ReturnCode.Status);
                    }
                    else
                    {
                        log.ErrorFormat("Error attempting to open previous log file: {0}", newPreviousLogFile);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error attempting to open previous log file: {0} - {1}", newPreviousLogFile, ex.Message);
                    throw new aaLogReaderException("Error attempting to open previous log file.", ex);
                }
            }

            return localRecord;
        }

        /// <summary>
        /// Get the log file path for a specific message number
        /// </summary>
        /// <param name="MessageNumber">Message number to search for</param>
        /// <returns>Complete paths to log files containing specific message number.  Will return "" if no log file found</returns>
        public List<string> GetLogFilePathsForMessageNumber(ulong MessageNumber)
        {
            log.Debug("MessageNumber - " + MessageNumber);

            List<string> returnValue = new List<string>();

            //Now try to find the specific file where the
            List<LogHeader> foundLogHeaders = this.IndexLogHeaders().FindAll(x => x.StartMsgNumber <= MessageNumber && MessageNumber <= x.EndMsgNumber).OrderBy(x => x.StartFileTime).ToList<LogHeader>();

            if (foundLogHeaders.Count > 0)
            {
                foreach (LogHeader localheader in foundLogHeaders)
                {
                    returnValue.Add(localheader.LogFilePath);
                }
            }
            else
            {
                log.WarnFormat("Could not find log file for MessageNumber {0}", MessageNumber);
            }

            return returnValue;
        }

        /// <summary>
        /// Get the log file path for a specific message timestamp
        /// </summary>
        /// <param name="MessageTimestamp">Message timestamp to search for</param>
        /// <returns>Complete paths to log files containing specific message timestamp.  Will return "" if no log file found</returns>
        public List<string> GetLogFilePathsForMessageTimestamp(DateTime MessageTimestamp)
        {
            return this.GetLogFilePathsForMessageFileTime((ulong)MessageTimestamp.ToFileTime());
        }

        /// <summary>
        /// Get the log file path for a specific message filetime
        /// </summary>
        /// <param name="MessageFiletime">Message filetime to search for</param>
        /// <returns>Complete paths to log files containing specific message filetime.  Will return "" if no log file found</returns>
        public List<string> GetLogFilePathsForMessageFileTime(ulong MessageFiletime)
        {
            log.DebugFormat("MessageFiletime - {0}", MessageFiletime);

            List<string> returnValue = new List<string>();

            //Now try to find the specific file where the message is located based on timestamp
            List<LogHeader> foundLogHeaders = this.IndexLogHeaders().FindAll(x => x.StartFileTime <= MessageFiletime && MessageFiletime <= x.EndFileTime).OrderBy(x => x.StartFileTime).ToList<LogHeader>();

            if (foundLogHeaders.Count > 0)
            {
                foreach (LogHeader localheader in foundLogHeaders)
                {
                    returnValue.Add(localheader.LogFilePath);
                }
            }
            else
            {
                log.WarnFormat("Could not find log file for message filetime {0}", MessageFiletime);
            }


            return returnValue;
        }

        /// <summary>
        /// Return a single log record identified by the specific message number.  If no record is found a blank log record is returned.
        /// </summary>
        /// <param name="MessageNumber">Specific message number to search for</param>
        /// <returns>A single log record</returns>
        public LogRecord GetRecordByMessageNumber(ulong messageNumber)
        {
            return GetRecordByMessageNumber(messageNumber, CurrentLogHeader);
        }

        /// <summary>
        /// Return a single log record identified by the specific message number.  If no record is found a blank log record is returned.
        /// </summary>
        /// <param name="messageNumber">Specific message number to search for</param>
        /// <returns>A single log record</returns>
        public LogRecord GetRecordByMessageNumber(ulong messageNumber, LogHeader logHeader)
        {
            log.DebugFormat("MessageNumber - {0}", messageNumber);

            LogRecord returnValue = new LogRecord();

            /* For optimization purposes first locate the log files that may contain the specific message number
             We say file(s) because there is currently an issue with how the log system writes files that may repeat a message number
             in that case will find the first match and return that
            */
            foreach (string logFilePath in GetLogFilePathsForMessageNumber(messageNumber))
            {
                // Get a reference to the log file by opening it
                if (!OpenLogFile(logFilePath).Status)
                {
                    throw new aaLogReaderException(string.Format("Error opening log file {0}", logFilePath));
                }

                //Get the header which should be loaded into a global in memory now
                LogHeader localHeader = logHeader;

                //Determine if we are closer to the beginning or end
                if ((messageNumber - localHeader.StartMsgNumber) <= (localHeader.EndMsgNumber - messageNumber))
                {
                    //Looks like we are closer to beginning to start at beginning and go next
                    returnValue = GetFirstRecord();

                    // Start looping until we find the record we are looking for
                    while (returnValue.ReturnCode.Status && returnValue.MessageNumber < messageNumber)
                    {
                        returnValue = GetNextRecord();
                    }
                }
                else
                {
                    //Looks like we are closer to the end so start at end and go previous
                    returnValue = GetLastRecord();

                    // Start looping until we find the record we are looking for
                    while (returnValue.ReturnCode.Status && returnValue.MessageNumber > messageNumber)
                    {
                        returnValue = GetPrevRecord();
                    }
                }

                // Check to see if we have found our record
                if (returnValue.MessageNumber == messageNumber)
                {
                    // Dump out of the for loop
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        ///  Return a single log record identified by the specific message filetime.  If no record is found a blank log record is returned.
        /// </summary>
        /// <param name="MessageFiletime">Message filetime to use when searching</param>
        /// <param name="TimestampEarlyOrLate">Earliest = Message immediately before timestamp, Latest = Message immediately after timestamp</param>
        /// <returns>A single log record</returns>
        public LogRecord GetRecordByFileTime(ulong MessageFiletime, EarliestOrLatest TimestampEarlyOrLate = EarliestOrLatest.Earliest)
        {
            log.DebugFormat("MessageFiletime - {0}", MessageFiletime);

            LogRecord returnValue = new LogRecord();
            bool foundRecord = false;

            /* For optimization purposes first locate the log files that may contain a message with the specified filetime
             We say file(s) because there is currently an issue with how the log system writes files that may overlap timestamps
             in that case will find the first match and return that
             *
             * General premise of searching early or late is that early will get message immediately on or before target timestamp
             * and late will get message immediately on or after target timestamp
            */
            foreach (string logFilePath in GetLogFilePathsForMessageFileTime(MessageFiletime))
            {
                // Get a reference to the log file by opening it
                if (!OpenLogFile(logFilePath).Status)
                {
                    throw new aaLogReaderException(string.Format("Error opening log file {0}", logFilePath));
                }

                //Get the header which should be loaded into a global in memory now
                LogHeader localHeader = this.CurrentLogHeader;

                //Determine if we are closer to the beginning or end
                if ((MessageFiletime - localHeader.StartFileTime) <= (localHeader.EndFileTime - MessageFiletime))
                {

                    log.DebugFormat("Starting from beginning of file at filetime {0}", localHeader.StartFileTime);

                    //Looks like we are closer to beginning to start at beginning and go next
                    returnValue = GetFirstRecord();

                    // Start looping until we find the record we are looking for considering the Early or Late Timestamp parameters
                    while (returnValue.ReturnCode.Status)
                    {
                        // If we have gone past our target timestamp then go back and get the last record
                        if (returnValue.EventFileTime >= MessageFiletime)
                        {
                            if (TimestampEarlyOrLate == EarliestOrLatest.Earliest)
                            {
                                // Go back one record
                                returnValue = GetPrevRecord();

                                // Make sure we got a good record then dump out of the while loop
                                if (returnValue.ReturnCode.Status)
                                {
                                    foundRecord = true;
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        // Get the next record
                        returnValue = GetNextRecord();
                    }
                }
                else
                {
                    //Looks like we are closer to the end so start at end and go previous
                    returnValue = GetLastRecord();

                    // Start looping until we find the record we are looking for considering the Early or Late Timestamp parameters
                    while (returnValue.ReturnCode.Status)
                    {
                        // If we have gone past our target timestamp then go back and get the last record
                        if (returnValue.EventFileTime <= MessageFiletime)
                        {
                            if (TimestampEarlyOrLate == EarliestOrLatest.Latest)
                            {
                                // Go back one record
                                returnValue = GetNextRecord();

                                // Make sure we got a good record then dump out of the while loop
                                if (returnValue.ReturnCode.Status)
                                {
                                    foundRecord = true;
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        // Get the previous record
                        returnValue = GetPrevRecord();
                    }
                }

                // Check to see if we have found our record
                if (foundRecord)
                {
                    // Dump out of the for loop
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        ///  Return a single log record identified by the specific message timestamp.  If no record is found a blank log record is returned.
        /// </summary>
        /// <param name="MessageTimestamp">Message timestamp to use when searching</param>
        /// <param name="TimestampEarlyOrLate">Earliest = Message immediately before timestamp, Latest = Message immediately after timestamp</param>
        /// <returns>A single log record</returns>
        public LogRecord GetRecordByTimestamp(DateTime MessageTimestamp, EarliestOrLatest TimestampEarlyOrLate = EarliestOrLatest.Earliest)
        {
            log.DebugFormat("MessageTimestamp - {0}", MessageTimestamp);
            return this.GetRecordByFileTime((ulong)MessageTimestamp.ToFileTime(), TimestampEarlyOrLate);
        }

        /// <summary>
        /// Get list of records bounded by the specified start message and specific count of messages including and after the message at the start message number.
        /// </summary>
        /// <param name="MessageNumber">Starting Message Number</param>
        /// <param name="Count">Count of records to return</param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByStartMessageNumberAndCount(ulong MessageNumber, int Count)
        {
            log.DebugFormat("MessageNumber - {0}", MessageNumber);
            log.DebugFormat("Count - {0}", Count);

            return this.GetRecordsByMessageNumberAndCount(MessageNumber, Count, SearchDirection.Forward);
        }

        /// <summary>
        /// Get list of records bounded by the specified end message and specific count of messages including and before the message at the end message number.
        /// </summary>
        /// <param name="MessageNumber">Ending Message Number</param>
        /// <param name="Count">Count of records to return</param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByEndMessageNumberAndCount(ulong MessageNumber, int Count)
        {
            log.DebugFormat("MessageNumber - {0}", MessageNumber);
            log.DebugFormat("Count - {0}", Count);
            return this.GetRecordsByMessageNumberAndCount(MessageNumber, Count, SearchDirection.Back);
        }

        /// <summary>
        /// Get list of records bounded by the specified message number, specific count of messages, and search direction.
        /// </summary>
        /// <param name="MessageNumber">Specified starting Message Number</param>
        /// <param name="Count">Count of records to return</param>
        /// <param name="Direction">The direction to search, forwards or backwards</param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByMessageNumberAndCount(ulong MessageNumber, int Count, SearchDirection Direction = SearchDirection.Forward)
        {
            log.DebugFormat("MessageNumber - {0}", MessageNumber);
            log.DebugFormat("Count - {0}", Count);
            log.DebugFormat("Direction - {0}", Direction.ToString());

            List<LogRecord> returnValue = new List<LogRecord>();

            // Get the first recordin the list
            LogRecord localRecord = this.GetRecordByMessageNumber(MessageNumber);

            if (localRecord.ReturnCode.Status)
            {
                returnValue.Add(localRecord);
            }

            // Now start looping through until we have exceeded our target count or the last record we read returned a non true status, indicating a bad read or more typically no more records to read.
            while ((returnValue.Count < Count) && localRecord.ReturnCode.Status)
            {
                if (Direction == SearchDirection.Back)
                {
                    localRecord = GetPrevRecord();
                }
                else
                {
                    localRecord = GetNextRecord();
                }

                if (localRecord.ReturnCode.Status)
                {
                    returnValue.Add(localRecord);
                }
                else
                {
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Get list of records bounded by the specified start and end message number
        /// </summary>
        /// <param name="StartMessageNumber">Specified starting message number</param>
        /// <param name="EndMessageNumber">Specified ending message number</param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByStartandEndMessageNumber(ulong StartMessageNumber, ulong EndMessageNumber)
        {
            log.DebugFormat("StartMessageNumber - {0}", StartMessageNumber);
            log.DebugFormat("EndMessageNumber - {0}", EndMessageNumber);

            List<LogRecord> returnValue = new List<LogRecord>();
            LogRecord localRecord = new LogRecord();

            if (StartMessageNumber > EndMessageNumber)
            {
                // Reverse
                log.WarnFormat("Start ({0}) and End ({0}) Message Numbers Reversed. Correcting before proceeding", StartMessageNumber, EndMessageNumber);
                ulong temp = EndMessageNumber;
                EndMessageNumber = StartMessageNumber;
                StartMessageNumber = temp;
            }

            localRecord = this.GetRecordByMessageNumber(StartMessageNumber);

            if (localRecord.ReturnCode.Status)
            {
                returnValue.Add(localRecord);
            }

            while ((localRecord.MessageNumber <= EndMessageNumber) && localRecord.ReturnCode.Status)
            {
                localRecord = GetNextRecord();

                if (localRecord.ReturnCode.Status)
                {
                    returnValue.Add(localRecord);
                }
                else
                {
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Get list of records bounded by specified end message filetime and message count
        /// </summary>
        /// <param name="EndFileTime">Ending message filetime</param>
        /// <param name="MessageCount"></param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByEndFileTimeAndCount(ulong EndFileTime, int MessageCount = 1000)
        {
            log.DebugFormat("EndFileTime - {0}", EndFileTime);
            log.DebugFormat("MaximumMessageCount - {0}", MessageCount);

            List<LogRecord> returnValue = new List<LogRecord>();
            LogRecord localRecord = new LogRecord();

            //Find the exact message number and use the search by message number function
            localRecord = this.GetRecordByFileTime(EndFileTime, EarliestOrLatest.Earliest);

            if (!localRecord.ReturnCode.Status)
            {
                log.WarnFormat("Can't locate a record considering filetime {0}", EndFileTime);
                return returnValue;
            }

            //Record retrieval was succesful
            return this.GetRecordsByEndMessageNumberAndCount(localRecord.MessageNumber, MessageCount);
        }

        /// <summary>
        /// Get list of records bounded by specified end message timestamp and message count
        /// </summary>
        /// <param name="EndTimestamp">Ending message timestamp</param>
        /// <param name="MessageCount"></param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByEndTimestampAndCount(DateTime EndTimestamp, int MessageCount = 1000)
        {
            log.DebugFormat("EndTimestamp - {0}", EndTimestamp);
            log.DebugFormat("MaximumMessageCount - {0}", MessageCount);

            return this.GetRecordsByEndFileTimeAndCount((ulong)EndTimestamp.ToFileTime(), MessageCount);

        }

        /// <summary>
        /// Get list of records bounded by specified start message filetime and message count
        /// </summary>
        /// <param name="StartFileTime">Starting message filetime</param>
        /// <param name="MessageCount"></param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByStartFileTimeAndCount(ulong StartFileTime, int MessageCount = 1000)
        {
            log.DebugFormat("StartFileTime - {0}", StartFileTime);
            log.DebugFormat("MaximumMessageCount - {0}", MessageCount);

            List<LogRecord> returnValue = new List<LogRecord>();
            LogRecord localRecord = new LogRecord();

            //Find the exact message number and use the search by message number function
            localRecord = this.GetRecordByFileTime(StartFileTime, EarliestOrLatest.Latest);

            if (!localRecord.ReturnCode.Status)
            {
                log.WarnFormat("Can't locate a record considering filetime {0}", StartFileTime);
                return returnValue;
            }

            //Record retrieval was succesful
            return this.GetRecordsByStartMessageNumberAndCount(localRecord.MessageNumber, MessageCount);
        }

        /// <summary>
        /// Get list of records bounded by specified start message timestamp and message count
        /// </summary>
        /// <param name="StartTimestamp">Starting message timestamp</param>
        /// <param name="MessageCount"></param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByStartTimestampAndCount(DateTime StartTimestamp, int MessageCount = 1000)
        {
            log.DebugFormat("StartTimestamp - {0}", StartTimestamp);
            log.DebugFormat("MaximumMessageCount - {0}", MessageCount);

            return this.GetRecordsByStartFileTimeAndCount((ulong)StartTimestamp.ToFileTime(), MessageCount);

        }

        /// <summary>
        /// Get list of records bounded by the start and end file time
        /// </summary>
        /// <param name="StartFileTime">Starting filetime for search</param>
        /// <param name="EndFileTime">Ending filetime for search</param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByStartAndEndFileTime(ulong StartFileTime, ulong EndFileTime)
        {
            log.DebugFormat("StartFileTime - {0}", StartFileTime);
            log.DebugFormat("EndFileTime - {0}", EndFileTime);

            List<LogRecord> returnValue = new List<LogRecord>();

            //Find the exact message number and use the search by message number function
            LogRecord startRecord = this.GetRecordByFileTime(StartFileTime, EarliestOrLatest.Latest);

            if (!startRecord.ReturnCode.Status)
            {
                log.WarnFormat("Can't locate a starting record considering filetime {0}", StartFileTime);
                return returnValue;
            }

            LogRecord endRecord = this.GetRecordByFileTime(EndFileTime, EarliestOrLatest.Earliest);

            if (!endRecord.ReturnCode.Status)
            {
                //If we can't find a record then just default to the latest record
                endRecord = this.GetUnreadRecords(1, "", true).First<LogRecord>();

                if (!endRecord.ReturnCode.Status)
                {
                    log.WarnFormat("Can't locate an ending record considering filetime {0}", EndFileTime);
                    return returnValue;
                }
            }

            return this.GetRecordsByStartandEndMessageNumber(startRecord.MessageNumber, endRecord.MessageNumber);
        }

        /// <summary>
        /// Get list of records bounded by the start and end timestamp
        /// </summary>
        /// <param name="StartTimeStamp">Starting timestamp for search</param>
        /// <param name="EndTimeStamp">Ending timestamp for search</param>
        /// <returns>List of Log Records</returns>
        public List<LogRecord> GetRecordsByStartAndEndTimeStamp(DateTime StartTimeStamp, DateTime EndTimeStamp)
        {
            log.DebugFormat("StartTimeStamp - {0}", StartTimeStamp);
            log.DebugFormat("EndTimeStamp - {0}", EndTimeStamp);

            return this.GetRecordsByStartAndEndFileTime((ulong)StartTimeStamp.ToFileTime(), (ulong)EndTimeStamp.ToFileTime());

        }

        /// <summary>
        /// Get messages starting from the last lastRecordRead working backwards.
        /// </summary>
        /// <param name="maximumMessages">Maximum number of messages to return</param>
        /// <param name="messagePatternToStop">Message pattern to match for ending search</param>
        /// <param name="IgnoreCacheFile">Ignore the cache file and read all messages up to maximum or message pattern</param>
        /// <returns></returns>
        public List<LogRecord> GetUnreadRecords(ulong maximumMessages = 1000, string messagePatternToStop = "", bool IgnoreCacheFile = false)
        {
            log.DebugFormat("maximumMessages - {0}", maximumMessages.ToString());
            log.DebugFormat("messagePatternToStop - {0}", messagePatternToStop);
            log.DebugFormat("IgnoreCacheFile - {0}", IgnoreCacheFile);

            ulong lastMessageNumber = ulong.MinValue;

            // If we are not explicitely ignoring the cache file AND we haven't specified a starting message number
            // then read the cache file to rigure out where we stopped last time.
            if ((!IgnoreCacheFile))
            {
                string cacheFilePath = this.GetStatusCacheFilePath();
                log.Debug("cacheFilePath - " + cacheFilePath);

                // If the cache file exists and we should not ignore it
                if (File.Exists(cacheFilePath))
                {
                    // Get the JSON from the file
                    string objectJSONFromCacheFile = File.ReadAllText(this.GetStatusCacheFilePath());

                    // Deserialize into the Log Record
                    LogRecord lastRecordFromCacheFile = JsonConvert.DeserializeObject<LogRecord>(objectJSONFromCacheFile);

                    log.Debug("lastRecordFromCacheFile - " + lastRecordFromCacheFile.ToJSON());

                    // Get the last message number from the retrieved lastRecordRead if it's available
                    if (lastRecordFromCacheFile != null)
                    {
                        lastMessageNumber = lastRecordFromCacheFile.MessageNumber;
                    }

                    log.DebugFormat("lastMessageNumber - {0}", lastMessageNumber.ToString());
                }
            }

            return this.GetRecordsInternal(lastMessageNumber, maximumMessages, messagePatternToStop);
        }

        /// <summary>
        /// Get messages utilizing start and stop message numbers if specified
        /// </summary>
        /// <param name="stopReadMessageNumber">Message number to stop reading records.  Default is ulong min value.</param>
        /// <param name="earliestMessageNumber">Message number to start reading.  Default is ulong max value</param>
        /// <param name="maximumMessages">Maximum number of messages to return</param>
        /// <param name="messagePatternToStop">Message pattern to match for ending search</param>
        /// <returns></returns>
        private List<LogRecord> GetRecordsInternal(ulong stopReadMessageNumber = ulong.MinValue, ulong maximumMessages = 1000, string messagePatternToStop = "")
        {
            List<LogRecord> logRecordList = new List<LogRecord>();
            LogRecord localRecord;
            bool getAnotherRecord;
            ReturnCodeStruct localReturnCode;

            try
            {
                log.Debug("stopReadMessageNumber - " + stopReadMessageNumber.ToString());
                log.Debug("maximumMessages - " + maximumMessages.ToString());
                log.Debug("messagePatternToStop - " + messagePatternToStop);

                //If the latest file in the directory does not match the file we are currently working on
                if (this.CurrentLogFilePath != this.LatestFileInPath(this.GetLogDirectory(), "*.aalog"))
                {
                    log.Info("Latest log file has changed.  Forcing a reread.");

                    // Force a reread
                    localReturnCode = this.OpenCurrentLogFile();

                    if (!localReturnCode.Status)
                    {
                        throw new aaLogReaderException("Error opening Current Log File.");
                    }
                }

                // Force a reread of the header so we know the latest values
                var logHeader = this.ReadLogHeader(this._fileStream);

                if (!logHeader.ReturnCode.Status)
                {
                    throw new aaLogReaderException("Error reading log header.");
                }

                log.Debug("logHeader.MsgLastNumber - " + logHeader.EndMsgNumber.ToString());
                log.Debug("lastReadMessageNumber - " + stopReadMessageNumber.ToString());

                // Short circuit if there are no new records
                if (logHeader.EndMsgNumber <= stopReadMessageNumber)
                {
                    log.Debug(string.Format("Short circuit return because this.logHeader.MsgLastNumber <= stopReadMessageNumber {0} <= {1} so we have read all of the messages", this.CurrentLogHeader.EndMsgNumber, stopReadMessageNumber));
                    return logRecordList;
                }

                // Start by getting the current last record in the current log file
                localRecord = this.GetLastRecord();

                log.Debug("GetLastRecord Message Number - " + localRecord.MessageNumber);
                log.Debug("GetLastRecord localRecord.ReturnCode.Status - " + localRecord.ReturnCode.Status);

                // If we get a lastRecordRead then add to the list and start iterating backwards through the message list.
                if (localRecord.ReturnCode.Status)
                {
                    logRecordList.Add(localRecord);

                    /* If the last retrieval was good
                     * and we have an offset for previous lastRecordRead
                     * and we haven't passed the maximum lastRecordRead count limit
                     * retrieve the next previous lastRecordRead
                     */

                    getAnotherRecord = this.ShouldGetNextRecord(localRecord, (ulong)logRecordList.Count, stopReadMessageNumber, maximumMessages, messagePatternToStop);

                    log.Debug("getAnotherRecord - " + getAnotherRecord);

                    while (getAnotherRecord)
                    {
                        localRecord = this.GetPrevRecord();

                        log.Debug("GetPrevRecord localRecord.ReturnCode.Status - " + localRecord.ReturnCode.Status);

                        if (localRecord.ReturnCode.Status)
                        {
                            logRecordList.Add(localRecord);
                        }

                        // Calculate if we should get another lastRecordRead
                        getAnotherRecord = this.ShouldGetNextRecord(localRecord, (ulong)logRecordList.Count, stopReadMessageNumber, maximumMessages, messagePatternToStop);

                        log.Debug("getAnotherRecord - " + getAnotherRecord);
                    }

                    // Write out the cache file if we read records
                    // The log record to cache should be the latest record, by message number
                    this.WriteStatusCacheFile(logRecordList.OrderByDescending(item => item.MessageNumber).First());
                }

                // After all records have been retrieved, apply filter
                // TODO: Consider profiling application at this layer vs during actual record retrieval.  The issue with at record retrieval is that it might interfere with
                // tracking mechanisms around last record etc.

                foreach (LogRecordFilterStruct CurrentFilter in Options.LogRecordPostFilters)
                {
                    ApplyLogRecordPostFilter(ref logRecordList, CurrentFilter);
                }

            }
            catch (Exception ex)
            {
                // Eat the exception here
                log.Error(ex);
            }

            return logRecordList;

        }

        /// <summary>
        /// Apply a filter to a list of log records
        /// </summary>
        /// <param name="LogRecordList">Log Records to apply filter to</param>
        /// <param name="RecordFilter">Filter to apply to list of log records</param>
        private void ApplyLogRecordPostFilter(ref List<LogRecord> LogRecordList, LogRecordFilterStruct RecordFilter)
        {
            log.Debug("");
            log.Debug("LogRecordList.Count - " + LogRecordList.Count.ToString());
            log.Debug("Filter - " + JsonConvert.SerializeObject(RecordFilter));

            switch (RecordFilter.Field.ToLower())
            {

                case "messagemumbermin":
                    ulong MessageNumberMinFilter = ulong.MaxValue;

                    if (ulong.TryParse(RecordFilter.Filter, out MessageNumberMinFilter))
                    {
                        LogRecordList = LogRecordList.Where<LogRecord>(x => x.MessageNumber >= MessageNumberMinFilter).ToList();
                    }
                    break;

                case "messagenumbermax":
                    ulong MessageNumberMaxFilter = ulong.MinValue;

                    if (ulong.TryParse(RecordFilter.Filter, out MessageNumberMaxFilter))
                    {
                        LogRecordList = LogRecordList.Where<LogRecord>(x => x.MessageNumber <= MessageNumberMaxFilter).ToList();
                    }
                    break;

                case "datetimemin":
                    DateTime DateTimeMinFilter = DateTime.MaxValue;

                    if (DateTime.TryParse(RecordFilter.Filter, out DateTimeMinFilter))
                    {
                        LogRecordList = LogRecordList.Where<LogRecord>(x => x.EventDateTime >= DateTimeMinFilter).ToList();
                    }
                    break;

                case "datetimemax":
                    DateTime DateTimeMaxFilter = DateTime.MinValue;

                    if (DateTime.TryParse(RecordFilter.Filter, out DateTimeMaxFilter))
                    {
                        LogRecordList = LogRecordList.Where<LogRecord>(x => x.EventDateTime <= DateTimeMaxFilter).ToList();
                    }
                    break;

                case "processid":
                    Regex ProcessIDRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => ProcessIDRegexSearch.IsMatch(x.ProcessID.ToString())).ToList();
                    break;

                case "threadid":
                    Regex ThreadIDRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => ThreadIDRegexSearch.IsMatch(x.ThreadID.ToString())).ToList();
                    break;

                case "logflag":
                    Regex LogFlagRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => LogFlagRegexSearch.IsMatch(x.LogFlag)).ToList();
                    break;

                case "component":
                    Regex ComponentRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => ComponentRegexSearch.IsMatch(x.Component)).ToList();
                    break;

                case "message":
                    Regex MessageRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => MessageRegexSearch.IsMatch(x.Message)).ToList();
                    break;

                case "processname":
                    Regex ProcessNameRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => ProcessNameRegexSearch.IsMatch(x.Message)).ToList();
                    break;

                case "sessionid":
                    Regex SessionIDRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => SessionIDRegexSearch.IsMatch(x.SessionID)).ToList();
                    break;

                case "hostfqdn":
                    Regex HostFQDNRegexSearch = new Regex(RecordFilter.Filter, RegexOptions.IgnoreCase);
                    LogRecordList = LogRecordList.Where<LogRecord>(x => HostFQDNRegexSearch.IsMatch(x.HostFQDN)).ToList();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Calculate if the logic should get the next record based on multiple factors
        /// </summary>
        /// <param name="lastRecord">The last record retrieved</param>
        /// <param name="logRecordCount">Current number of records retrieved</param>
        /// <param name="lastReadMessageNumber">Message number indicating it should be the last message to retrieve</param>
        /// <param name="maximumMessages">Maximum number of messages to retrieve. Will be compared to logRecordCount</param>
        /// <param name="messagePatternToStop">A specific message pattern to indicate the logic should not retrieve the next record</param>
        /// <returns></returns>
        private bool ShouldGetNextRecord(LogRecord lastRecord, ulong logRecordCount, ulong lastReadMessageNumber, ulong maximumMessages, string messagePatternToStop)
        {
            bool returnValue = false;

            try
            {
                log.Debug("");
                log.Debug("lastReadMessageNumber - " + lastReadMessageNumber.ToString());
                log.Debug("maximumMessages - " + maximumMessages.ToString());
                log.Debug("messagePattern - " + messagePatternToStop);

                /* If the last retrieval was good
                * and we have an offset for previous lastRecordRead
                * and we haven't passed the maximum lastRecordRead count limit
                * retrieve the next previous lastRecordRead
                */
                returnValue = (lastRecord.ReturnCode.Status && (lastRecord.OffsetToNextRecord > 0) && (lastRecord.MessageNumber > (lastReadMessageNumber + 1)) && (logRecordCount < maximumMessages));

                /* If the message pattern is not blank then apply a regex to see if we get a match
                 * If we match then that means this is the last lastRecordRead we should retrieve so return false
                 */
                if (returnValue && messagePatternToStop != "")
                {
                    returnValue &= !System.Text.RegularExpressions.Regex.IsMatch(lastRecord.Message, messagePatternToStop, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                returnValue = false;
            }

            return returnValue;
        }

        #endregion

        #region Private Helper Functions

        /// <summary>
        /// Write a text file out with metadata that can be used if the application is closed and reopened to read logs again
        /// </summary>
        /// <param name="CacheRecord">Complete record to write out containing cache information</param>
        private ReturnCodeStruct WriteStatusCacheFile(LogRecord CacheRecord)
        {
            log.Debug("");
            log.Debug("CacheRecord - " + CacheRecord.ToJSON());

            ReturnCodeStruct returnValue;

            try
            {
                System.IO.File.WriteAllText(this.GetStatusCacheFilePath(), CacheRecord.ToJSON());
                returnValue = new ReturnCodeStruct { Status = true, Message = "" };
            }
            catch (Exception ex)
            {
                log.Error(ex);
                returnValue = new ReturnCodeStruct { Status = false, Message = ex.Message };
            }

            return returnValue;

        }

        /// <summary>
        /// Calculate the path to the cache file
        /// </summary>
        /// <returns></returns>
        private string GetStatusCacheFilePath(string LogFilePath = "")
        {
            log.Debug("");
            string returnValue = "";

            try
            {
                string cacheFileName = "";

                // Check the global options to determine the features that have been configured

                if (!string.IsNullOrEmpty(Options.CacheFileNameCustom))
                {
                    cacheFileName = Options.CacheFileNameCustom;
                }
                else if (Options.CacheFileAppendProcessNameToBaseFileName)
                {
                    cacheFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + Options.CacheFileBaseName;
                }
                else
                {
                    cacheFileName = Options.CacheFileBaseName;
                }

                if (string.IsNullOrEmpty(LogFilePath))
                {
                    LogFilePath = Path.GetDirectoryName(this.CurrentLogFilePath);
                }

                returnValue = Path.Combine(LogFilePath, cacheFileName);
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                returnValue = "";
            }

            return returnValue;
        }

        /// <summary>
        /// Get the path to the local log directory
        /// </summary>
        /// <returns></returns>
        private string GetLogDirectory()
        {
            log.Debug("");

            string returnValue;

            //TODO: Figure out how to programatically determine the local log directory more deterministically
            try
            {
                if (!string.IsNullOrEmpty(Options.LogDirectory) && Directory.Exists(Options.LogDirectory))
                {
                    returnValue = Options.LogDirectory;
                }
                else
                {
                    // Use the registry key
                    returnValue = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\ArchestrA\Framework\Logger", "LogDir", Options.LogDirectory).ToString();
                    if (string.IsNullOrEmpty(returnValue))
                    {
                        // Use %ProgramData%\ArchestrA\LogFiles
                        var progData = System.Environment.GetEnvironmentVariable("ProgramData");
                        var logPath = Path.Combine(progData, @"ArchestrA\LogFiles");
                        if (Directory.Exists(logPath))
                            returnValue = logPath;
                        else
                        {
                            // Use default path
                            returnValue = @"C:\ProgramData\ArchestrA\LogFiles";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                returnValue = "";
            }

            return returnValue;
        }

        #endregion

    }
}