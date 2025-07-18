.PHONY: build

build:
	@mcs -r:System.Windows.Forms.dll -r:System.Drawing App.cs \
		-out:twilight-line-win-gui.exe -target:winexe -sdk:2 \
		-win32icon:twilight_line.ico -resource:twilight_line.ico
