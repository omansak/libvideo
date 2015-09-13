# Building the Repo

## Prerequisites

- MSBuild
- A C# 6 compiler
- a working copy of Bash (Windows users can install [Git Bash](http://www.git-scm.com/downloads))
 - cURL or wget

In addition to above, Mono 4.0.1 or higher is required if building from Unix.

### PCL reference assemblies

Linux users must install the `referenceassemblies-pcl` package before attempting to build. If you're not one of them, skip this.

**For Debian/Ubuntu users:**

    sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
    echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
    echo "deb http://jenkins.mono-project.com/repo/debian sid main" | sudo tee /etc/apt/sources.list.d/mono-jenkins.list
    sudo apt-get update
    sudo apt-get install referenceassemblies-pcl

**For Fedora/CentOS users:**

    sudo rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
    sudo yum-config-manager --add-repo http://download.mono-project.com/repo/centos/
    sudo yum-config-manager --add-repo http://jenkins.mono-project.com/repo/centos/
    sudo yum upgrade
    sudo yum install referenceassemblies-pcl

## Running the build script

First, make sure you've cloned the repoistory:

```
cd ~
git clone https://github.com/jamesqo/libvideo.git
cd libvideo
```

To run the build script for the first time, you'll need to specify the directory of `MSBuild.exe` (adjust accordingly):

    ./build.sh --msbuild "/C/Program Files (x86)/MSBuild/14.0/Bin"

Once this is done, you'll find a fresh batch of the core DLLs in `src/libvideo/bin/Release/` as well as the other projects in the solution.

### Passing options to the build script

By default, the script will only build the solution in `src/` as part of the build process. If you'd like to e.g. run tests or set the configuration to Debug during the build, run `./build.sh --help` to see a list of options.
