nuget = ./nuget.exe
msbuild = ./msbuild.exe
root = ./src
packages = ./nuget
name = VideoLibrary

sln = $(root)/libvideo.sln
proj = $(root)/libvideo
csproj = $(proj)/libvideo.csproj
dll = $(proj)/bin/Release/libvideo.dll
template = $(packages)/Template.nuspec
spec = $(packages)/$(name).nuspec

.PHONY: build nuget nuget-nobuild
build:
	$(msbuild) $(sln)
nuget: build nuget-nobuild
nuget-nobuild:
	cp $(dll) $(packages)/lib
	cat $(template) > $(spec)
	$(nuget) pack $(spec)
	mv $(name)*.nupkg $(packages)