#ifndef REALM_TIMESTAMP_HELPERS_HPP
#define REALM_TIMESTAMP_HELPERS_HPP


// copied from realm-java realm/realm-jni/src/util.hpp

#include <cstdint>
#include <realm/timestamp.hpp>

const int64_t unix_epoch_ticks = 621355968000000000;
const int64_t ticks_per_second = 10000000;
const int64_t nanoseconds_per_tick = 100;

inline int64_t to_ticks(const realm::Timestamp& ts)
{
    const int64_t seconds = ts.get_seconds();
    const int32_t nanoseconds = ts.get_nanoseconds();
    const int64_t ticks = seconds * ticks_per_second + nanoseconds / nanoseconds_per_tick + unix_epoch_ticks;
    return ticks;
}

inline realm::Timestamp from_ticks(int64_t ticks)
{
    const int64_t unix_ticks = ticks - unix_epoch_ticks;
    const int64_t seconds = unix_ticks / ticks_per_second;
    const int32_t nanoseconds = unix_ticks % ticks_per_second * nanoseconds_per_tick;
    return realm::Timestamp(seconds, nanoseconds);
}


#endif  // REALM_TIMESTAMP_HELPERS_HPP
