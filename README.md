# AlwaysUse

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
![Works with Visual Studio 2019](https://img.shields.io/static/v1.svg?label=VS&message=2019&color=5F2E96)

Automatically add using directives to open C# files.

Include a file called `.alwaysuse` in the same folder as the `*.csproj` or `*.sln` file. (It doesn't need to be included in the solution.)  
On each line of the file, add a namespace you want included in every C# file you open.

When you open any `*.cs` file, the specified namespaces will be added as using directives (if they're not already there.)

Get it from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=MattLaceyLtd.AlwaysUse).

If you have any requests or suggestions, please [raise an issue](https://github.com/mrlacey/alwaysuse/issues/new).

If you like it, please [leave a review on the marketplace](https://marketplace.visualstudio.com/items?itemName=MattLaceyLtd.AlwaysUse&ssr=false#review-details).

---

**NOTE.**  
This was created as an experiment.  
The result of the experiment was that this wasn't especially useful and could easily become frustrating.  
Additionally, as [C#10 introduced global usings](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive#global-modifier), I hope that the original need that prompted this experiment will be much less relevant.

I'm not planning to remove the extension from the marketplace but don't expect to update this code (or extension) going forward.
