This is a fork from ThisIsNotRocketScience/GerberTools

Tools to load/edit/create/panelize/prerender sets of gerber files.

## Updates
### New Features for ProductionFrame - 21/12/18

- Now, a list of fiducial points can be included to give more customization option (position, layer and size) for panel frames
- Frame configuration fields grouped according to its function
- General interface modification

## Building
### Visual Studio 2015 upwards
- Open the `GerberProjects/GerberProjects.sln` solution and convert it to your version of Visual Studio
(if necessary)
- Ensure your dependencies are up to date via Nuget: https://docs.microsoft.com/en-us/nuget/consume-packages/reinstalling-and-updating-packages
- Build the solution

All tools are clean C# and will run fine under Mono.
Please see http://www.thisisnotrocketscience.nl for news/updates/blogs

### Linux

Install `mono-devel` via official instructions: https://www.mono-project.com/download/stable/#download-lin. 
At the time of writing, this installs version 6.12.0 Stable (6.12.0.107), which compiles the project fine, but produces version warnings.
Versions from your distro might also work.


Run `./build.sh`. Nuget dependencies should be automatically fetched.
