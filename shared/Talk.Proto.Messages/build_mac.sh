#!/usr/bin/env bash
~/.nuget/packages/google.protobuf.tools/3.6.1/tools/macosx_x64/protoc \
    *.proto -I=. --csharp_out=. --csharp_opt=file_extension=.g.cs

