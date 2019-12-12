# Description

This is the sourcecode for the remote webservice of the the [IDBrowser](https://play.google.com/store/apps/details?id=ch.masshardt.idbrowser) Android app. The webservice allows remote access from the app to any [Photosupreme](https://www.idimager.com) database. (MSSQL and Postgresql)

Currently only the IDBrowserServiceCore is under active development. The .Net Core version is much faster than the older .Net Framework based webservices and it also runs under Mac, Linux and Windows.

# Projects

## IDBrowserService

Older .Net Framework based version of the webservice. (Not under development anymore)

## IDBrowserServiceCode

Core classes of .Net Framework based version of the webservice. (Not under development anymore)

## IDBrowserServiceCore

Currently actively maintained .Net Core based webservice. Works under Mac, Linux and Windows with support for MSSQL and Postgresql Databases. It should also work on ARM based systems, with exception of the image transform functions. (The ImageMagick.Net project is not yes compatible with ARM processors)

For compiled versions please look at the releases tab. 

For instructions how to configure the service please look at the [readme in the IDBrowserServiceCore folder](https://github.com/TheNetStriker/IDBrowserService/tree/master/IDBrowserServiceCore).

## IDBrowserServiceCoreTest

Test project for .Net Core based webservice.

## IDBrowserServiceStandalone

Standalone version of older .Net Framework based version of the webservice. (Not under development anymore)

## IDBrowserServiceTest

Test project for older .Net Framework based version of the webservice. (Not under development anymore)

## IDBrowserStreamingService

Older .Net based video streaming service. (Not under development anymore, replaced with streaming service in IDBrowserServiceCore)

## ManagementConsole

Management console (Currently only for batch video transformation)

## ThumbnailGenerator

Tool to batch generate thumbnails.