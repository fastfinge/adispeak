# adispeak
Solution to make AdiIRC interface with screen readers.

## Latest Preconfigured Adispeak

Get adispeak alpha 0.3 from:
https://github.com/fastfinge/adispeak/releases/download/v_0.3/adispeak_0.3.zip

## Installing

Put everything in adispeak\bin\x64\release into the same folder as AdiIRC.exe. Run AdiIRC.exe, go to file, plugins, then load. Load adispeak.dll.  It should work!  

## Building

Have Visual Studio 2017. Do the obvious things; the project should come with everything you need!

## Preconfigured AdiIRC defaults

The preconfigured `adispeak/bin/x64/Release` package includes AdiIRC config files that connect to `irc.rblind.com` and autojoin `#main` by default. It also ships a `sounds` folder wired into `config.ini` for AdiIRC's supported sound events, using short WAV files selected from Onj.zip.
