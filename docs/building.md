# Building the Repo

## Prerequisites

libvideo requires a working copy of Bash to build. This means that if you are on Windows (like I am) and you're somehow using GitHub without having installed Git Bash (dear God why), you will have to grab the latest version of it from [here](http://www.git-scm.com/downloads). You should also be able to do this on Cygwin and MSYS as well, but those options haven't been tested.

You should also be able to build this from Linux or OS X as long as you have a working installation of Mono, but this hasn't been tested as of yet.

## Running the build script

First, make sure you've cloned the repoistory:

```
cd ~
git clone https://github.com/James-Ko/libvideo.git
cd libvideo
```

To run the build script:

    ./build.sh

Once this is done, you'll find a fresh batch of the core DLLs in `src/libvideo/bin/Release/` as well as the other projects in the solution.

## Passing options to the build script

By default, the script will only build the solution in `src/` as part of the build process. If you'd like to e.g. run tests or set the configuration to Debug during the build, run `./build.sh --help` to see a list of options.
