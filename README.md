# Coree.NETStandard [![Master .NET](https://github.com/carsten-riedel/Coree.NETStandard/actions/workflows/dotnet-master.yml/badge.svg?branch=master)](https://github.com/carsten-riedel/Coree.NETStandard/actions/workflows/dotnet-master.yml) [![Nuget](https://img.shields.io/nuget/v/Coree.NETStandard?label=NuGet&labelColor=004880&logo=NuGet&logoColor=white)](https://www.nuget.org/packages/Coree.NETStandard)

![brand](https://raw.githubusercontent.com/carsten-riedel/Coree.NETStandard/master/images/brand.png)

Coree.NETStandard is a comprehensive library that provides a wide range of generic helper functionalities, designed to enhance and simplify coding tasks across .NET Standard projects.

### Documentation

As Coree.NETStandard is exclusively a library project, you can thoroughly explore its functionalities through the Docfx API documentation. This documentation is available in a Markdown format similar to this text, ensuring easy access and understanding for developers.

[Coree.NETStandard Docfx API](https://carsten-riedel.github.io/Coree.NETStandard/docfx/index.html)

### Usage

For deploying, the strategy varies based on the branch type:

 - Packages from feature, develop, release and master branches will be published to a personal GitHub package repository.
 - Deployments from release branches target NuGet's integration environment.
 - Deployments from master branches target the main NuGet repository.


Here are the commands to configure the NuGet sources for each scenario:

```
# feature, develop, release and master packages
dotnet nuget add source https://nuget.pkg.github.com/carsten-riedel/index.json -n GithubCoreeRepo

# release packages
dotnet nuget add source https://apiint.nugettest.org/v3/index.json -n int.nugettest.org

# master packages (normaly already configured)
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```


