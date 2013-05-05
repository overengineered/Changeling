Changeling
==========

A small utility for Windows that reassigns a program to open specified file type with
a program you want, but leaving the files icon unchanged. This is useful for example
if you want nice file extensions for source code files that come with Visual Studio,
but want to to launch a lightweight editor on double click, e.g. [Notepad2](http://www.flos-freeware.ch/notepad2.html)


## Download
[Version 1.0](https://dl.dropboxusercontent.com/u/5863966/Builds/Changeling-v1.0.7z)

## Usage

    changeling.exe program/key extension/file

    program/key     specify a full path to the program that you want to assign
                    the file type to. ""<program>"" ""%1"" will be used as a
                    command for files of that type
                    alternatively, you can define full command in app.config
                    and pass the key of that command instead

    extension/file  specify an extension or a path to a file with the extension
                    you want to change the default program for

## License

    Copyright 2012 Juozas Kontvainis

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
