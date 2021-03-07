#!/bin/sh
dotnet publish --self-contained=true -p:PublishTrimmed=true -p:PublishSingleFile=true -r win-x64 -o Build
