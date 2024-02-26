# Building for VS2019

- Open solution in vs2019
- Make any changes needed
- Run unit tests


# Building for VS2022

- Open solution in vs2022
- Make any changes needed
- Run unit tests

# Adding a new version of Visual Studio

- Create a new version folder in the Manifests folder
- Copy over vs2022\source.extension.vsixmanifest
- Make changes for new Visual Studio version
- Update ProtoAttributor proj file 
- Add a new entry for target version
    ```XML
    <VsTargetVersion Condition="'$(VsTargetVersion)' == '' and '$(VisualStudioVersion)' == '1X.0' ">VS20XX</VsTargetVersion>
    ```
- Add a new when clause to choose (Update XX to VS year, Update nuget packages)
    ```XML
     <When Condition="'$(VsTargetVersion)' == 'VS20XX'">
          <PropertyGroup>
            <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
            <AssemblyName>ProtoAttributor20XX</AssemblyName>
          </PropertyGroup>
          <ItemGroup>
            <None Include="..\Manifests\vs2022\source.extension.vsixmanifest" Link="source.extension.vsixmanifest">
              <SubType>Designer</SubType>
            </None>
            <PackageReference Include="Microsoft.CodeAnalysis">
              <Version>X.8.0</Version>
            </PackageReference>
            <PackageReference Include="Microsoft.VisualStudio.SDK" Version="1X.0.0-previews-1-31410-273" ExcludeAssets="runtime" />
            <PackageReference Include="Microsoft.VSSDK.BuildTools" Version="1X.0.3177-preview3" />
          </ItemGroup>
        </When>
    ```
	
More Info

[migrate-vsix-to-vs2022](https://cezarypiatek.github.io/post/migrate-vsix-to-vs2022/)