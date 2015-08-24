# Building the Repo

## Prerequisites

- MSBuild
- A C# 6 compiler
- a working copy of Bash (Windows users can install [Git Bash](http://www.git-scm.com/downloads))
 - cURL or wget

You should be able to build the repo from any OS as long as it meets the above requirements.

## Running the build script

First, make sure you've cloned the repoistory:

```
cd ~
git clone https://github.com/James-Ko/libvideo.git
cd libvideo
```

To run the build script for the first time, you'll need to specify the directory of `MSBuild.exe` (adjust accordingly):

    ./build.sh --msbuild "/C/Program Files (x86)/MSBuild/14.0/Bin"

Once this is done, you'll find a fresh batch of the core DLLs in `src/libvideo/bin/Release/` as well as the other projects in the solution.

### Passing options to the build script

By default, the script will only build the solution in `src/` as part of the build process. If you'd like to e.g. run tests or set the configuration to Debug during the build, run `./build.sh --help` to see a list of options.
