#!/bin/bash

test=1
nuget=1
run=0
config="Release"

scriptroot=$(cd "$(dirname $0)" && pwd -P)
usage="Usage: ./build.sh [OPTION]...

Options:

-c, --config [DEBUG|RELEASE]   set build configuration to Debug or Release
-h, --help                     display this help text and exit
-n, --nuget                    create NuGet package post-build
--norun                        build nothing unless specified by other options"

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
        *)
            usage
            exit 1
            ;;
    esac
    shift
done

if [ "$run" -eq 0 ]
then
    cd $scriptroot/src
    $scriptroot/MSBuild /property:Configuration=$config
    
    failerr "MSBuild failed on libvideo.sln! Exiting..."
fi

if [ "$test" -eq 0 ]
then
    for test in $scriptroot/tests
    do
        cd $test
        $scriptroot/MSBuild /property:Configuration=$config
        
        failerr "MSBuild failed on $test.sln! Exiting..."
        
        for subdir in .
        do
            if [ "$subdir" != "packages" ] && [ "$subdir" != ".vs" ]
            then
                cd $subdir/bin/$config
                ./$subdir.exe
                
                failerr "Test $subdir failed! Exiting..."
            fi
        done
    done
fi

if [ "$nuget" -eq 0 ]
then
    for projdir in $scriptroot/src
    do
        if [ "$projdir" != "packages" ] && [ "$projdir" != ".vs" ]
        then
            cd $projdir/bin/$config
            cp $projdir.dll $scriptroot/nuget/lib
            cd $scriptroot/nuget
            rm *.nupkg
            
            for spec in *.nuspec
            do
                ./NuGet pack $spec
            done
        fi
    done
fi
