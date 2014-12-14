aaLog
=================

A library and some simple example projects to read binary log files produced by System Platform components.

## Motivation

The logging system provided by System Platform is quite nice but there has always been one huge weakness.  You could only look at the log files for a single machine at a time.  In theory you may be able to copy the log files together in a single directory but this seemed cludgy and not very scalable.

A few years ago at the software conference in Dallas I had a conversation with a Wonderware engineer that had built a tool to read the log files and forward them to various different formats: Syslog, CSV, SQL Server, etc.  I thought this was a brilliant idea but unfortunately it was never productized so the the general customer base could benefit.  After some time I decided I would do something about it.

So with a little patience, ingenuity, and an existing DLL in the system I was able to reverse engineer the format of the log files.  From that I wrote a library, based on the original system provided one, with some nice improvements and additions.  

The first application that I plan to use the library for is an ultra simple console app that when run will output the latest unread messages to a STDOUT console.  From there you can configure your own application to consume this input for whatever purpose you wish.  I plan to create a [Splunk Universal Forwarder](http://docs.splunk.com/Splexicon:Universalforwarder) configuration to take this output and send to a Splunk Enterprise indexing system.  

## Usage

Using the library with all the defaults could not be simpler. In two lines you can get an object of type list for all unread records.  You can explore the code int he library to see how we manage the return list with a default max on the messages that can be overwritten if desired.  This example is directly from the Splunk Console example program 


	// Instantiate a new log reader object            
	aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader();
	// Get all unread records
    List<LogRecord> logRecords = logReader.GetUnreadRecords();

    // If we have any records then output kvp format to the console
    if(logRecords.Count > 0)
    {
        foreach(LogRecord record in logRecords)
        {
            Console.WriteLine(record.ToKVP());
        }
    }

In the example I am outputting the records in KVP (Key/Value Pair) format but I also have a method to output to JSON. What other formats do you need?  Just add them to the LogRecord class.  And of course contribute back to the library when you do :-).  

##The Magic
You may ask, what's the magic sauce behind tracking unread records so easily?  If you look through the library carefully you will see where I create a cache file in the logs directory every time I read log records.  This cache file is just a JSON dump of the last record read.  When I go to determine all unread records I read in this cache file and look at the message number.  This tells me how far back to go.  From there I start by reading the last record and work my way back until I find the previous message number or the maximum number of records to return.

##Example Projects

###aaLogConsoleTester
A basic console app to demonstrate reading unread records.

###aaLogGUITester
A simple GUI app that will send data to Splunk over a TCP connection.

###aaLogSplunkConsole
A basic console app to send output to the STDOUT for consumption by Splunk or any other processing engine you wish.

## Platforms

All projects were compiled against .Net 4.0 but in concept you can probably go pretty far back as I don't believe I've used anything too exotic with the exception of Nuget packages for JSON parsing and Logging.

##Path Forward
From here I would like for others to pick up the core and extend it with more sophisticated features as well as building full blown applications.  You can see a lot of my focus at the moment is getting these logs into Splunk.  However, there is no reason the community can't write more forwarders to other storage platforms.  My preference would be to keep the core library relatively clean and write your forwarders as separate projects that utilize the library. 

## TODO List

See my current roadmap and overall [TODO list](/TODO.md)

## Contributors

* [Andy Robinson](mailto:andy@phase2automation.com), Principal of [Phase 2 Automation](http://phase2automation.com).

## Shoutouts to ma Peeps
Thanks to Brian Gilmore (@BrianMGilmore) and Terry McCorkle (@0psys) of @splunk for validating the fact that this work will be very useful in supporting some of the bigger initiatives that have going on at Splunk, specifically around log collection in ICS for security. 

## License

MIT License. See the [LICENSE file](/LICENSE) for details.
