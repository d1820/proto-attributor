# ![alt text](./ProtoAttributor/logo.png "Title")

ProtoAttributor is an open source Visual Studio extension that can manage the appropriate attributes on a class to support ProtoBuf. It currently supports ProtoContract, ProtoMember, ProtoIgnore attributes. This extension lets you Add, Reorder, and Remove ProtoBuf attributes from a class.

One of the challenges with creating proper ProtoBuf contracts is getting the ordering correct and consistent. 
While small contract classes are easy to manage as classes get larger or lots of nested classes are created it gets harder and harder to manage and maintain these classes.
This is where ProtoAttributor shines. You can Add, Reorder, Remove ProtoBuf attributes from 1 or many classes.

## Adding Attributes

Adding attributes options not only adds the ProtoMember attributes, it will also ensure the class has the ProtoContract attribute and the proper using are applied.

Have an existing class that is already attributed and numbered, but you need to add new properties and attributes, ProtoAttributor will examine the existing attributes and start numbering at the next highest number ensuring proper backward and forward compatibility for your proto contract.

## Reordering Attributes

Reordering attributes now becomes a snap. this will restart the index at 1 and reset all the ProtoMembers in proper ascending order.
This is helpful when contracts have not been releases yet and you want to ensure standarized numbering of your contracts.

## Removing Attributes

Removing attributes will not only remove the ProtoMember attributes but also includes any attribute or using that is related to [Proto*] family.



<span style=" font-size:10px;">
Special thanks to <a href="https://logomakr.com/">logomakr.com</a> for the logo
</span>
