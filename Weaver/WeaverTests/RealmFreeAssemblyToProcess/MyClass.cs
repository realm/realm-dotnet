/*
 * This is a trivial project to test a scenario which may occur when people start development.

Say someone goes through very incremental development as they start to use Realm:

1. Create a new Solution
2. Add the Realm NuGet
3. Start declaring classes descending from `RealmObject` to use in their app.

If they try to build at each step, then step 2 has the Fody weaver linked in and looking
for `RealmObject` classes but none are present.

This is a trivial and fleeting state but must be robust.

So, this project exists with (fake) Realm included but doing nothing with it, and no `RealmObject`
subclasses.

It should always build without any errors and only a minor compilation warning from Fody

*/
using System;
namespace RealmFreeAssemblyToProcess
{
    public class MyClass
    {
        public MyClass()
        {
        }
    }
}
