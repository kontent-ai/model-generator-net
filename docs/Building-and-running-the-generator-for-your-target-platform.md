

* Clone the repository
* Navigate to the `kontent-generators-net\src\KontentModelGenerator` folder
* Run `dotnet build -r <RID>` to build the app. See the [list of all RIDs](https://docs.microsoft.com/en-us/dotnet/articles/core/rid-catalog).
* Run `dotnet publish -c release -r <RID>` to publish the app

```
dotnet run --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]
```

