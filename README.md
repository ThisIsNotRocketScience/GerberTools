# GerberTools
Tools to load/edit/create/panelize/prerender sets of gerber files.

A recent build is available on http://blog.thisisnotrocketscience.nl/projects/pcb-panelizer/

All tools are clean C# and will run fine under Mono.

Please see http://www.thisisnotrocketscience.nl for news/updates/blogs

Follow us on twitter: @rocket_not

## Building
### Visual Studio 2015 upwards
- Open the `GerberProjects/GerberProjects.sln` solution and convert it to your version of Visual Studio
(if necessary)
- Ensure your dependencies are up to date via Nuget: https://docs.microsoft.com/en-us/nuget/consume-packages/reinstalling-and-updating-packages
- Build the solution

### Linux
Run `./build.sh`. Dependencies should be automatically fetched.
