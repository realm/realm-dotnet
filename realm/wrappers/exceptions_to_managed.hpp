#ifndef EXCEPTIONS_TO_MANAGED_H
#define EXCEPTIONS_TO_MANAGED_H


/// @warning Keep these codes aligned with RealmExceptionCodes.cs

#include <exception>
#include <string>

namespace realm {
    /**
    * These numeric codes are based on the Java "test" codes in Util.java
    * To aid anyone comparing code, they have retained the same names.
    */
    enum class RealmExceptionCodes : size_t {
        Exception_ClassNotFound = 0,
        Exception_NoSuchField = 1,
        Exception_NoSuchMethod = 2,
        Exception_IllegalArgument = 3,
        Exception_IOFailed = 4,
        Exception_FileNotFound = 5,
        Exception_FileAccessError = 6,
        Exception_IndexOutOfBounds = 7,
        Exception_TableInvalid = 8,
        Exception_UnsupportedOperation = 9,
        Exception_OutOfMemory = 10,
        Exception_FatalError = 11,
        Exception_RuntimeError = 12,
        Exception_RowInvalid = 13,
        Exception_EncryptionNotSupported = 14
    };


    void ThrowManaged(const std::exception& exc, RealmExceptionCodes exceptionCode, const std::string& message = "");
    void ThrowManaged();
}  // namespace realm

#endif  // EXCEPTIONS_TO_MANAGED_H
