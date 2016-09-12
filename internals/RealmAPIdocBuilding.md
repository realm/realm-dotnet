Building the API Doc
====================

The docs are built with [Doxygen](http://www.doxygen.org) with some customisation, using the configuration in `realmPublicAPI.doxy`. It is easier to use the _Doxywizard_ GUI app installed with Doxygen to edit the config and it provides mouse-over help for fields.

Rather than using _traditional_ Doxygen markup, the classes and members are documented using the more wordy [XML-based](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx) Microsoft style. This provides Intellisense documentation inside both the Visual Studio and Xamarin Studio IDEs, when you mouse-over symbols.

Doxygen has [support for most](http://www.stack.nl/~dimitri/doxygen/manual/xmlcmds.html) of the XML commands.

The Doxygen output has been customised to provide responsive layout using the Twitter Bootstrap framework, based on the samples provided at [Velron/doxygen-bootstrapped](https://github.com/Velron/doxygen-bootstrapped/). Files copied and renamed to be different from the default names Doxygen generates:

* `header.html` copied to `realmheader.html`
* `footer.html` copied to `realmfooter.html`
* `customdoxygen.css` copied to `realmcustomdoxygen.css`
* `doxy-boot.js`

The files are pointed to by matching settings in the HTML section of the config.

## Custom Layout
The `realmDoxygenLayout.xml` file was generated from doxygen with the `doxygen -l` command (tip, if you have Doxygen installed on a Mac, you need to _Display Package Contents_ on the app to get the commandline doxygen nested inside).

Tabs were hidden which cannot be configured normall, by setting their attribute `visible=no`:

* `pages` tab
* `classes` tab
* `classlist` tab

The nested tabs under the `classes` tab were moved out to appear directly under `navindex`.

The Class Index is not generated, using the Doxygen configuration, because its role was adequately covered by the Class List. The index is of little use for such a simple API.

Similarly, we disable the graphical Class Hierarchy so that the textual version is seen immediately.