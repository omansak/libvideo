#!/bin/bash

test=1
nuget=1
run=0
cache=0
config="Release"

scriptroot="$(cd "$(dirname $0)" && pwd -P)"
nugetpath="$scriptroot/nuget/NuGet.exe"
cachefile="$scriptroot/$(basename $0).cache"

msbuildprompt="Please specify the directory where MSBuild is installed.
Example: ./build.sh \"/C/Program Files (x86)/MSBuild/14.0/Bin\""

usage="Usage: ./build.sh [OPTION]...

Options:

-c, --config [DEBUG|RELEASE]   set build configuration to Debug or Release
-h, --help                     display this help text and exit
-m, --msbuild                  specify path to MSBuild.exe (required on first run)
--nocache                      do not cache the path to MSBuild.exe if specified
--norun                        build nothing unless specified by other options
-n, --nuget                    create NuGet package post-build"

usage() {
    echo "$usage" 1>&2
}

failerr() {
    exitcode=$?
    if [ "$exitcode" -ne 0 ]
    then
        echo "$1"
        exit $exitcode
    fi
}

project() {
    if [ "$1" != "packages" ] && [ "$1" != ".vs" ] && [ "${1##*.}" != "sln" ]
    then
        return 0
    fi

    return 1
}

executable() {
    grep "<OutputType>Exe</OutputType>" "$1/$1.csproj" &> /dev/null
    return "$?"
}

# get options
while [[ "$#" > 0 ]]
do
    case "$1" in
        -t|--test)
            test=0
            ;;
        -n|--nuget)
            nuget=0
            ;;
        -c|--config)
            case "${2,,}" in
                "debug")
                    config="Debug"
                    ;;
                "release")
                    # enabled by default
                    ;;
                *)
                    usage
                    exit 1
                    ;;
            esac
            shift
            ;;
        --norun)
            run=1
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        -m|--msbuild)
            # IMPORTANT: all reads/writes to $msbuildpath MUST be quoted
            # because the typical path for Windows users is in Program Files (x86).
            msbuildpath="$2/MSBuild.exe"
            shift
            ;;
        --nocache)
            cache=1
            ;;
        *)
            usage
            exit 1
            ;;
    esac
    shift
done

# determine path to MSBuild.exe
if [ -z "$msbuildpath" ]
then
    msbuildpath="$(cat $cachefile 2> /dev/null)"

    if [ -z "$msbuildpath" ]
    then
        echo "$msbuildprompt"
        exit 1
    elif [ ! -e "$msbuildpath" ]
    then
        echo "$msbuildpath does not exist. Please specify the directory where it is installed."
        exit 1
    fi
else
    if [ ! -e "$msbuildpath" ]
    then
        echo "$msbuildpath does not exist."
        exit 1
    fi

    if [ $cache -eq 0 ]
    then
        echo "$msbuildpath" > $cachefile
    fi
fi

if [ "$run" -eq 0 ]
then
    echo "Building src..."
    cd $scriptroot/src
    "$msbuildpath" /property:Configuration=$config

    failerr "MSBuild failed on libvideo.sln! Exiting..."
fi

if [ "$test" -eq 0 ]
then
    echo "Running tests..."

    for test in $scriptroot/tests/*
    do
        echo "Building test $test..."
        cd $test
        "$msbuildpath" /property:Configuration=$config

        failerr "MSBuild failed on $test.sln! Exiting..."

        for subtest in *
        do
            if project "$subtest" && executable "$subtest"
            then
                echo "Running subtest $subtest..."
                cd $subtest/bin/$config
                ./$subtest.exe

                failerr "Subtest $subtest failed! Exiting..."
            fi
        done
    done
fi

if [ "$nuget" -eq 0 ]
then
    echo "Creating NuGet packages..."

    for projdir in $scriptroot/src/*
    do
        baseproj="$(basename $projdir)"
        if project "$baseproj"
        then
            echo "Getting assemblies from $projdir..."
            cd $projdir/bin/$config
            mkdir $scriptroot/nuget/lib 2> /dev/null
            cp $baseproj.dll $scriptroot/nuget/lib
        fi
    done

    echo "Cleaning existing packages..."
    cd $scriptroot/nuget
    rm *.nupkg

    # stole a few of these lines from the CoreFX build.sh
    if [ ! -e $nugetpath ]
    then
        echo "Restoring NuGet.exe..."

        # curl has HTTPS CA trust-issues less often than wget, so try that first
        which curl &> /dev/null
        if [ $? -eq 0 ]; then
           curl -sSL --create-dirs -o $nugetpath https://api.nuget.org/downloads/nuget.exe
       else
           which wget &> /dev/null
           failerr "cURL or wget is required to build libvideo."
           wget -q -O $nugetpath https://api.nuget.org/downloads/nuget.exe
        fi

        failerr "Failed to restore NuGet.exe."
    fi

    for spec in *.nuspec
    do
        echo "Packing $spec..."
        $nugetpath pack $spec

        failerr "Packing $spec failed! Exiting..."
    done
fi
