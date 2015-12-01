Building the API Doc
====================

The docs are built with [Doxygen](http://www.doxygen.org) with some customisation.

Rather than using _traditional_ Doxygen markup, the classes and members are documented using the more wordy [XML-based](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx) Microsoft style. This provides Intellisense documentation inside both the Visual Studio and Xamarin Studio IDEs, when you mouse-over symbols.

Doxygen has [support for most](http://www.stack.nl/~dimitri/doxygen/manual/xmlcmds.html) of the XML commands.

The Doxygen output has been customised to provide responsive layout using the Twitter Bootstrap framework, based on the samples provided at [Velron/doxygen-bootstrapped](https://github.com/Velron/doxygen-bootstrapped/).