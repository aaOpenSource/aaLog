The options file is used to specify a list of options you can use when instantiating the log reader. Or if you like you can load an options file at a later time and push into a current instance of the log reader.

##Base Options
### LogDirectory
The log directory specifies where the code shold look by default for the aaLog files.

###CacheFileBaseName
The cache file base name lets you specify a unique base file name for your cache file.  You might want to do this if you don't like the default name or you have another specific reason.

###CacheFileAppendProcessNameToBaseFileName
If you want to use the base filename but append a unique process name you can set this to true.  You would do this in a situation where you had multiple programs all reading the same log directory and you wanted each of them to keep their own unique cache file.  You may run into a scenario, though where you have multiple instances with the same process name.  In that case you might want to inject custom cache file name.

###CacheFileNameCustom
If you don't want to use the existing methods for creating a cache file name you can create your own custom cache file name.

###IgnoreCacheFileOnFirstRead
If you want the application to ignore the cache file on first read, set this flag to true.

##Filter Options
You can specify as many filters as you wish.  The basic concept is that you create an array of filters.  Each filter as two members; Field and Filter.

###Field
The field specifies the field name to filter on.  This might be component, message, logflag, etc.  For some special numeric fields we allow max and min decorated fields.

The allowed **fields** are
 
	messagemumbermin - simple numerical comparison                  
	messagenumbermax - simple numerical comparison
	datetimemin - simple numerical comparison
	datetimemax - simple numerical comparison
	processid - legal regular expression
	threadid - legal regular expression
	logflag - legal regular expression
	component - legal regular expression
	message - legal regular expression
	processname - legal regular expression
	sessionid - legal regular expression
	hostfqdn - legal regular expression

###Filter
The filter is specified according to the rules defined for each field above.

##Example
Below is an what an example of what a complete options file might look like.  Note that the filters are applied in the exact order listed.  Conceptually it doesn't matter as the effect of the filters is additive.

	{
		"LogDirectory": "C:\\ProgramData\\ArchestrA\\LogFiles",
		"CacheFileBaseName": "aaLogReaderCache",
		"CacheFileNameCustom": "",
		"CacheFileAppendProcessNameToBaseFileName": true,
		"IgnoreCacheFileOnFirstRead": true,
		"LogRecordPostFilters": 
		[
			{
				"Field": "Message",
				"Filter": "Warning 40|Message 41"
			},
			{
				"Field": "Message",
				"Filter": "Warning 40|Message 41"
			},
			{
				"Field": "MessageNumberMin",
				"Filter": "6826080"
			},
			{
				"Field": "MessageNumberMax",
				"Filter": "6826085"
			},
			{
				"Field": "DateTimeMin",
				"Filter": "2015-06-19 01:45:00"
			},
			{
				"Field": "DateTimeMax",
				"Filter": "2015-06-19 01:45:05"
			},
			{
				"Field": "ProcessID",
				"Filter": "7260"
			},
			{
				"Field": "ThreadID",
				"Filter": "7264"
			},
			{
				"Field": "Message",
				"Filter": "Started"
			}
		]
	}

