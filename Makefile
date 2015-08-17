.ONESHELL:
	SHELL = /bin/bash

nuget = $(shell pwd)/nuget.exe
msbuild = $(shell pwd)/msbuild.exe
root = $(shell pwd)/src
packages = $(shell pwd)/nuget
tdir = $(shell pwd)/tests
name = VideoLibrary

sln = $(root)/libvideo.sln
proj = $(root)/libvideo
csproj = $(proj)/libvideo.csproj
dll = $(proj)/bin/$(config)/libvideo.dll
spec = $(packages)/$(name).nuspec

copy = true
build = true
test = true
config = Release

.PHONY: build nuget
build:
	if [[ $(build) != "false" ]]
	then
		$(msbuild) $(sln) /property:Configuration=$(config)
		
		if [[ $(test) != "false" ]]
		then
			cd $(tdir)
			for dir in $$(echo */)
			do
				cd $$dir
				pattern="*.sln"
				all=($$pattern)
				target="$${all[0]}"
				$(msbuild) $$target /property:Configuration=Release

				name=$${target:0:$${#target}-4}
				cd $$name/bin/Release
				./"$$name".exe
				
				code=$$?
				if [[ $$code != 0 ]]
				then
					echo "Test [$$name] has failed!"
					exit $$code
				fi
				cd ..
			done
			cd ..
		fi
	fi
	if [[ $(copy) != "false" ]]
	then
		cp $(dll) .
	fi
nuget: build
	mkdir $(packages)/lib
	cp libvideo.dll $(packages)/lib
	$(nuget) pack $(spec)
	mv *.nupkg $(packages)
