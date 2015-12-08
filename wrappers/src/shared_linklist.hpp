/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

#ifndef SHARED_LINKLIST_HPP
#define SHARED_LINKLIST_HPP

#include <memory>

namespace realm {
  
  /**
   Wrapper class used to hang onto the smart pointer LinkViewRef
   so we can pass a raw pointer back to C#.
   
   @see table_destroy_linklist
  */
  typedef std::shared_ptr<LinkViewRef> SharedLinkViewRef;
}

#endif  // SHARED_LINKLIST_HPP
