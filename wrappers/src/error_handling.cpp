////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//////////////////////////////////////////////////////////////////////////// 
#include <stdexcept>
#include <sstream>
#include <cassert>
#include <realm/object-store/shared_realm.hpp>
#include <realm/object-store/object_store.hpp>
#include "realm_export_decls.hpp"
#include "error_handling.hpp"
#include "shared_realm_cs.hpp"

// core headers for exception types
#include <realm/util/file.hpp> 
#include <realm/alloc_slab.hpp>
#include <realm/object-store/object_accessor.hpp>

using namespace realm::app;

namespace realm {
    /**
    @note mostly copied from util.cpp in Java but has a much richer range of exceptions
    @warning if you update these codes also update the matching RealmExceptionCodes.cs
    */
    NativeException convert_exception(std::exception_ptr err)
    {
        try {
            if (err == nullptr) {
                throw;  // rethrow so we can add typing information by different catches
            }
            else {
                std::rethrow_exception(err);
            }

        }
        catch (const CustomException& e) {
            return NativeException(e, e.custom_error);
        }
        catch (const ManagedExceptionDuringCallback& e) {
            return NativeException(e, CustomError::None, e.m_managed_error);
        }
        catch (const AppError& e) {
            REALM_ASSERT_DEBUG(e.additional_status_code == util::none);
            REALM_ASSERT_DEBUG(e.link_to_server_logs == "");

            return NativeException(e);
        }
        catch (const SyncError& e) {
            REALM_ASSERT_DEBUG(false);

            return NativeException(e.status.code(), e.status.reason());
        }
        catch (const Exception& e) {
            return NativeException(e);
        }
        catch (const std::exception& e) {
            return NativeException(ErrorCodes::Error::UnknownError, e.what());
        }
        catch (...) {
            return NativeException(ErrorCodes::Error::UnknownError, "Unknown exception caught which doesn't descend from std::exception");
        }
    }

}   // namespace realm
