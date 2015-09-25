/// @warning Keep these codes aligned with ExceptionsToManaged.h in wrappers
namespace RealmNet
{
    /**
     * These numeric codes are based on the Java "test" codes in Util.java
     * To aid anyone comparing code, they have retained the same names.
     */ 
    public enum RealmExceptionCodes
    {
        Exception_ClassNotFound=0,
        Exception_NoSuchField=1,
        Exception_NoSuchMethod=2,
        Exception_IllegalArgument=3,
        Exception_IOFailed=4,
        Exception_FileNotFound=5,
        Exception_FileAccessError=6,
        Exception_IndexOutOfBounds=7,
        Exception_TableInvalid=8,
        Exception_UnsupportedOperation=9,
        Exception_OutOfMemory=10,
        Exception_FatalError=11,
        Exception_RuntimeError=12,
        Exception_RowInvalid=13,
        Exception_EncryptionNotSupported=14     
    }
}

