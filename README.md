# TorrentHandler

TorrentHandler routes .torrent files to the correct client based on tracker domains and can bring the target client window to the foreground.

## Configuration

Create a config file at:

%APPDATA%\TorrentHandler\config.json

You can start from this minimal template:

{
  "version": 1,
  "clients": [
    {
      "id": "tv",
      "label": "TV",
      "path": "C:\\Path\\To\\Client.exe",
      "focus": {
        "mode": "processPath",
        "windowClass": null,
        "windowTitle": null,
        "windowTitleContains": "uTorrent 2.2.1"
      }
    }
  ],
  "categories": [
    {
      "id": "tv",
      "label": "TV",
      "clientId": "tv",
      "rules": [
        {
          "trackerDomains": [
            "landof.tv"
          ]
        }
      ]
    }
  ]
}

### Focus options

- mode:
  - none: skip focusing
  - processPath: find by executable path
  - window: find by window class/title
- windowClass: exact window class name (optional)
- windowTitle: exact window title (optional)
- windowTitleContains: substring match for window title (optional)

## Releases

Releases publish a single executable:

- TorrentHandler.exe

Download the release and place the config file in %APPDATA%\TorrentHandler\config.json.
