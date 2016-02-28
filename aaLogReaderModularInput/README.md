aaLogReaderModularInput
=================

This project creates a Splunk Modular input that makes use of aaLog library to send entries to the Splunk server.

## Deployment

You have two options for deployment.

### Build and Copy

From Visual Studio you can rebuild the project.  Under the project properties you will find the following post-build script

```
REM Uncomment the 'goto Deploy' line below to deploy to your local Splunk instance on each build.
REM You will need to either have the %SPLUNK_HOME% environment variable set, or replace $(SPLUNK_HOME) with the path to your Splunk instance.
REM You will also need to be running Visual Studio as an Administrator so that it has permissions to copy the files to Splunk
goto Deploy

:Deploy
xcopy "$(ProjectDir)$(ProjectName)\*.*" "$(SPLUNK_HOME)\etc\apps\$(ProjectName)" /F /R /Y /I /E
xcopy "$(TargetDir)\*.*" "$(SPLUNK_HOME)\etc\apps\$(ProjectName)\bin" /F /R /Y /I
echo Deployed Application to $(SPLUNK_HOME)\etc\apps\$(ProjectName)
```

This script will copy the results of the build to the Splunk Apps Directory, typically `C:\Program Files\Splunk\etc\apps\`.

### Unzip and Copy
If you do not have Visual Studio to compile the solution I have included a ZIP file `aaLogReaderModularInput.zip` that you can unzip and place into the `C:\Program Files\Splunk\etc\apps\` folder.
