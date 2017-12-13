# Windows 10 Virtual Desktop Tracker

When using Windows 10's virtual desktops feature, there's no way to tell at a glance which virtual desktop you're currently on.

This utility program shows a tasktray icon tracking the current virtual desktop number.

Initially the icon will display "0," meaning no virtual desktops are in use.  When the user creates and switches to an alternate desktop, the original desktop will be designated "1" instead of "0" and the new desktop will be "2," and so on.  The program tracks up to 9 concurrent virtual desktops.

You can also manually set individual desktop background images per virtual desktop via an INI-file.

## Getting Started

Put VDTracker.exe somewhere, create a shortcut to it, open a file explorer with 'shell:startup' and drop the shortcut in there so the program starts as boot.

Learn how to [configure individual virtual desktop backgrounds](https://github.com/belzecue/VDTracker/wiki/INI-file).

### Prerequisites

Build is targetted for dotnet 2.

## Roadmap

No futher features planned.

## License

Copyright 2017 Andrew Ferguson.  Licensed under the MIT License.

## Acknowledgments

* Based on example code by Chris Lewis [MSFT] - [link](https://blogs.msdn.microsoft.com/winsdk/2015/09/10/virtual-desktop-switching-in-windows-10/)

