# netcore-desktop

[![NuGet Badge](https://buildstats.info/nuget/netcore-desktop)](https://www.nuget.org/packages/netcore-desktop/)

.NET core desktop

- [API Documentation](https://devel0.github.io/netcore-desktop/html/annotated.html)
- [Changelog](https://github.com/devel0/netcore-desktop/commits/master)

<hr/>

<!-- TOC -->
* [Examples](#examples)
  + [0003 ( GridSplitterManager )](#0003--gridsplittermanager-)
* [How this project was built](#how-this-project-was-built)
* [Documentation (github pages)](#documentation-github-pages)
  + [Build and view locally](#build-and-view-locally)
  + [Build and commit into docs branch](#build-and-commit-into-docs-branch)
<!-- TOCEND -->

<hr/>


## Examples

List of [examples](https://devel0.github.io/netcore-desktop/html/examples.html).

### 0003 ( GridSplitterManager )

![](data/img/example-0002.gif)

## How this project was built

```sh
mkdir netcore-desktop
cd netcore-desktop

dotnet new sln

mkdir src examples

cd src
dotnet new classlib -n netcore-desktop
mv netcore-desktop desktop
cd desktop
dotnet add package netcore-ext
dotnet add package Avalonia.Desktop
cd ../..

cd examples
dotnet new avalonia.mvvm -n example
mv example/example.csproj example/example-0001.csproj
mv example example-0001
cd ..

dotnet sln add src/desktop examples/example-0001

dotnet build

# documentation css

mkdir data
git submodule add https://github.com/jothepro/doxygen-awesome-css.git data/doxygen-awesome-css
cd data/doxygen-awesome-css
# doxygen 1.9.7
git checkout 245c7c94c20eac22730ef89035967f78b77bf405
cd ../..
```

## Documentation (github pages)

Configured through Settings/Pages on Branch docs ( path /docs ).

- while master branch exclude "docs" with .gitignore the docs branch doesn't


### Build and view locally

```sh
./doc build
./doc serve
./doc view
```

### Build and commit into docs branch

```sh
./doc commit
```
