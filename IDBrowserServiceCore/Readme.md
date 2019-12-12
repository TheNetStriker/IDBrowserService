# IDBrowserServiceCore

Currently actively maintained .Net Core based webservice. Works under Mac, Linux and Windows with support for MSSQL and Postgresql Databases. It should also work on ARM based systems, with exception of the image transform functions. (The ImageMagick.Net project is not yes compatible with ARM processors)

# Prerequisites

* Asp.Net Core 3.1 runtime

* FFMpeg for Video transformation and video thumbnails (Must also be added to the PATH environment variable)

# Setup

## appsettings.json
### global settings
| Settings | Description |
| --- | --- |
| Urls | Url to host the webservice. Multiple URL's can be set using a semicolon as delimiter. |
| ImageFileExtensions | List of file extensions for images. (Used for batch thumbnail generation) |
| VideoFileExtensions | List of file extensions for videos. (Used for batch thumbnail generation and video transformation) |
| Sites | In this section is is possible to define multiple sites with different Photosupreme databases. This way a single service can provide access to multiple Photosupreme databases. The name of the site must then be used in the url. (e.g. http://192.168.1.1/SiteName/values/GetImageProperties) Please take a look at the Site Settings section how to configure a new site. |
| Serilog | Logging configuration |
### Site settings
| Settings | Description |
| --- | --- |
| ConnectionStrings.DBType | Database type to connect to. (MsSql or Postgresql) |
| ConnectionStrings.IDImager | Connection string of the Photosupreme database on MsSql or Postgresql. (Connection strings for MsSql and Postgressql are different. Please find some examples for the Entity framework on the internet) |
| ConnectionStrings.IDImagerThumbs | Connection string of the Photosupreme thumbs database on MsSql or Postgresql. (Connection strings for MsSql and Postgressql are different. Please find some examples for the Entity framework on the internet) |
| ServiceSettings.CreateThumbnails | If set to "true" the service will try to generate thumbnails if they don't exist. |
| ServiceSettings.MThumbmailWidth | Thumbnail width for thumbnail generation. |
| ServiceSettings.MThumbnailHeight | Thumbnail height for thumbnail generation. |
| ServiceSettings.KeepAspectRatio | Setting to keep aspect ratio for thumbnail generation. |
| ServiceSettings.SetGenericVideoThumbnailOnError | Not used anymore |
| ServiceSettings.FilePathReplace | Using this settings the file paths in the database can be adapted to the local path where the service is running. (e.g. if the database was created under windows and the service runs under Linux) |
| ServiceSettings.TranscodeDirectory | Directory to put transcoded video files for this site. |

# Security

If the service should be accessable from the web, it should at least be secured with https. Better would be to also secure the service using a client certificate authentification. This can be done by using a reverse proxy like HAProxy. The client certificate can then be added to the Android client to ensure that only the clients with the certificate can connect.

Authentification using client certificates only works vor the values api at the moment. For the media streaming api the connection can be https but without a client certificate authentification. The reason for this is that the vlc media player currently does not support authentification using client certificates.

# Starting the service

The service can simply be started using the commandline
```
dotnet IDBrowserServiceCore.dll
```
Under Linux the service can be started automatically using a systemctl service using the following config:
```
[Unit]
Description=IDBrowser Service Core

[Service]
WorkingDirectory=/var/www/IDBrowserServiceCore
ExecStart=/usr/bin/dotnet /var/www/IDBrowserServiceCore/IDBrowserServiceCore.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-IDBrowserServiceCore
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production 

[Install]
WantedBy=multi-user.target
```

# Commandline commands
The IDBrowserServiceCore.dll can also execute batch jobs from the commandline. At the moment there are the following commandline commands available:
## TranscodeAllVideos
This command transcodes all videos to a specific resultion into the TranscodeDirectory. The command needs the name of the site and the resolution (Hd480, Hd720, Hd1018) as additional parameters. Here is an example:
dotnet IDBrowserServiceCore.dll
```
dotnet IDBrowserServiceCore.dll TranscodeAllVideos MySite Hd480
```
## GenerateThumbnails
This command generates all missing thumbnails in the database. The command needs the site name as additional parameter. Here is an example:
```
dotnet IDBrowserServiceCore.dll GenerateThumbnails MySite
```
After that several optional Parameters can be specified to reduce the amount of images to be checked. There is also an option to overwrite existing thumnnails. This command uses the ImageFileExtensions and the VideoFileExtensions settings from the appsettings.json file to get a list of images and videos to process.