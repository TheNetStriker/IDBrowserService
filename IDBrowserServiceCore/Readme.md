
# IDBrowserServiceCore

Currently actively maintained .Net Core based webservice. Works under Mac, Linux and Windows with support for MSSQL and Postgresql Databases. It should also work on ARM based systems, with exception of the image transform functions. (The ImageMagick.Net project is not yes compatible with ARM processors)

# Docker
For easy installation under Linux a Docker container can be installed.

Minimal start command:
```
docker run -t -i -d -p 5000:80 \
--name idbrowserservicecore \
--restart always \
-v /mnt/myVolume:/mnt/myVolume \
-e Sites__site1__ConnectionStrings__DBType="Postgres" \
-e Sites__site1__ConnectionStrings__IDImager="Host=192.168.16.2;Database=photosupreme;Username=idimager_main;Password=idi_main_2606;" \
-e Sites__site1__ConnectionStrings__IDImagerThumbs="Host=192.168.16.2;Database=photosupreme_thumbs;Username=idimager_main;Password=idi_main_2606;" \
-e Sites__site1__ServiceSettings__FilePathReplace__0__PathMatch="\\MYSERVER\myVolume" \
-e Sites__site1__ServiceSettings__FilePathReplace__0__PathReplace="/mnt/myVolume" \
-e Sites__site1__ServiceSettings__TranscodeDirectory="/mnt/myVolume/VideoTranscode/site1" \
-e Sites__site1__ServiceSettings__TokenSecretKey="ThisIsUnsecurePleaseChangeMeAsSoonAsPossible" \
-e Sites__site1__ServiceSettings__TokenIssuer="IDBrowserServiceCore_Site_site1" \
-e Sites__site1__ServiceSettings__TokenAudience="IDBrowserUser" \
-e Sites__site1__ServiceSettings__TokenExpiration="01:00:00" \
thenetstriker/idbrowserservicecore:1.2.1
```
Additional application environment variables:
```
-e Urls="http://*:5000" \
-e Serilog__MinimumLevel="Error" \
-e UseResponseCompression="true" \
-e UseSwagger="true" \
```
Additional site environment variables:
```
-e Sites__site1__ServiceSettings__CreateThumbnails="true" \
-e Sites__site1__ServiceSettings__MThumbmailWidth="1680" \
-e Sites__site1__ServiceSettings__MThumbnailHeight="1260" \
-e Sites__site1__ServiceSettings__DisableInsecureMediaPlayApi="true" \
-e Sites__site1__ServiceSettings__EnableDatabaseCache="true" \
-e Sites__site1__ServiceSettings__CronJobs__UpdateDatabaseCacheJob="0 */15 * * * ?" \
```
It is also possible to configure multiple sites. Just copy all -e environment variables and replace site1 with the second site name.

# Prerequisites

* Asp.Net 5.0 runtime

* FFMpeg for Video transformation and video thumbnails (Must also be added to the PATH environment variable)

# Setup

