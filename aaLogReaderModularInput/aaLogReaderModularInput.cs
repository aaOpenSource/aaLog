using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Splunk.ModularInputs;
using aaLogReader;

namespace aaLogReaderModularInput
{
    public class Program : ModularInput
    {
        /// <summary>
        /// Main method which dispatches to ModularInput.Run&lt;T&gt;.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>An exit code.</returns>
        public static int Main(string[] args)
        {
            uint timeout_sec = 60;

            #if DEBUG_VALIDATE
                        return Run<Program>(args, DebuggerAttachPoints.ValidateArguments, timeout_sec);
            #elif DEBUG_STREAMEVENTS
                        return Run<Program>(args, DebuggerAttachPoints.StreamEvents, timeout_sec);
            #else
                        return Run<Program>(args);
            #endif
        }

        /// <summary>
        /// Define a Scheme instance that describes this modular input's behavior. The scheme
        /// will be serialized to XML and printed to stdout when this program is invoked with
        /// the sole argument <tt>--scheme</tt>, which Splunk does when starting up and each time
        /// the app containing the modular input is enabled.
        /// </summary>
        public override Scheme Scheme
        {
            get
            {
                return new Scheme
                {
                    Title = "Archestra Log File Reader",
                    Description = "Read log entries from an Archestra aaLog file",
                    Arguments = new List<Argument>
                    { 
                            new Argument
                            {
                                Name = "logfilepath",
                                Description = "Path to the log file on the local machine-test",
                                DataType = DataType.String,
                                RequiredOnCreate = true
                            },
                            new Argument
                            {
                                Name = "maxmessagecount",
                                Description = "Maximum number of messages to read in a single scan",
                                DataType = DataType.Number,
                                RequiredOnCreate = true,
                                Validation = "is_pos_int('maxmessagecount')"
                            }
                    }
                };

            }
        }

        /// <summary>
        /// Check that the values of arguments specified for a newly created or edited instance of
        /// this modular input are valid. If they are valid, set <tt>errorMessage</tt> to <tt>""</tt>
        /// and return <tt>true</tt>. Otherwise, set <tt>errorMessage</tt> to an informative explanation 
        /// of what makes the arguments invalid and return <tt>false</tt>.
        /// </summary>
        /// <param name="validation">a Validation object specifying the new argument values.</param>
        /// <param name="errorMessage">an output parameter to pass back an error message.</param>
        /// <returns><tt>true</tt> if the arguments are valid and <tt>false</tt> otherwise.</returns>
        public override bool Validate(Validation validation, out string errorMessage)
        {
            bool returnValue = true;            
            errorMessage = "";

            try
            {

                string logfilepath = ((SingleValueParameter)(validation.Parameters["logfilepath"])).ToString();                

                // Verify path is valid
                if (returnValue && !System.IO.Directory.Exists(logfilepath))
                {
                    errorMessage = "LogFilePath " + logfilepath + " does not exist on local machine";
                    return false;
                }

            }
            catch(Exception)
            {
                errorMessage = "Error processing validation logic.";
                returnValue = false;
            }

            return returnValue;
        }

        /// <summary>
        /// Write events to Splunk from this modular input.
        /// </summary>
        /// <remarks>
        /// This function will be invoked once for each instance of the modular input, though that invocation
        /// may or may not be in separate processes, depending on how the modular input is configured. It should
        /// extract the arguments it needs from <tt>inputDefinition</tt>, then write events to <tt>eventWriter</tt>
        /// (which is thread safe).
        /// </remarks>
        /// <param name="inputDefinition">a specification of this instance of the modular input.</param>
        /// <param name="eventWriter">an object that handles writing events to Splunk.</param>
        public override async Task StreamEventsAsync(InputDefinition inputDefinition, EventWriter eventWriter)
        {
            try
            {
                string logfilepath = ((SingleValueParameter)(inputDefinition.Parameters["logfilepath"])).ToString();
                Int32 maxmessagecount = ((SingleValueParameter)(inputDefinition.Parameters["maxmessagecount"])).ToInt32();

                // Initialize the log reader
                aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader(logfilepath);
                
                // Write an entry to the Splunk system log indicating we have initialized
                await eventWriter.LogAsync(Severity.Info, "Initialized Log reader for path " + logfilepath + " and message count " + maxmessagecount.ToString());

                while (true)
                {
                    //Simple call to get all unread records, limiting the return count to max message count
                    List<LogRecord> logRecords = logReader.GetUnreadRecords(maxmessagecount);

                    //await eventWriter.LogAsync(Severity.Debug, "Read " + logRecords.Count.ToString() + " records");
                   
                    // Loop through each lastRecord and send to Splunk
                    foreach (LogRecord record in logRecords)
                    {
                        await eventWriter.QueueEventForWriting(new Event
                        {
                            Stanza = inputDefinition.Name,
                            Data = record.ToKVP()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Eat error message
                eventWriter.LogAsync(Severity.Error, ex.ToString());
            }
        }
    }
}