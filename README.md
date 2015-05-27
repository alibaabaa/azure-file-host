# azure-file-host

## Azure image and file host with JSON configuration
azure-file-host is an ASP.NET web app that accepts POSTed files and pushes them to an Azure blob store. Multiple configurations are supported via JSON config files, including useful stuff like **image resizing** and **file renaming**.

## Overview
The deployed website accepts [standard multipart/form-data](http://stackoverflow.com/questions/4526273/what-does-enctype-multipart-form-data-mean) POST requests containing a file, API key, and configuration name.

The received API key is used to look up a matching JSON configuration (see `example-config.json`) from an Azure storage table. The POSTed configuration name is then used to match a section from the JSON, and the uploaded file is processed according to the matched rules.

The resulting file(s) are then stored to the blob storage account described in the JSON config, and the resulting blob URLs returned in the HTTP response.

## Pre-requisites
1. An Azure storage account with a table named `uploadconfigs`.
2. An Azure storage account (can be the same account as step 1) to store the hosted file blobs.

## Installation
azure-file-host is built to work as an Azure deployed web app, but works just as well in a traditional hosting environment.

### Deploying to Azure
Deploy the project directly to an Azure web app and add an app setting from the Azure management console with the key `ConfigurationStore` and the value of the connection string to the Azure storage account that will hold your JSON configuration.

### Deploying to a traditional hosting environment
Modify the `Web.config`, replacing the value of `<add key="ConfigurationStore" value="UseDevelopmentStorage=true;" />` with the connection string to the Azure storage account that will hold your JSON configuration.

## Configuration
Each JSON configuration should be added as a row to an Azure table called `uploadconfigs`. Each row should consist of columns:

* `PartitionKey`: `JsonConfig`
* `RowKey`: This configuration's API key GUID in lower-case **with hyphens removed**
  * For example: `cdf300ff14fd4129a638cb8c1e261fa2`
* `Config`: A string of well-formed JSON configuration.

## JSON configuration options
Refer to [example-config.json](https://github.com/alibaabaa/azure-file-host/blob/master/example-config.json) for a sample JSON configuration and the available options. `imageactions` are used when processing jpeg and png file types. All other files are processed using `fileactions`. A `contentActionSet` is chosen by matching to the POSTed request parameter `config`.
