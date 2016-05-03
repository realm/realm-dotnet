#ifndef REALM_TIMESTAMP_HELPERS_HPP
#define REALM_TIMESTAMP_HELPERS_HPP


// copied from realm-java realm/realm-jni/src/util.hpp

#include <cstdint>
#include <realm/timestamp.hpp>

inline int64_t to_milliseconds(const realm::Timestamp& ts)
{
    // From core's reference implementation aka unit test
    // FIXME: check for overflow - for about 400 years in  the future
    const int64_t seconds = ts.get_seconds();
    const int32_t nanoseconds = ts.get_nanoseconds();
    const int64_t milliseconds = seconds * 1000 + nanoseconds / 1000000; // This may overflow
    return milliseconds;
}


inline realm::Timestamp from_milliseconds(int64_t milliseconds)
{
    // From core's reference implementation aka unit test
    int64_t seconds = milliseconds / 1000;
    int32_t nanoseconds = (milliseconds % 1000) * 1000000;
    return realm::Timestamp(seconds, nanoseconds);
}



#endif  // REALM_TIMESTAMP_HELPERS_HPP