## appsettings.json
### global settings
| Settings | Description |
| --- | --- |
| Urls | Url to host the webservice. Multiple URL's can be set using a semicolon as delimiter. |
| ImageFileExtensions | List of file extensions for images. (Used for batch thumbnail generation) |
| VideoFileExtensions | List of file extensions for videos. (Used for batch thumbnail generation and video transformation) |
| UseResponseCompression | Enables http response compression (gzip and brotli) |
| UseSwagger | Enables webservice swagger documentation on /SitName/swagger. |
| Serilog | Logging configuration |
## sites.json
In this file is is possible to define multiple sites with different Photosupreme databases. This way a single service can provide access to multiple Photosupreme databases. The name of the site must then be used in the url. (e.g. http://192.168.1.1/SiteName/values/GetImageProperties) Please take a look at the Site Settings section and the example.sites.json how to configure a new site.
### Site settings
| Settings | Description |
| --- | --- |
| ConnectionStrings.DBType | Database type to connect to. (MsSql or Postgresql) |
| ConnectionStrings.IDImager | Connection string of the Photosupreme database on MsSql or Postgresql. (Connection strings for MsSql and Postgressql are different. Please find some examples for the Entity framework on the internet) |
| ConnectionStrings.IDImagerThumbs | Connection string of the Photosupreme thumbs database on MsSql or Postgresql. (Connection strings for MsSql and Postgressql are different. Please find some examples for the Entity framework on the internet) |
| ServiceSettings.CreateThumbnails | If set to "true" the service will try to generate thumbnails if they don't exist. |
| ServiceSettings.MThumbmailWidth | Thumbnail width for thumbnail generation. |
| ServiceSettings.MThumbnailHeight | Thumbnail height for thumbnail generation. |
| ServiceSettings.FilePathReplace | Using this settings the file paths in the database can be adapted to the local path where the service is running. (e.g. if the database was created under windows and the service runs under Linux) |
| ServiceSettings.TranscodeDirectory | Directory to put transcoded video files for this site. |
| ServiceSettings.TokenSecretKey| Secret key for token generation for the secure media play api. (min 256 bytes) |
| ServiceSettings.TokenIssuer | Token issuer name for token generation. |
| ServiceSettings.TokenAudience | Token audience name for token generation. |
| ServiceSettings.TokenExpiration | Time until token expires. (Format Hour:Minute:Second) |
| ServiceSettings.DisableInsecureMediaPlayApi| If set to true the unsecure media play api will be disabled. |
| ServiceSettings.EnableDatabaseCache| Added in version 1.9. Enables database cache if set to true. This only caches the most expensive queries for the image properties with photo count. Cache is updated with the UpdateDatabaseCacheJob cron job. |
| ServiceSettings.CronJobs.UpdateDatabaseCacheJob| Added in version 1.9. Cron expression to update database cache. (Default is every 15 Minutes "0 */15 * * * ?") Cache is only updated if new image properties are added/deleted or images get a new image property assigned or removed. |

# Security

If the service should be accessable from the web, it should at least be secured with https. Better would be to also secure the service using a client certificate authentification. This can be done by using a reverse proxy like HAProxy. The client certificate can then be added to the Android client to ensure that only the clients with the certificate can connect.

Authentification using client certificates only works vor the values api. For the media streaming api the connection can be https but without client certificate authentification. The reason for this is that the vlc media player currently does not support authentification using client certificates. For this reason I've implemented a token based authentification in the media service. Tokens can be requested in the secure values service and then used in the media service to play videos. The tokens are only valid for limited time. (see setting **ServiceSettings.TokenExpiration**) For backwards compatibility the usecure play function is still enabled and can be disabled using the **ServiceSettings.DisableInsecureMediaPlayApi** setting.

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
This command transcodes all videos to a specific resultion into the TranscodeDirectory. The command requires the name of the site and the resolution (Hd480, Hd720, Hd1018) as parameters and optionally the number of FFmpeg instances (default 2) and the log level (default Error, possible values Verbose, Debug, Information, Warning, Error and Fatal). Here is an example:
```
dotnet IDBrowserServiceCore.dll TranscodeAllVideos MySite Hd480 2 Error
```
When using the Docker container use the following command:
```
docker exec -it idbrowserservicecore dotnet IDBrowserServiceCore.dll TranscodeAllVideos MySite Hd480 2 Error
```
## GenerateThumbnails
This command generates all missing thumbnails in the database. The command needs the site name as additional parameter. Here is an example:
```
dotnet IDBrowserServiceCore.dll GenerateThumbnails MySite
```
When using the Docker container use the following command:
```
docker exec -it idbrowserservicecore dotnet IDBrowserServiceCore.dll GenerateThumbnails MySite
```
After that several optional Parameters can be specified to reduce the amount of images to be checked. There is also an option to overwrite existing thumnnails. This command uses the ImageFileExtensions and the VideoFileExtensions settings from the appsettings.json file to get a list of images and videos to process.