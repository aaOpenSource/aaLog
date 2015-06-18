using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]

namespace aaLogReader
{
	public class aaLogReader : IDisposable
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SessionIDSegments sessionSeg;
        public FileTime sTime;
		public LogHeader logHeader;
		public LogRecord lastRecordRead;
		public ReturnCode returnValue;
		public ReturnCode returnCloseValue;
		private FileStream globalFileStream;
        private string currentLogFilePath;        
        private static aaLogReaderOptions globalOptions;


        #region CTOR/DTOR

        /// <summary>
        /// Default constructor using default options
        /// </summary>
        public aaLogReader()
        {
            // Setup logging
            log4net.Config.BasicConfigurator.Configure();

            log.Debug("Create aaLogReader");            

            try
            {
                // Initialize with default options
                globalOptions = new aaLogReaderOptions();

                this.Initialize();
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Constructor specifying options
        /// </summary>       
        ///<param name="InitializationOptions">Options passed as an aaLogReaderOptions object </param>
        public aaLogReader(aaLogReaderOptions InitializationOptions)
        {
            // Setup logging
            log4net.Config.BasicConfigurator.Configure();

            log.Debug("Create aaLogReader");
            log.Debug("Options - " + JsonConvert.SerializeObject(InitializationOptions));

            try
            {
                globalOptions = InitializationOptions;

                this.Initialize();
            }
            catch
            {
                throw;
            }
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (globalFileStream != null)
                {
                    globalFileStream.Dispose();
                    globalFileStream = null;
                }
            }
            // free native resources if there are any.
        }
    
       #endregion

        #region Initilization

        /// <summary>
        /// Initialize the log reader by opening the correct log file
        /// </summary>
        private void Initialize()
        {
            log.Debug("");
            ReturnCode returnValue;

            try
            {                             
                // Open current log file
                returnValue = this.OpenCurrentLogFile(globalOptions.LogDirectory);
            }
            catch
            {
                throw;
            }

        }

         #endregion


        #region Options Management

        /// <summary>
        /// Set the options for the instance on the fly
        /// </summary>
        /// <param name="OptionsObject"></param>
        public void SetOptions(aaLogReaderOptions OptionsObject)
        {
            globalOptions = OptionsObject;
        }

        #endregion

        #region File Management

        /// <summary>
        /// Open a log file specified by file path
        /// </summary>
        /// <param name="LogFilePath">Complete file path to log file</param>
        /// <returns></returns>
        public ReturnCode OpenLogFile(string LogFilePath)
        {
            log.Debug("");
            ReturnCode localReturnCode;

            try
            {
                localReturnCode.Status = false;
                localReturnCode.Message = "";

                log.Debug("LogFilePath - " + LogFilePath);
                
                // Verify we have a file path
                if (LogFilePath != "")
                {
                    log.Info("Opening log file " + LogFilePath);

                    // Save the log path
                    this.currentLogFilePath = LogFilePath;

                    // Open up a filestream.  Make sure we access in read only and also allow others processes to read/write while we have it open
                    this.globalFileStream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    if ((this.globalFileStream.CanRead) && (this.globalFileStream.Length > 0))
                    {
                        log.Info("Opened log file " + LogFilePath);
                        
                        // If opening the file was a success then go ahead and read in the header
                        this.ReadLogHeader(this.globalFileStream);

                        // Get the return code from the log header read
                        localReturnCode = this.logHeader.ReturnCode;

                        log.Debug("logHeader - " + this.logHeader.ToJSON());
                        log.Debug("localReturnCode - " + localReturnCode);

                    }
                    else
                    {
                        throw new aaLogReaderException("Can not open log file " + LogFilePath);
                    }
                }
                else
                {
                    throw new aaLogReaderException("Attempted to open log file with blank path");
                }
            }
            catch
            {
                throw;
            }

            return localReturnCode;
        }

        /// <summary>
        /// Open the latest log file in a specified directory or the default directory
        /// </summary>
        /// <param name="LogDirectory">Directory to inspect for latest log file</param>
        /// <returns></returns>
        public ReturnCode OpenCurrentLogFile(string LogDirectory = "")
        {
            log.Debug("");
            ReturnCode localReturnCode;
            
            try
            {                
                if (LogDirectory == "")
                {
                    LogDirectory = this.GetLogDirectory();
                }

                log.Debug("LogDirectory - " + LogDirectory);

                localReturnCode = this.OpenLogFile(this.LatestFileInPath(LogDirectory, "*.aalog"));

                log.Debug("localReturnCode - " + localReturnCode.ToString());

            }
            catch (Exception ex)
            {
                localReturnCode.Status = false;
                localReturnCode.Message = ex.ToString();
                throw;
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
            string fullFileName = "";

            try
            {
                // Get directory info for the referenced directories
                DirectoryInfo localDirectoryInfo = new DirectoryInfo(Path);

                // Get the last written file in the directory
                fullFileName = localDirectoryInfo.GetFiles(FileSearchPattern).OrderByDescending(f => f.LastWriteTimeUtc).First().FullName;

            }
            catch(Exception ex)
            {
                log.Error(ex);
                fullFileName = "";
            }

            return fullFileName;
        }

		/// <summary>
		/// Close the currently open log file
		/// </summary>
		/// <returns></returns>
        public ReturnCode CloseCurrentLogFile()
		{
            log.Debug("");
            ReturnCode localReturnCode;

            localReturnCode.Status = true;
            localReturnCode.Message = "";

			try
			{
                // Close the global file stream to cleanup
                if (this.globalFileStream != null)
                {                    
				    this.globalFileStream.Close();
                    log.Info("Closed log file " + this.currentLogFilePath);
                }
			}
			catch (Exception ex)
			{
				localReturnCode.Status = false;
                localReturnCode.Message = ex.ToString();
                throw;			
			}
            return localReturnCode;
		}
        
        #endregion

        #region Log Record Functions

        /// <summary>
        /// Read the log file header with default options
        /// </summary>
        /// <returns></returns>
        public LogHeader ReadLogHeader()
        {
            log.Debug("");
            if (this.logHeader != null)
            {
                return this.logHeader;
            }
            else
            {
                return this.ReadLogHeader(this.globalFileStream, false);
            }
        }
        
        /// <summary>
        /// Read the log file header from the currently opened filestream
        /// </summary>
        /// <param name="logFileStream">Currently opened file stream from the log file</param>
        /// <param name="ForceReread">Force a reread of the header even if the header is not currently null</param>
        /// <returns></returns>
        public LogHeader ReadLogHeader(FileStream logFileStream, bool ForceReread = false)
        {
            log.Debug("");
            int readResult;
            LogHeader localHeader = new LogHeader();
            int workingPosition = 0;
            byte[] byteArray = new byte[1];

            try
            {            
               /*
                * If we are not explicitly forcing a reread then look to see 
                * if the header is null.  If it is not that means we've already read the 
                * header so just return that.  If it is null then continue on with
                * the logic
                */
                
                if(!ForceReread)
                {
                    if (this.logHeader != null)
                    {
                        return this.logHeader;
                    }
                }

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

                // Log the actual information to the debug for review later
                //log.Debug("Header Byte Data : " + GetStringFromBytes(byteArray, 0, byteArray.Length - 1));
                
                string headerString = GetStringFromBytes(byteArray, 0, headerLength);

                // Start to pick out the values

                // Start Message Number
                workingPosition = 20;
                localHeader.MsgStartingNumber = BitConverter.ToUInt64(byteArray, workingPosition);
                
                // Message Count
                workingPosition = 28;
                localHeader.MsgCount = (ulong)BitConverter.ToUInt32(byteArray, workingPosition);
                
                // Last Message Number
                localHeader.MsgLastNumber = Convert.ToUInt64(decimal.Subtract(new decimal(checked(localHeader.MsgStartingNumber + localHeader.MsgCount)), decimal.One));
                
                // Start and End Time
                workingPosition = 32;
                localHeader.StartDateTime = this.GetDateTimeFromByteArray(byteArray, workingPosition);
                workingPosition = 40;
                localHeader.EndDateTime = this.GetDateTimeFromByteArray(byteArray, workingPosition);

                // Offset for the first lastRecord
                workingPosition = 48;
                localHeader.OffsetFirstRecord = (int)BitConverter.ToUInt32(byteArray, workingPosition);

                // Offset for the last lastRecord
                workingPosition = 52;
                localHeader.OffsetLastRecord = (int)BitConverter.ToUInt32(byteArray, workingPosition);

                // Computer Name
                workingPosition = 56;
                localHeader.ComputerName = this.GetSingleStringFieldFromByteArray(byteArray, workingPosition);

                // Session
                workingPosition = workingPosition + (localHeader.ComputerName.Length * 2) + 2;
                localHeader.Session = localHeader.ComputerName = this.GetSingleStringFieldFromByteArray(byteArray, workingPosition);

                // Previous File Name
                workingPosition = workingPosition + (localHeader.Session.Length*2) + 2;
                localHeader.PrevFileName = this.GetSingleStringFieldFromByteArray(byteArray, workingPosition);

                //HostFQDN
                localHeader.HostFQDN = this.GetFQDN();

                log.Debug("Local Header - " + localHeader.ToJSON());

                localHeader.ReturnCode.Status = true;
                localHeader.ReturnCode.Message = "";

            }
            catch(Exception ex)
            {
                
                this.returnCloseValue = this.CloseCurrentLogFile();

                localHeader.ReturnCode.Status = false;
                localHeader.ReturnCode.Message = ex.Message;
                
                throw;
            }
            finally
            {
                // Set the log header to this locaheader we have calculated
                this.logHeader = localHeader;
            }

            return localHeader;
        }

        /// <summary>
        /// Read a log lastRecord that starts at the specified offset
        /// </summary>
        /// <param name="FileOffset">Offset for the current file stream</param>
        /// <param name="MessageNumber">Passed message number to set on the log lastRecord.  This should be calculated from external logic</param>
        /// <returns></returns>
        private LogRecord ReadLogRecord(int FileOffset, ulong MessageNumber = 0)
        {
            log.Debug("");

            log.Debug("FileOffset - " + FileOffset.ToString());
            log.Debug("MessageNumber - " + MessageNumber.ToString());

            int recordLength = 0;
            LogRecord localRecord = new LogRecord();
            byte[] byteArray = new byte[1];
            int workingOffset = 0;

            try
            {

                // Initialize the return status
                localRecord.ReturnCode.Status = false;
                localRecord.ReturnCode.Message = "";

                // Initialize working position
                workingOffset = 0;

                // Check to make sure we can even read from the file
                if(!globalFileStream.CanSeek)
                {
                    throw new aaLogReaderException("Log file not open for reading");
                }

                // Go to the spot in the file stream specified by the offset
                this.globalFileStream.Seek((long)FileOffset, SeekOrigin.Begin);

                // Make sure we have at least 8 byteArray of data to read before hitting the end
                byteArray = new byte[8];
                if (this.globalFileStream.Read(byteArray, 0, 8) == 0)
                {
                    throw new aaLogReaderException("Attempt to read past End-Of-Log-File");
                }

                //Get the first 4 byteArray of data byte array that we just retrieved.  
                // This tells us how long this lastRecord is.
                recordLength = BitConverter.ToInt32(byteArray, 4);

                // If the lastRecord length is not > 0 then bail on the function, returning an empty lastRecord with status code
                if(recordLength <= 0)
                {
                    throw new aaLogReaderException("Record Length is 0");
                }

                //Go back and reset to the specified offset
                this.globalFileStream.Seek((long)FileOffset, SeekOrigin.Begin);

                //Recreate the byte array with the proper length
                byteArray = new byte[checked(recordLength + 1)];

                //Now get the actual lastRecord data into the byte array for processing
                this.globalFileStream.Read(byteArray, 0, recordLength);

                // Record Length.  We've already calculated this so just use internal variable
                localRecord.RecordLength = recordLength; 

                // Offset to Previous Record.
                workingOffset = 8;
                localRecord.OffsetToPrevRecord = (int)BitConverter.ToUInt32(byteArray, workingOffset);

                // Offset to Nex Record
                localRecord.OffsetToNextRecord = checked(FileOffset + recordLength);

                // Session ID
                workingOffset = 12;
                localRecord.SessionID = this.GetSessionIDSegments(byteArray, (long)workingOffset).SessionID; //this.sessionSeg.SessionID;

                // Process ID
                workingOffset = 16;
                localRecord.ProcessID = (int)BitConverter.ToUInt32(byteArray, workingOffset);

                // Thread ID
                workingOffset = 20;
                localRecord.ThreadID = (int)BitConverter.ToUInt32(byteArray, workingOffset);

                // Date Time
                workingOffset = 24;
                localRecord.EventDateTime = this.GetDateTimeFromByteArray(byteArray, workingOffset);

                // Log Flag
                workingOffset = 32;
                localRecord.LogFlag = this.GetSingleStringFieldFromByteArray(byteArray, (long)workingOffset);

                /* 
                 * Calc new working offset based on length of previously retrieved field.
                 * Can't forget that we're dealing with Unicode so we have to double the 
                 * length to find the proper byte offset
                 */

                workingOffset = workingOffset + (localRecord.LogFlag.Length * 2) + 2;
                localRecord.Component = this.GetSingleStringFieldFromByteArray(byteArray, (long)workingOffset);

                workingOffset = workingOffset + (localRecord.Component.Length * 2) + 2;
                localRecord.Message = this.GetSingleStringFieldFromByteArray(byteArray, (long)workingOffset);

                workingOffset = workingOffset + (localRecord.Message.Length * 2) + 2;
                localRecord.ProcessName = this.GetSingleStringFieldFromByteArray(byteArray, (long)workingOffset);

                // Get the host from the header information
                localRecord.HostFQDN = ReadLogHeader().HostFQDN;

                localRecord.ReturnCode.Status = true;
                localRecord.ReturnCode.Message = "";
                // Set the message number on the lastRecord based on the value passed
                localRecord.MessageNumber = MessageNumber;

            }
            catch (System.ApplicationException saex)
            {                
                // If this is a past the end of file message then handle gracefully
                if(saex.Message == "Attempt to read past End-Of-Log-File")
                {               
                    this.returnCloseValue = this.CloseCurrentLogFile();

                    // Re-init the lastRecord to make sure it's totally blank.  Don't want to return a partial lastRecord
                    localRecord = new LogRecord();
                    localRecord.ReturnCode.Status = false;
                    localRecord.ReturnCode.Message = saex.Message;
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }

            // Set the last lastRecord read to this one.
            this.lastRecordRead = localRecord;

            // Return the working lastRecord
            return localRecord;
        }

        /// <summary>
        /// Get the first lastRecord in the log as specified by the OffsetFirstRecord in the header.
        /// </summary>
        /// <returns></returns>
        public LogRecord GetFirstRecord()
		{
            log.Debug("");
            LogRecord localRecord = new LogRecord();

			if (this.logHeader.OffsetFirstRecord == 0)
			{
				this.lastRecordRead.ReturnCode.Status = false;
                this.lastRecordRead.ReturnCode.Message = "";
			}
			else
			{
                localRecord = this.ReadLogRecord(this.logHeader.OffsetFirstRecord, this.logHeader.MsgStartingNumber);
			}

            return localRecord;
		}

        /// <summary>
        /// Get the last lastRecord in the log as specified by the OffsetLastRecord in the header.
        /// </summary>
        /// <returns></returns>
		public LogRecord GetLastRecord()
		{
            log.Debug("");
            LogRecord localRecord = new LogRecord();

			if (this.logHeader.OffsetLastRecord == 0)
			{
                localRecord.ReturnCode.Status = false;
                localRecord.ReturnCode.Message = "Offset to Last Record is 0.  No record returned.";
			}
			else
			{
                localRecord = this.ReadLogRecord(this.logHeader.OffsetLastRecord, this.logHeader.MsgLastNumber);
			}

            return localRecord;

        }
        
        /// <summary>
        /// Get the next lastRecord in the log file
        /// </summary>
        /// <returns></returns>
        public LogRecord GetNextRecord()
		{
            log.Debug("");
            LogRecord localRecord;
            ulong LastMessageNumber;

                if (this.lastRecordRead.OffsetToNextRecord == 0)
                {
                    // We haven't read any records yet so just get the first lastRecord
                    return this.GetFirstRecord();
                }
                else
                {
                    // Cache the last message number
                    LastMessageNumber = this.lastRecordRead.MessageNumber;

                    // If we are already at the end of the log file
                    if (LastMessageNumber >= this.logHeader.MsgLastNumber)
                    {
                        throw new aaLogReaderException("Attempt to read past End-Of-Log-File");
                    }

                    // Read the lastRecord based off offset information from last lastRecord read
                    localRecord = this.ReadLogRecord(this.lastRecordRead.OffsetToNextRecord, Convert.ToUInt64(decimal.Add(new decimal(LastMessageNumber), decimal.One)));
                }

            return localRecord;
		}

        /// <summary>
        /// Get the lastRecord immediately previous to the current lastRecord in the log file.  This call will swap to previous log files as required.
        /// </summary>
        /// <returns></returns>
		public LogRecord GetPrevRecord()
		{
            log.Debug("");
            ulong LastMessageNumber;
            LogRecord localRecord = new LogRecord();

			try
            {                
                if (this.lastRecordRead.OffsetToPrevRecord != 0)
				{
                    log.Debug("this.lastRecordRead.OffsetToPrevRecord != 0");

                    // Cache the last message number
                    LastMessageNumber = this.lastRecordRead.MessageNumber;
                    // Read the lastRecord based off offset information from last lastRecord read

                    localRecord = this.ReadLogRecord(this.lastRecordRead.OffsetToPrevRecord, Convert.ToUInt64(decimal.Subtract(new decimal(LastMessageNumber), decimal.One)));

                    // Calculate the new message number
                    //localRecord.MessageNumber = Convert.ToUInt64(decimal.Subtract(new decimal(LastMessageNumber), decimal.One));

                    //this.lastRecordRead.ReturnCode.Status = true;
                    //this.lastRecordRead.ReturnCode.Message = "";
				}
                // Check to see if we are at the beginning of if there is another log file we can connect to
				else if (System.String.Compare(this.logHeader.PrevFileName, "", false) == 0)  
				{
                    log.Debug("this.lastRecordRead.OffsetToPrevRecord = 0 AND this.logHeader.PrevFileName == 0");

                    localRecord.ReturnCode.Status = false;
                    // Beginning of Log
                    localRecord.ReturnCode.Message = "BOL";
				}
				else
				{
                    log.Debug("Close current log file");

                    // Close the currently opened log file
                    this.globalFileStream.Close();
                    
                    string newPreviousLogFile = string.Concat(new string[] {this.GetLogDirectory(), "\\", this.logHeader.PrevFileName });

                    log.Debug("newPreviousLogFile - " + newPreviousLogFile);

                    if(this.OpenLogFile(newPreviousLogFile).Status)
                    {
                        localRecord = this.GetLastRecord();
                        log.Debug("localRecord.ReturnCode.Status - " + localRecord.ReturnCode.Status);
                    }

                    {
                        throw new aaLogReaderException("Error attempting to open previous log file.");
                    }
				}
			}
			catch
			{                
                throw;				
			}

            return localRecord;
		}

        /// <summary>
        /// Get all unread messages starting from the last lastRecord working backwards.
        /// </summary>
        /// <param name="maximumMessages">Maximum number of messages to return</param>
        /// <param name="messagePatternToStop">Message pattern to match for ending search</param>
        /// <param name="IgnoreCacheFile">Ignore the cache file and read all messages up to maximum or message pattern</param>
        /// <returns></returns>
        public List<LogRecord>GetUnreadRecords(int maximumMessages = 1000, string messagePatternToStop = "", bool IgnoreCacheFile = false)        
        {
            try
            {
                log.Debug("");
                log.Debug("maximumMessages - " + maximumMessages.ToString());
                log.Debug("messagePatternToStop - " + messagePatternToStop);

                string cacheFilePath = this.GetStatusCacheFilePath();

                log.Debug("cacheFilePath - " + cacheFilePath);

                ulong lastMessageNumber = 0;

                // If the cache file exists and we should not ignore it
                if (File.Exists(cacheFilePath) && !IgnoreCacheFile)
                {                
                    // Get the JSON from the file
                    string objectJSONFromCacheFile = File.ReadAllText(this.GetStatusCacheFilePath());
                    // Deserialize into the Log Record
                    LogRecord lastRecordFromCacheFile = JsonConvert.DeserializeObject<LogRecord>(objectJSONFromCacheFile);

                    log.Debug("lastRecordFromCacheFile - " + lastRecordFromCacheFile.ToJSON());
                
                    // Get the last message number from the retrieved lastRecord if it's available
                    if (lastRecordFromCacheFile != null)
                    {
                        lastMessageNumber = lastRecordFromCacheFile.MessageNumber;
                    }

                    log.Debug("lastMessageNumber - " + lastMessageNumber.ToString());

                }

                return this.GetUnreadRecords(lastMessageNumber, maximumMessages, messagePatternToStop);

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get all unread messages starting from the last lastRecord and stopping at the last read message number.
        /// </summary>
        /// <param name="lastReadMessageNumber">Last message number previously read.</param>
        /// <param name="maximumMessages">Maximum number of messages to return</param>
        /// <param name="messagePatternToStop">Message pattern to match for ending search</param>
        /// <returns></returns>
        private List<LogRecord> GetUnreadRecords(ulong lastReadMessageNumber, int maximumMessages = 1000, string messagePatternToStop = "")
        {
            List<LogRecord> logRecordList = new List<LogRecord>();
            LogRecord localRecord; 
            bool getAnotherRecord;
            
            try
            {
                log.Debug("");
                log.Debug("lastReadMessageNumber - " + lastReadMessageNumber.ToString());
                log.Debug("maximumMessages - " + maximumMessages.ToString());
                log.Debug("messagePatternToStop - " + messagePatternToStop);

                //If the latest file in the directory does not match the file we are currently working on
                if (this.currentLogFilePath != this.LatestFileInPath(this.GetLogDirectory(),"*.aalog"))
                {
                    log.Info("Latest log file has changed.  Forcing a reread.");

                    // Force a reread
                    this.OpenCurrentLogFile();
                }
                
                // Force a reread of the header so we know the latest values
                this.ReadLogHeader(this.globalFileStream, true);

                log.Debug("logHeader.MsgLastNumber - " + this.logHeader.MsgLastNumber.ToString());
                log.Debug("lastReadMessageNumber - " + lastReadMessageNumber);

                // Check the header to see if any new records have been added
                if(this.logHeader.MsgLastNumber > lastReadMessageNumber)
                { 
                    // Start with the last lastRecord
                    localRecord = this.GetLastRecord();

                    log.Debug("GetLastRecord Message Number - " + localRecord.MessageNumber);
                    log.Debug("GetLastRecord localRecord.ReturnCode.Status - " + localRecord.ReturnCode.Status);

                    // If we get a lastRecord then add to the list and start iterating
                    if (localRecord.ReturnCode.Status)
                    {
                        logRecordList.Add(localRecord);

                        /* If the last retrieval was good 
                         * and we have an offset for previous lastRecord 
                         * and we haven't passed the maximum lastRecord count limit
                         * retrieve the next previous lastRecord
                         */

                        getAnotherRecord = this.ShouldGetNextRecord(localRecord, logRecordList.Count, lastReadMessageNumber, maximumMessages, messagePatternToStop);

                        //getAnotherRecord = localRecord.ReturnCode.Status && (localRecord.OffsetToNextRecord > 0) && (localRecord.MessageNumber > lastReadMessageNumber) && (logRecordList.Count < maximumMessages);

                        log.Debug("getAnotherRecord - " + getAnotherRecord);

                        while (getAnotherRecord)
                        {
                            localRecord = this.GetPrevRecord();

                            log.Debug("GetPrevRecord localRecord.ReturnCode.Status - " + localRecord.ReturnCode.Status);

                            if (localRecord.ReturnCode.Status)
                            {
                                logRecordList.Add(localRecord);
                            }

                            // Calculate if we should get another lastRecord
                            //getAnotherRecord = localRecord.ReturnCode.Status && (localRecord.OffsetToNextRecord > 0) && (localRecord.MessageNumber > lastReadMessageNumber) && (logRecordList.Count < maximumMessages);
                            
                            getAnotherRecord = this.ShouldGetNextRecord(localRecord, logRecordList.Count, lastReadMessageNumber, maximumMessages, messagePatternToStop);

                            log.Debug("getAnotherRecord - " + getAnotherRecord);
                        }

                        // Write out the cache file if we read records
                        this.WriteStatusCacheFile();
                    }                    
                }

            }
                catch
            {
                throw;
            }

            return logRecordList;

        }

        private bool ShouldGetNextRecord(LogRecord lastRecord,int logRecordCount, ulong lastReadMessageNumber, int maximumMessages, string messagePatternToStop)
        {
            bool returnValue = false;

            try
            {
                log.Debug("");
                log.Debug("lastReadMessageNumber - " + lastReadMessageNumber.ToString());
                log.Debug("maximumMessages - " + maximumMessages.ToString());
                log.Debug("messagePattern - " + messagePatternToStop);

                /* If the last retrieval was good 
                * and we have an offset for previous lastRecord 
                * and we haven't passed the maximum lastRecord count limit
                * retrieve the next previous lastRecord
                */
                returnValue = (lastRecord.ReturnCode.Status && (lastRecord.OffsetToNextRecord > 0) && (lastRecord.MessageNumber > (lastReadMessageNumber+1)) && (logRecordCount < maximumMessages));

                /* If the message pattern is not blank then apply a regex to see if we get a match 
                 * If we match then that means this is the last lastRecord we should retrieve so return false
                 */
                if(returnValue && messagePatternToStop != "")
                {
                    returnValue &= !System.Text.RegularExpressions.Regex.IsMatch(lastRecord.Message, messagePatternToStop, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
                returnValue = false;                
            }

            return returnValue;
        }

        #endregion

        #region Private Helper Functions
        
        /// <summary>
        /// Translate a byte array to a date time
        /// </summary>
        /// <param name="byteArray">Byte array containing lastRecord data</param>
        /// <param name="startingOffset">Starting offset for the data field</param>
        /// <returns></returns>
        private DateTime GetDateTimeFromByteArray(byte[] byteArray, long startingOffset)
        {

            DateTime localDate;

            try
            {
                // DateTime is an 8 byte value with a Low Byte and High Byte.
                // We use a custom structure called file time with Low Byte and High Byte Elements
                // Then in the FileTime struct we calculate the value by combining the high byte and low byte
                this.sTime.dwLowDateTime = BitConverter.ToUInt32(byteArray, (int)startingOffset);
                this.sTime.dwHighDateTime = BitConverter.ToUInt32(byteArray, checked((int)startingOffset + 4));

                localDate = DateTime.Parse((DateTime.FromFileTime((long)this.sTime.value).ToString("MM/dd/yyyy hh:mm:ss.fff tt")));
            }
            catch
            {
                throw;
            }

            return localDate;
        }
		
        /// <summary>
        /// Get a single string field starting at a specified offset.
        /// </summary>
        /// <param name="byteArray">Byte array containing lastRecord data</param>
        /// <param name="startingOffset">Starting offset for the data field</param>
        /// <returns></returns>
        private string GetSingleStringFieldFromByteArray(byte[] byteArray, long startingOffset)
        {            
            string returnValue;
            int fieldLength;
            
            try
            {
                // Initialize to blank
                returnValue = "";

                // Get the length of the string field
                fieldLength = this.GetStringFieldInByteArrayLength(byteArray,startingOffset );
                
                if(fieldLength > 0)
                {                    
                    /* Get the string value from the byte array
                     * We are using this method instead of Encoding.Unicode.GetString
                     * because in the development environment this function returned strang
                     * results in the form of Chinese characters when the value should
                     * have been traditional EN-US text.
                     */ 
                    
                    returnValue = this.GetStringFromBytes(byteArray, (int)startingOffset, fieldLength);

                }
                else
                {
                    /*
                     * If field is zero length then just return a blank string.  
                     * This should alert downstream logic that something's not quite right
                     * but no need in complicating this function with handling of that error
                     */
                   
                    returnValue = "";
                }

            }
            catch
            {                
                throw;                
            }

            return returnValue;

        }

        /// <summary>
        /// Cast an array of byteArray to a string
        /// </summary>
        /// <param name="byteArray">Byte array containing lastRecord data</param>
        /// <param name="startingOffset">Starting offset for the data field</param>
        /// <param name="Length">Length of field</param>
        /// <returns></returns>
        private string GetStringFromBytes(byte[] byteArray, int StartOffset, int Length)
        {
            // Credits: http://stackoverflow.com/questions/472906/converting-a-string-to-byte-array-without-using-an-encoding-byte-by-byte           
            char[] chars = new char[Length / sizeof(char)];
            System.Buffer.BlockCopy(byteArray, StartOffset, chars, 0, Length);
            return new string(chars);
        }  

        /// <summary>
        /// Get the length of a string field in a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing lastRecord data</param>
        /// <param name="startingOffset">Starting offset for the data field</param>
        /// <returns></returns>
        private int GetStringFieldInByteArrayLength(byte[] byteArray, long startingOffset)
		{
            
            int returnValue;
            ushort currentIntValue;
            int calculatedLength;
            int workingIndex;

            /* 
             * The concept of this function is fairly basic
             * Start moving through the byte array from the start index 
             * and keep going until you hit an int (2 byteArray), value = 0.  
             * This signifies the end of the field
             */

            try
            {
                // Initialize the calculated length to 0
                calculatedLength = 0;

                // Cast startingOffset back down to an int
                //int startingIndex = checked((int)startingOffset);
                workingIndex = checked((int)startingOffset);

                // Get the first byte value
                currentIntValue = BitConverter.ToUInt16(byteArray, workingIndex);

                // Loop until we hit a byte value of 0
                while (currentIntValue != 0)
                {
                    currentIntValue = BitConverter.ToUInt16(byteArray, workingIndex);

                    if (currentIntValue == 0)
                    {
                        // Found the 0 value.  Bail out.
                        continue;
                    }

                    // Add to 2 byteArray to the the length and keep going.
                    // We add 2 byteArray because in the previous statement we are working with 16 bit integer
                    calculatedLength = calculatedLength + 2;
                    //Index 
                    workingIndex = checked(workingIndex + 2);
                }

                returnValue = checked(calculatedLength);
            }
            catch
            {                
                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// Extract SessionID segments from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing lastRecord data</param>
        /// <param name="startingOffset">Starting offset for the data field</param>
        /// <returns></returns>
        private SessionIDSegments GetSessionIDSegments(byte[] byteArray, long startingOffset)
        {
            SessionIDSegments returnValue = new SessionIDSegments();

            // Session ID segment is just 4 8 byte values in a row, but in reverse order
            try
            {
                returnValue.Segment1 = byteArray[startingOffset + 3];
                returnValue.Segment2 = byteArray[startingOffset + 2];
                returnValue.Segment3 = byteArray[startingOffset +1 ];
                returnValue.Segment4 = byteArray[startingOffset];
            }
            catch
            {
                throw;
            }
        
            return returnValue;

        }

        /// <summary>
        /// Write the status cache file with the last record read
        /// </summary>
        /// <returns></returns>
        public bool WriteStatusCacheFile()
        {
            try
            {
                return this.WriteStatusCacheFile(this.GetLastRecord());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Write a text file out with metadata that can be used if the application is closed and reopened to read logs again
        /// </summary>
        public bool WriteStatusCacheFile(LogRecord CacheRecord)
        {
            log.Debug("");
            log.Debug("CacheRecord - " + CacheRecord.ToJSON());

            try
            {            
                System.IO.File.WriteAllText(this.GetStatusCacheFilePath(), this.GetLastRecord().ToJSON());
                return true;
            }
            catch
            {
                throw;
            }            
        }

        /// <summary>
        /// Read the contents on the StatusCacheFile into a log lastRecord.
        /// </summary>
        /// <returns></returns>
        public LogRecord ReadStatusCacheFile()
        {
            log.Debug("");
            LogRecord localRecord = new LogRecord();

            try
            {
                localRecord = JsonConvert.DeserializeObject<LogRecord>(File.ReadAllText(this.GetStatusCacheFilePath()));                
            }
            catch(Exception ex)
            {
                log.Error(ex);
                throw;
            }

            return localRecord;
        }

        /// <summary>
        /// Calculate the path to the cache file
        /// </summary>
        /// <returns></returns>
        private string GetStatusCacheFilePath()
        {
            log.Debug("");
            string returnValue = "";

            try
            {
                string cacheFileName = "";

                // Check the global options to determine the features that have been configured

                if (globalOptions.CacheFileNameCustom != "")
                {
                    cacheFileName = globalOptions.CacheFileNameCustom;
                }
                else if (globalOptions.CacheFileAppendProcessNameToBaseFileName)
                {
                    cacheFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + globalOptions.CacheFileBaseName;
                }
                else
                {
                    cacheFileName = globalOptions.CacheFileBaseName;
                }

                returnValue = Path.GetDirectoryName(this.currentLogFilePath) + "\\" + cacheFileName;
            }
            catch
            {                
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
                if (System.IO.Directory.Exists(globalOptions.LogDirectory))
                {
                    returnValue = globalOptions.LogDirectory;
                }
                else
                {
                    returnValue = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\ArchestrA\Framework\Logger", "LogDir", globalOptions.LogDirectory).ToString();
                }
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }

        /// <summary>
        /// Get the fully qualified domain name of the local machine
        /// </summary>
        /// <returns></returns>
        private string GetFQDN()
        {
            log.Debug("");

            // Credits: http://stackoverflow.com/questions/804700/how-to-find-fqdn-of-local-machine-in-c-net

            string hostName;

            try
            {
                string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                hostName = Dns.GetHostName();

                if ((!hostName.EndsWith(domainName) && (domainName != ""))) // if hostname does not already include domain name
                {
                    hostName += "." + domainName;   // add the domain name part
                }
             }
            catch
            {
                hostName = "";
            }

            return hostName;                    // return the fully qualified name
        }
        
        #endregion

    }
}