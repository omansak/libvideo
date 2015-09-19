#!/bin/bash

test=1
nuget=1
samples=1
run=0
cache=0
config="Release"
verbosity="minimal"

scriptroot="$(cd "$(dirname $0)"; pwd -P)"
nugetpath="$scriptroot/NuGet.exe"
cachefile="$scriptroot/$(basename $0).cache"
targets="portable-net45+win+wpa81+MonoAndroid10+xamarinios10+MonoTouch10"
osys=$(uname)
windows="$(uname | grep -E 'CYGWIN|MINGW|MSYS' 1> /dev/null; echo $?)"

msbuildprompt="Please specify the directory where MSBuild is installed.
Example: ./build.sh --msbuild \"/C/Program Files (x86)/MSBuild/14.0/Bin\""

usage="Usage: ./build.sh [OPTION]...

Options:

-c, --config [DEBUG|RELEASE]   set build configuration to Debug or Release
-h, --help                     display this help text and exit
-m, --msbuild                  specify path to MSBuild.exe (required on first run)
--nocache                      do not cache the path to MSBuild.exe if specified
--norun                        build nothing unless specified by other options
-n, --nuget                    create NuGet package post-build
-s, --samples                  build all projects in the samples/ dir
-t, --test                     compile and run tests after libvideo.sln is built
-v, --verbosity                set MSBuild verbosity, e.g. quiet"

error() {
    echo "$1" 1>&2
}

usage() {
    error "$usage"
}

failerr() {
    exitcode=$?
    if [ "$exitcode" -ne 0 ]
    then
        error "$1"
        exit $exitcode
    fi
}

project() {
    if [ "$1" != "packages" -a "$1" != ".vs" -a "${1##*.}" != "sln" ]
    then
        return 0
    fi

    return 1
}

executable() {
    grep "<OutputType>Exe</OutputType>" "$1/$1.csproj" &> /dev/null
    return "$?"
}

nuget() {
    if [ $windows -eq 0 ]
    then
        $nugetpath $@
    else
        mono $nugetpath $@
    fi
}

buildproj() {
    nuget restore
    if [ $windows -eq 0 ]
    then
        "$msbuildpath" /property:Configuration=$config /verbosity:$verbosity
    else
        mono "$msbuildpath" /property:Configuration=$config /verbosity:$verbosity
    fi
}

absolute() {
    echo "$(cd "$(dirname "$1")"; pwd -P)/$(basename "$1")"
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
        -v|--verbosity)
            case "${2,,}" in
                "quiet"|"normal"|"detailed"|"diagnostic")
                    verbosity="${2,,}"
                    ;;
                "minimal")
                    # enabled by default
                    ;;
                *)
                    usage
                    exit 1
                    ;;
            esac
            shift
            ;;
        -s|--samples)
            samples=0
            ;;
        *)
            usage
            exit 1
            ;;
    esac
    shift
done

# check for Mono (mostly plagiarized from CoreFX)
if [ $windows -ne 0 ]
then
    if [ $osys == "Linux" ]
    then
        monoroot=/usr
    elif [ $osys == "FreeBSD" ]
    then
        monoroot=/usr/local
    else
        # OS X (Darwin)
        monoroot=/Library/Frameworks/Mono.framework/Versions/Current
    fi

    referenceassemblyroot=$monoroot/lib/mono/xbuild-frameworks

    monoversion=$(mono --version | grep "version 4.[1-9]")

    if [ $? -ne 0 ]
    then
        # if built from tarball, Mono only identifies itself as 4.0.1
        monoversion=$(mono --version | egrep "version 4.0.[1-9]+(.[0-9]+)?")
        if [ $? -ne 0 ]
        then
            error "Mono 4.0.1.44 or later is required to build libvideo on Unix."
            exit 1
        else
            error "WARNING: Mono 4.0.1.44 or later is required to build libvideo on Unix. Unable to assess if current version is supported."
        fi
    fi

    if [ ! -e "$referenceassemblyroot/.NETPortable" ]
    then
        error "PCL reference assemblies not found! Install 'referenceassemblies-pcl' via your distro's package manager, and try again."
        exit 1
    fi
fi

# determine path to MSBuild.exe
if [ -z "$msbuildpath" ]
then
    msbuildpath="$(cat $cachefile 2> /dev/null)"

    if [ $? -ne 0 ]
    then
        error "$msbuildprompt"
        exit 1
    elif [ ! -e "$msbuildpath" ]
    then
        error "\"$msbuildpath\" was read from the cache, but it does not exist."
        error "$msbuildprompt"
        exit 1
    fi
else
    if [ ! -e "$msbuildpath" ]
    then
        error "\"$msbuildpath\" does not exist."
        exit 1
    fi

    if [ $cache -eq 0 ]
    then
        msbuildpath="$(absolute "$msbuildpath")"
        echo "$msbuildpath" > $cachefile
    fi
fi

# restore NuGet.exe if not yet installed
# will admit I stole a few of these lines from the CoreFX build.sh...
if [ ! -e $nugetpath ]
then
    echo "Restoring NuGet.exe..."

    # curl has HTTPS CA trust-issues less often than wget, so try that first
    which curl &> /dev/null
    if [ $? -eq 0 ]; then
       curl -sSL -o $nugetpath https://api.nuget.org/downloads/nuget.exe
   else
       which wget &> /dev/null
       failerr "cURL or wget is required to build libvideo."
       wget -q -O $nugetpath https://api.nuget.org/downloads/nuget.exe
    fi

    failerr "Failed to restore NuGet.exe."
fi

# start the actual build

if [ "$run" -eq 0 ]
then
    echo "Building src..."
    cd $scriptroot/src
    buildproj

    failerr "MSBuild failed on libvideo.sln! Exiting..."
fi

if [ "$test" -eq 0 ]
then
    echo "Running tests..."

    for test in $scriptroot/tests/*
    do
        echo "Building test $test..."
        cd $test
        buildproj

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

if [ "$samples" -eq 0 ]
then
    echo "Building samples..."

    for sample in $scriptroot/samples/*
    do
        echo "Building sample $sample..."
        cd $sample
        buildproj

        failerr "MSBuild failed on $sample.sln! Exiting..."
    done
fi

if [ "$nuget" -eq 0 ]
then
    echo "Creating NuGet packages..."

    for packageroot in $scriptroot/nuget/*
    do
        package="$(basename $packageroot)"
        echo "Getting assemblies for $package..."
        mkdir -p $packageroot/lib/$targets
        cd $scriptroot/src/$package/bin/$config
        cp $package.dll $packageroot/lib/$targets

        echo "Cleaning existing $package packages..."
        cd $packageroot
        rm *.nupkg 2> /dev/null

        for spec in *.nuspec
        do
            echo "Packing $spec..."
            nuget pack $spec

            failerr "Packing $spec failed! Exiting..."
        done
    done
fi
