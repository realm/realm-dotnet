# Installing the NuGet packages

Realm for Xamarin consists of two NuGet packages: RealmNet and RealmNetWeaver.
The RealmNet package contains the fundamentals for Realm whereas the RealmNetWeaver 
package contains a [Fody](https://github.com/Fody/Fody) weaver, 
responsible for turning your RealmObject subclasses into persisted ones.

For the private beta, you should have received these packages as raw .nupkg files.
After the beta period, the packages will be available in the official NuGet package
repository.

## Xamarin Studio on OSX

In order to add these to your project, you need to let Xamarin Studio use a folder 
as a NuGet repository. You do this in the menu Xamarin Studio -> Preferences -> NuGet/Sources. 
Click the Add button, type a name like "Local", and set the URL to point to your folder. 
If, say, your OSX username is John and you created a RealmNet folder on your desktop,
this would be "/Users/John/Desktop/". Username and Password should be left blank.

Now, you should be able to add Realm to an iOS project. Click Add Package on your project, 
select your Local (or whatever you named it before) repository in the dropdown in the 
upper left corner, you should be see both packages. 
*It's important that you add RealmNetWeaver first and then RealmNet*. Dependencies don't seem to 
work in local package repositories. 
Select RealmNetWeaver first add it and close the dialog. Open it again and add RealmNet.

At this point, you should have the two packages installed. If your project was already using 
Fody you were asked if you wanted to replace FodyWeavers.xml or not.
Whether you did this or not, the important thing is that you end up with a FodyWeavers.xml file 
that contains all the weavers you want active, including RealmNetWeaver.

This is an example of how your FodyWeavers.xml file would look if you were using the 
[PropertyChanged weaver](https://www.nuget.org/packages/PropertyChanged.Fody/) and Realm:

    <?xml version="1.0" encoding="utf-8" ?>
    <Weavers>
      <PropertyChanged />
      <RealmNetWeaver />
    </Weavers>

