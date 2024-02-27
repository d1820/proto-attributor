![ProtoAttributor](https://github.com/d1820/proto-attributor/blob/main/visual-studio/ProtoAttributor/logo.png?raw=true)

![GitHub CI](https://img.shields.io/github/actions/workflow/status/d1820/proto-attributor/dotnet.yml)
![GitHub License](https://img.shields.io/github/license/d1820/proto-attributor)
![Visual Studio Marketplace Version (including pre-releases)](https://img.shields.io/visual-studio-marketplace/v/DanTurco.ProtoAttributor2022)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/DanTurco.ProtoAttributor2022)


ProtoAttributor is an open source Visual Studio extension that can manage the appropriate attributes on a class to support ProtoBuf.
It currently supports ProtoContract, ProtoMember, ProtoIgnore, DataContract, DataMember, IgnoreDataMember attributes. This extension lets you Add, Reorder, and Remove ProtoBuf attributes from a class.
This works in conjunction with the [protobuf-net](https://github.com/protobuf-net/protobuf-net) 3.0+ Nuget package


One of the challenges with creating proper ProtoBuf contracts is getting the ordering correct and consistent.
While small contract classes are easy to manage as classes get larger or lots of nested classes are created it gets harder and harder to manage and maintain these classes.
This is where ProtoAttributor shines. You can Add, Reorder, Remove ProtoBuf attributes from 1 or many classes.

![Preview](https://github.com/d1820/proto-attributor/blob/main/visual-studio/ProtoAttributor/ProtoImagePreview.jpg?raw=true)

## Adding Attributes

Adding attributes options not only adds the ProtoMember/DataMember attributes, it will also ensure the class has the ProtoContract/DataContract attributes and the proper using statements are applied.

Have an existing class that is already attributed and numbered, but you need to add new properties and attributes, ProtoAttributor will examine the existing attributes and start numbering at the next highest number ensuring proper backward and forward compatibility for your proto contract.

## Reordering Attributes

Reordering attributes now becomes a snap. this will restart the index at 1 and reset all the ProtoMembers/DataMembers in proper ascending order.
This is helpful when contracts have not been releases yet and you want to ensure standardized numbering of your contracts.

## Removing Attributes

Removing attributes will not only remove the ProtoMember/DataMember attributes but also includes any attribute or using statements that are related to the [Proto*] or [Data*] family.

## Ways to Use

ProtoAttributor can handle single files already open in Visual Studio from the Tools menu or it can handle entire directories of files from the solution explorer.

## Proto[Attributor] In Action

### Working With ProtoMembers

![alt ProtoContractVideo](https://github.com/d1820/proto-attributor/blob/main/visual-studio/ProtoAttributor/Resources/ProtoContractVideo.gif?raw=true)

### Working With DataMembers

![alt DataContractVideo](https://github.com/d1820/proto-attributor/blob/main/visual-studio/ProtoAttributor/Resources/DataContractVideo.gif?raw=true)


### Working With A Single File

![alt SinglePageProtoActions](https://github.com/d1820/proto-attributor/blob/main/visual-studio/ProtoAttributor/Resources/SinglePageProtoActions.gif?raw=true)



<span style="font-size:10px;">
Special thanks to <a href="https://logomakr.com/">logomakr.com</a> for the logo
</span>
