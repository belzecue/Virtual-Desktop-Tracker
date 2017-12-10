# Windows 10 Virtual Desktop Tracker

When using Windows 10's virtual desktops feature, there's no way to tell at a glance which virtual desktop you're currently on.

This utility program shows a tasktray icon tracking the current virtual desktop number.

Initially the icon will display "0," meaning no virtual desktops are in use.  When the user creates and switches to an alternate desktop, the original desktop will be designated "1" instead of "0" and the new desktop will be "2," and so on.  The program tracks up to 9 concurrent virtual desktops.  (I included only 9 numerical icons!)

## Getting Started

Put the VDTracker.exe somewhere, create a shortcut to it, open a file explorer with 'shell:startup' and drop the shortcut in there so the program starts as boot.

### Prerequisites

Build targetted for dotnet 2.

### Installing

Put the VDTracker.exe somewhere, create a shortcut to it, open a file explorer with 'shell:startup' and drop the shortcut in there so the program starts as boot.

## Roadmap

Possibility of adding a virtual desktop background image manager to auto-switch backgrounds -- unless MS eventually beats me to that no-brainer.

## License

This project is licensed under the MIT License.

## Acknowledgments

* Based on example code by Chris Lewis [MSFT] - [link](https://blogs.msdn.microsoft.com/winsdk/2015/09/10/virtual-desktop-switching-in-windows-10/)

