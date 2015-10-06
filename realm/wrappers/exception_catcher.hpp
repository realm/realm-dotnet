/*
* Copyright 2015 Realm Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#ifndef EXCEPTION_CATCHER_H
#define EXCEPTION_CATCHER_H


void ConvertException(const char *file, int line);

/**
Leanest possible way to catch exceptions and invoke function which rethrows to classify them.
@note nearly the same as the Java util.hpp definition
*/
#define CATCH_STD catch (...) {  ConvertException(__FILE__, __LINE__);  }

#endif  //  EXCEPTION_CATCHER_H
