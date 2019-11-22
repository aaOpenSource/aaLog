# aaLogElasticFileBeat

This project creates a Windows service that runs a periodic function to get all the unread log messages and forward them to FileBeat

## FileBeat setup
Follow the FileBeat setup instructions though you don't need to install the service (this service will take care of calling the filebeat binary). Set up a single input of type stdin, with two other lines: 
- enabled: true
- json.keys_under_root: true

## aaLogElasticFileBeat setup
Run setup.msi. Look in the install location (Default is C:\Program Files (x86)\aaOpenSource\aaLogElasticFileBeat) for aaLogElasticFileBeat.yml. You can change the update interval or point at different locations than default for the binary and config files for filebeat.