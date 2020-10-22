#!/bin/bash

set -euo pipefail
set -x

nuget () {
	package="$1"
	version="$2"
	zip="`mktemp`.zip"
	url="https://www.nuget.org/api/v2/package/$package/$version"
	dir="GerberProjects/packages/$package.$version"

	test -d "$dir" || {
		wget "$url" -O "$zip"
		mkdir -p "$dir"
		unzip "$zip" -d "$dir"
		rm "$zip"
	}
}

# Needed as xbuild gets confused about some modern terminals
export TERM=xterm

nuget Triangle 0.0.6-Beta3
nuget DotNetZip 1.12.0
nuget OpenTK 3.0.1
nuget OpenTK.GLControl 3.0.1
nuget netDXF 2.0.2
nuget netDXF 0.9.3
nuget GlmNet 0.5.1
nuget DockPanelSuite 3.0.6
nuget DockPanelSuite.ThemeVS2015 3.0.6


xbuild /p:Configuration=Debug GerberProjects/GerberProjects.sln
