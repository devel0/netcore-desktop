# netcore-desktop

[![NuGet Badge](https://buildstats.info/nuget/netcore-desktop)](https://www.nuget.org/packages/netcore-desktop/)

.NET core desktop

- [API Documentation](https://devel0.github.io/netcore-desktop/html/annotated.html)
- [Changelog](https://github.com/devel0/netcore-desktop/commits/master)

<hr/>

<!-- TOC -->
* [Examples](#examples)
    - [0001 ( TextBoxSlider )](#0001--textboxslider-)
    - [0002 ( GridSplitterManager )](#0002--gridsplittermanager-)
* [How this project was built](#how-this-project-was-built)
<!-- TOCEND -->

<hr/>

## Examples

### 0001 ( TextBoxSlider )

[code](https://github.com/devel0/netcore-desktop/blob/ed1b3e8c9bea960242138a0ed183044be5e01083/examples/0001/MainWindow.xaml#L23)

![](data/img/example-0001.png)

### 0002 ( GridSplitterManager )

[code](https://github.com/devel0/netcore-desktop/blob/ed1b3e8c9bea960242138a0ed183044be5e01083/examples/0002/MainWindow.xaml#L40)

![](data/img/example-0002.gif)

## How this project was built

```sh
mkdir netcore-desktop
cd netcore-desktop

dotnet new sln
dotnet new classlib -n netcore-desktop

cd netcore-desktop
dotnet add package netcore-ext --version 1.0.0
dotnet add package Avalonia.Desktop --version 0.10.18
cd ..

dotnet sln add netcore-desktop
dotnet restore
dotnet build
```
