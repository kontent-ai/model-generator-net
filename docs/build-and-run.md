# Building and running the generator fo your target platform

* Clone the repository
* Navigate to the `model-generator-net\src\KontentModelGenerator` folder
* Run `dotnet build -r <RID>` to build the app. See the [list of all RIDs](https://docs.microsoft.com/en-us/dotnet/articles/core/rid-catalog).
* Run `dotnet publish -c release -r <RID>` to publish the app

```sh
dotnet run --environmentid "<environmentid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]
```
