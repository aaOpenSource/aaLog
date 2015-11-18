aaLog
=================

A library and some simple example projects to read binary log files produced by System Platform components.

## Motivation

The logging system provided by System Platform is quite nice but there has always been one huge weakness.  You could only look at the log files for a single machine at a time.  In theory you may be able to copy the log files together in a single directory but this seemed cludgy and not very scalable.

A few years ago at the software conference in Dallas I had a conversation with a Wonderware engineer that had built a tool to read the log files and forward them to various different formats: Syslog, CSV, SQL Server, etc.  I thought this was a brilliant idea but unfortunately it was never productized so the the general customer base could benefit.  After some time I decided I would do something about it.

So with a little patience, ingenuity, and an existing DLL in the system I was able to reverse engineer the format of the log files.  No, I'm not a genius or a hacker, just spent a little special time with the files and the code before they agreed to show me their secrets.  From that I wrote a library, based heavily on the original system provided one, with some nice improvements and additions.  

The first application that I plan to use the library for is an ultra simple console app that when run will output the latest unread messages to a STDOUT console.  From there you can configure your own application to consume this input for whatever purpose you wish.  I plan to create a [Splunk Universal Forwarder](http://docs.splunk.com/Splexicon:Universalforwarder) configuration to take this output and send to a Splunk Enterprise indexing system.  

## Usage

Using the library with all the defaults could not be simpler. In two lines you can get an object of type list for all unread records.  You can explore the code int he library to see how we manage the return list with a default max on the messages that can be overwritten if desired.  This example is directly from the Splunk Console example program 

```c#
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
```

In the example I am outputting the records in KVP (Key/Value Pair) format but I also have a method to output to JSON. What other formats do you need?  Just add them to the LogRecord class.  And of course contribute back to the library when you do :-).  

##The Magic
You may ask, what's the magic sauce behind tracking unread records so easily?  If you look through the library carefully you will see where I create a cache file in the logs directory every time I read log records.  This cache file is just a JSON dump of the last record read.  When I go to determine all unread records I read in this cache file and look at the message number.  This tells me how far back to go.  From there I start by reading the last record and work my way back until I find the previous message number or the maximum number of records to return.

##A Note on Performance
I haven't done exhaustive scientific testing but I have done enough to know that performance simply isn't an issue.  I ran a number of tests reading the last 10,000 records.  On average this took about 200 ms.  For those who are mathematically challenged (like me sometimes) that means we are clipping at 50,000 records/second pace for reading.  This was done inside a Win2K8R2 VM with 6 GB of RAM.  Doing shorter reads the rate tends to go down as you are spending more time on the up front stuff like opening the file stream and reading the header as a portion of the overall cost.  Your output format will heavily influence the overall performance but for my money if you are blasting more than 50,000 log records a second then you have really serious issues in your environment that need to be addressed first before you worry about log consolidation and analysis.   

##Example Projects

###aaLogConsoleTester
A basic console app to demonstrate reading unread records.

###aaLogGUITester
A simple GUI app that will send data to Splunk over a TCP connection.

###aaLogSplunkConsole
A basic console app to send output to the STDOUT for consumption by Splunk or any other processing engine you wish.

###aaLogReaderModularInput
A Splunk modular input to simplify forwarding Archestra logs to [Splunk](http://www.splunk.com/).  To read about modular inputs for C# start with this link -
[How to create modular inputs in Splunk SDK for C# v2.x](http://dev.splunk.com/view/csharp-sdk-pcl/SP-CAAAEY3)

If you think at first blush that creating a modular input is a lot more work than just streaming data over TCP in KVP format you would be correct.  However, after you work through all of the details you will see that a modular input provides a package that provides for a much more consistent and repeatable experience for the end user.  It is important to note that you will require a Splunk Forwarder to be installed on the machine where you are collecting logs.  If this is not feasible then a standalone EXE that doesn't require an installation and run as a service might be a better option.

###aalogWebAPI
A project to allow for acessing log file data as on ODATA feed over HTTP.  Currently there is only a single call `GetLogRecords` that calls `GetUnreadRecords` from the log reader library.

## Platforms

All projects were compiled against .Net 4.0 but in concept you can probably go pretty far back as I don't believe I've used anything too exotic with the exception of Nuget packages for JSON parsing and Logging.

##Path Forward
From here I would like for others to pick up the core and extend it with more sophisticated features as well as building full blown applications.  You can see a lot of my focus at the moment is getting these logs into Splunk.  However, there is no reason the community can't write more forwarders to other storage platforms.  My preference would be to keep the core library relatively clean and write your forwarders as separate projects that utilize the library. 

## Build Notes
For some reason, probably 100% due to my lack of understanding, when you Git Clone, the aaLogReader project does not build because it is missing a reference to log4Net.  I have found the easiest way to resolve this is to click on the missing reference in the list, change Copy Local to True and then rebuild.  This will go out and restore the nuget package.  Another method is to manage the NuGet references and uninstall/reinstall log4Net.  I have also had intermittent issues with the GuiTester complaining about mismatched assemblies related to the JSON package.  You can safely upgrade to version 6.08 if for some reason your version when you pull down is 6.06. If you have any more questions please feel free to give me a shout and I'll do my best to help/make up for my lack of mastery in the subject :-)

Another issue I've found is that the reference JSON files for the unit tests are not getting copied to the output directory on build.  You might have to set this on your copy of the solution.

##Testing
There are a small number of unit tests written using NUnit.  I would like to make this a priority in the coming months so we can maintain the code quality as more people contribute to the project.

##Chat Room

[![Join the chat at https://gitter.im/aaOpenSource/aaLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/aaOpenSource/aaLog)

## TODO List
Check out the [Issues](/../../issues) List

##Contributing
Check out the [Contributing](/CONTRIBUTING.MD) file

## Contributors

* [Andy Robinson](mailto:andy@phase2automation.com), Principal of [Phase 2 Automation](http://phase2automation.com).
* See list of [Contributors](/../../graphs/contributors) on the repo for others

## Shoutouts to ma Peeps
Thanks to Brian Gilmore (@BrianMGilmore) and Terry McCorkle (@0psys) of @splunk for validating the fact that this work will be very useful in supporting some of the bigger initiatives that have going on at Splunk, specifically around log collection in ICS for security. 

Also another huge piece of credit to my undercover elves in the 949 that inspired this work and might want to re-join the effort now that it's in the wild.

## License

MIT License. See the [LICENSE file](/LICENSE) for details.
