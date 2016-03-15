/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#ifndef DEBUG_HPP
#define DEBUG_HPP

#include <string>

namespace realm {

#ifndef NDEBUG
void debug_log(std::string message);
#endif

} // namespace realm

#endif // DEBUG_HPP
