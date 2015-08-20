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

project() {
    if [ "$1" != "packages" ] && [ "$1" != ".vs" ] && [ "${1##*.}" != "sln" ]
    then
        return 0
    fi
    
    return 1
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
    echo "Building src..."
    cd $scriptroot/src
    $scriptroot/MSBuild /property:Configuration=$config
    
    failerr "MSBuild failed on libvideo.sln! Exiting..."
fi

if [ "$test" -eq 0 ]
then
    echo "Running tests..."

    for test in $scriptroot/tests/*
    do
        echo "Building test $test..."
        cd $test
        $scriptroot/MSBuild /property:Configuration=$config
        
        failerr "MSBuild failed on $test.sln! Exiting..."
        
        for subtest in *
        do
            if project "$subtest"
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
        if project "$projdir"
        then
            echo "Getting assemblies from $projdir..."
            cd $projdir/bin/$config
            mkdir $scriptroot/nuget/lib 2> /dev/null
            cp $projdir.dll $scriptroot/nuget/lib
            
            echo "Cleaning existing packages..."
            cd $scriptroot/nuget
            rm *.nupkg
            
            for spec in *.nuspec
            do
                echo "Packing $spec..."
                ./NuGet pack $spec
                
                failerr "Packing $spec failed! Exiting..."
            done
        fi
    done
fi
