/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 

#ifdef DYNAMIC  // clang complains when making a dylib if there is no main(). :-/
int main() { return 0; }
#endif
