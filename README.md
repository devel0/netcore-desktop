# netcore-desktop

[![NuGet Badge](https://buildstats.info/nuget/netcore-desktop)](https://www.nuget.org/packages/netcore-desktop/)

.NET core desktop

- [API Documentation](https://devel0.github.io/netcore-desktop/api/SearchAThing.html)
- [Changelog](https://github.com/devel0/netcore-desktop/commits/master)

<hr/>

- [Quickstart](#quickstart)
- [Examples](#examples)
- [How this project was built](#how-this-project-was-built)

<hr/>

## Quickstart

- [nuget package](https://www.nuget.org/packages/netcore-desktop/)

in .cs file

```csharp
using SearchAThing;
```

in .xaml file specify namespace

```
xmlns:desktop="clr-namespace:SearchAThing;assembly=netcore-desktop"
```

to run examples

```sh
cd netcore-desktop
code .
```

hit F5 to start example ( change by edit [.vscode/launch.json](.vscode/launch.json) )

## Examples

#### 0001

TextBoxSlider example

![](data/img/example-0001.png)

#### 0002

GridSplitterManager example

![](data/img/example-0002.gif)

#### set resource dictionary

- create an app `dotnet new avalonia.mvvm -na yournamespace -n fldname` ( see [avalonia dotnet template](https://github.com/AvaloniaUI/avalonia-dotnet-templates) )
- add netcore-dekstop [nuget pkg](https://www.nuget.org/packages/netcore-desktop/)
- create a `Dictionary1.axaml` into your `/Views` folder

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:analysis2" xmlns:desktop="clr-namespace:SearchAThing;assembly=netcore-desktop">
    <desktop:SmartConverter x:Key="SmartConverter"/>
</ResourceDictionary>
```

- load dictionary resource from `/Views/MainWindow.axaml`

```xml
    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceInclude Source="/Views/Dictionary1.axaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>
```

- set some avprop in your `/ViewModels/MainWindowViewModel.cs` ( see [snippets](https://github.com/devel0/knowledge/blob/85580f932306550df88b3d988657de104eb71584/doc/vscode-tips.md#snippets) )

```csharp
using ReactiveUI;

namespace analysis2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool initialized = false;    
        public bool Initialized
        {
            get => initialized;
            set => this.RaiseAndSetIfChanged(ref initialized, value);
        }
    }
}
```

- usage example

```xml
<StackPanel VerticalAlignment="Center" HorizontalAlignment="Center" 
    IsVisible="{Binding Initialized, Converter={StaticResource SmartConverter}, ConverterParameter=true false true}">
    <TextBlock Text="Initialization in progress..."/>
</StackPanel>
```

- enable prop change activity from `MainWindow.axaml.cs`

```csharp

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext != null)
            {
                var model = DataContext as MainWindowViewModel;
                if (model != null && !model.Initialized)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);

                        model.Initialized = true;
                        System.Console.WriteLine($"INIT");
                    });
                }
            }
        }
```

## How this project was built

```sh
mkdir netcore-desktop
cd netcore-desktop

dotnet new sln
dotnet new classlib -n netcore-desktop

cd netcore-desktop
dotnet add package netcore-util --version 1.6.1
dotnet add package Avalonia --version 0.10.0-preview2
cd ..

dotnet sln add netcore-desktop
dotnet restore
dotnet build
```
