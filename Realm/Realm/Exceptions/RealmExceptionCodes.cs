﻿////////////////////////////////////////////////////////////////////////////
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

namespace Realms.Exceptions
{
    /// <summary>Codes used in forwarding exceptions from the native C++ core, to be regenerated in C#.</summary>
    /// <remarks> <b>Warning:</b> Keep these codes aligned with realm_error_type.hpp in wrappers.</remarks>
    internal enum RealmExceptionCodes : int
    {
        RLM_ERR_NONE = 0,
        RLM_ERR_RUNTIME = 1000,
        RLM_ERR_RANGE_ERROR = 1001,
        RLM_ERR_BROKEN_INVARIANT = 1002,
        RLM_ERR_OUT_OF_MEMORY = 1003,
        RLM_ERR_OUT_OF_DISK_SPACE = 1004,
        RLM_ERR_ADDRESS_SPACE_EXHAUSTED = 1005,
        RLM_ERR_MAXIMUM_FILE_SIZE_EXCEEDED = 1006,
        RLM_ERR_INCOMPATIBLE_SESSION = 1007,
        RLM_ERR_INCOMPATIBLE_LOCK_FILE = 1008,
        RLM_ERR_INVALID_QUERY = 1009,
        RLM_ERR_BAD_VERSION = 1010,
        RLM_ERR_UNSUPPORTED_FILE_FORMAT_VERSION = 1011,
        RLM_ERR_MULTIPLE_SYNC_AGENTS = 1012,
        RLM_ERR_OBJECT_ALREADY_EXISTS = 1013,
        RLM_ERR_NOT_CLONABLE = 1014,
        RLM_ERR_BAD_CHANGESET = 1015,
        RLM_ERR_SUBSCRIPTION_FAILED = 1016,
        RLM_ERR_FILE_OPERATION_FAILED = 1017,
        RLM_ERR_FILE_PERMISSION_DENIED = 1018,
        RLM_ERR_FILE_NOT_FOUND = 1019,
        RLM_ERR_FILE_ALREADY_EXISTS = 1020,
        RLM_ERR_INVALID_DATABASE = 1021,
        RLM_ERR_DECRYPTION_FAILED = 1022,
        RLM_ERR_INCOMPATIBLE_HISTORIES = 1023,
        RLM_ERR_FILE_FORMAT_UPGRADE_REQUIRED = 1024,
        RLM_ERR_SCHEMA_VERSION_MISMATCH = 1025,
        RLM_ERR_NO_SUBSCRIPTION_FOR_WRITE = 1026,
        RLM_ERR_OPERATION_ABORTED = 1027,

        RLM_ERR_SYSTEM_ERROR = 1999,

        RLM_ERR_LOGIC = 2000,
        RLM_ERR_NOT_SUPPORTED = 2001,
        RLM_ERR_BROKEN_PROMISE = 2002,
        RLM_ERR_CROSS_TABLE_LINK_TARGET = 2003,
        RLM_ERR_KEY_ALREADY_USED = 2004,
        RLM_ERR_WRONG_TRANSACTION_STATE = 2005,
        RLM_ERR_WRONG_THREAD = 2006,
        RLM_ERR_ILLEGAL_OPERATION = 2007,
        RLM_ERR_SERIALIZATION_ERROR = 2008,
        RLM_ERR_STALE_ACCESSOR = 2009,
        RLM_ERR_INVALIDATED_OBJECT = 2010,
        RLM_ERR_READ_ONLY_DB = 2011,
        RLM_ERR_DELETE_OPENED_REALM = 2012,
        RLM_ERR_MISMATCHED_CONFIG = 2013,
        RLM_ERR_CLOSED_REALM = 2014,
        RLM_ERR_INVALID_TABLE_REF = 2015,
        RLM_ERR_SCHEMA_VALIDATION_FAILED = 2016,
        RLM_ERR_SCHEMA_MISMATCH = 2017,
        RLM_ERR_INVALID_SCHEMA_VERSION = 2018,
        RLM_ERR_INVALID_SCHEMA_CHANGE = 2019,
        RLM_ERR_MIGRATION_FAILED = 2020,
        RLM_ERR_TOP_LEVEL_OBJECT = 2021,

        RLM_ERR_INVALID_ARGUMENT = 3000,
        RLM_ERR_PROPERTY_TYPE_MISMATCH = 3001,
        RLM_ERR_PROPERTY_NOT_NULLABLE = 3002,
        RLM_ERR_READ_ONLY_PROPERTY = 3003,
        RLM_ERR_MISSING_PROPERTY_VALUE = 3004,
        RLM_ERR_MISSING_PRIMARY_KEY = 3005,
        RLM_ERR_UNEXPECTED_PRIMARY_KEY = 3006,
        RLM_ERR_MODIFY_PRIMARY_KEY = 3007,
        RLM_ERR_INVALID_QUERY_STRING = 3008,
        RLM_ERR_INVALID_PROPERTY = 3009,
        RLM_ERR_INVALID_NAME = 3010,
        RLM_ERR_INVALID_DICTIONARY_KEY = 3011,
        RLM_ERR_INVALID_DICTIONARY_VALUE = 3012,
        RLM_ERR_INVALID_SORT_DESCRIPTOR = 3013,
        RLM_ERR_INVALID_ENCRYPTION_KEY = 3014,
        RLM_ERR_INVALID_QUERY_ARG = 3015,
        RLM_ERR_NO_SUCH_OBJECT = 3016,
        RLM_ERR_INDEX_OUT_OF_BOUNDS = 3017,
        RLM_ERR_LIMIT_EXCEEDED = 3018,
        RLM_ERR_OBJECT_TYPE_MISMATCH = 3019,
        RLM_ERR_NO_SUCH_TABLE = 3020,
        RLM_ERR_TABLE_NAME_IN_USE = 3021,
        RLM_ERR_ILLEGAL_COMBINATION = 3022,
        RLM_ERR_BAD_SERVER_URL = 3023,

        RLM_ERR_CUSTOM_ERROR = 4000,

        RLM_ERR_CLIENT_USER_NOT_FOUND = 4100,
        RLM_ERR_CLIENT_USER_NOT_LOGGED_IN = 4101,
        RLM_ERR_CLIENT_APP_DEALLOCATED = 4102,
        RLM_ERR_CLIENT_REDIRECT_ERROR = 4103,
        RLM_ERR_CLIENT_TOO_MANY_REDIRECTS = 4104,

        RLM_ERR_BAD_TOKEN = 4200,
        RLM_ERR_MALFORMED_JSON = 4201,
        RLM_ERR_MISSING_JSON_KEY = 4202,
        RLM_ERR_BAD_BSON_PARSE = 4203,

        RLM_ERR_MISSING_AUTH_REQ = 4300,
        RLM_ERR_INVALID_SESSION = 4301,
        RLM_ERR_USER_APP_DOMAIN_MISMATCH = 4302,
        RLM_ERR_DOMAIN_NOT_ALLOWED = 4303,
        RLM_ERR_READ_SIZE_LIMIT_EXCEEDED = 4304,
        RLM_ERR_INVALID_PARAMETER = 4305,
        RLM_ERR_MISSING_PARAMETER = 4306,
        RLM_ERR_TWILIO_ERROR = 4307,
        RLM_ERR_GCM_ERROR = 4308,
        RLM_ERR_HTTP_ERROR = 4309,
        RLM_ERR_AWS_ERROR = 4310,
        RLM_ERR_MONGODB_ERROR = 4311,
        RLM_ERR_ARGUMENTS_NOT_ALLOWED = 4312,
        RLM_ERR_FUNCTION_EXECUTION_ERROR = 4313,
        RLM_ERR_NO_MATCHING_RULE = 4314,
        RLM_ERR_INTERNAL_SERVER_ERROR = 4315,
        RLM_ERR_AUTH_PROVIDER_NOT_FOUND = 4316,
        RLM_ERR_AUTH_PROVIDER_ALREADY_EXISTS = 4317,
        RLM_ERR_SERVICE_NOT_FOUND = 4318,
        RLM_ERR_SERVICE_TYPE_NOT_FOUND = 4319,
        RLM_ERR_SERVICE_ALREADY_EXISTS = 4320,
        RLM_ERR_SERVICE_COMMAND_NOT_FOUND = 4321,
        RLM_ERR_VALUE_NOT_FOUND = 4322,
        RLM_ERR_VALUE_ALREADY_EXISTS = 4323,
        RLM_ERR_VALUE_DUPLICATE_NAME = 4324,
        RLM_ERR_FUNCTION_NOT_FOUND = 4325,
        RLM_ERR_FUNCTION_ALREADY_EXISTS = 4326,
        RLM_ERR_FUNCTION_DUPLICATE_NAME = 4327,
        RLM_ERR_FUNCTION_SYNTAX_ERROR = 4328,
        RLM_ERR_FUNCTION_INVALID = 4329,
        RLM_ERR_INCOMING_WEBHOOK_NOT_FOUND = 4330,
        RLM_ERR_INCOMING_WEBHOOK_ALREADY_EXISTS = 4331,
        RLM_ERR_INCOMING_WEBHOOK_DUPLICATE_NAME = 4332,
        RLM_ERR_RULE_NOT_FOUND = 4333,
        RLM_ERR_API_KEY_NOT_FOUND = 4334,
        RLM_ERR_RULE_ALREADY_EXISTS = 4335,
        RLM_ERR_RULE_DUPLICATE_NAME = 4336,
        RLM_ERR_AUTH_PROVIDER_DUPLICATE_NAME = 4337,
        RLM_ERR_RESTRICTED_HOST = 4338,
        RLM_ERR_API_KEY_ALREADY_EXISTS = 4339,
        RLM_ERR_INCOMING_WEBHOOK_AUTH_FAILED = 4340,
        RLM_ERR_EXECUTION_TIME_LIMIT_EXCEEDED = 4341,
        RLM_ERR_NOT_CALLABLE = 4342,
        RLM_ERR_USER_ALREADY_CONFIRMED = 4343,
        RLM_ERR_USER_NOT_FOUND = 4344,
        RLM_ERR_USER_DISABLED = 4345,
        RLM_ERR_AUTH_ERROR = 4346,
        RLM_ERR_BAD_REQUEST = 4347,
        RLM_ERR_ACCOUNT_NAME_IN_USE = 4348,
        RLM_ERR_INVALID_PASSWORD = 4349,
        RLM_ERR_SCHEMA_VALIDATION_FAILED_WRITE = 4350,
        RLM_ERR_APP_UNKNOWN = 4351,
        RLM_ERR_MAINTENANCE_IN_PROGRESS = 4352,

        RLM_ERR_WEBSOCKET_GOINGAWAY = 4400,
        RLM_ERR_WEBSOCKET_PROTOCOLERROR = 4401,
        RLM_ERR_WEBSOCKET_UNSUPPORTEDDATA = 4402,
        RLM_ERR_WEBSOCKET_RESERVED = 4403,
        RLM_ERR_WEBSOCKET_NOSTATUSRECEIVED = 4404,
        RLM_ERR_WEBSOCKET_ABNORMALCLOSURE = 4405,
        RLM_ERR_WEBSOCKET_INVALIDPAYLOADDATA = 4406,
        RLM_ERR_WEBSOCKET_POLICYVIOLATION = 4407,
        RLM_ERR_WEBSOCKET_MESSAGETOOBIG = 4408,
        RLM_ERR_WEBSOCKET_INAVALIDEXTENSION = 4409,
        RLM_ERR_WEBSOCKET_INTERNALSERVERERROR = 4410,
        RLM_ERR_WEBSOCKET_TLSHANDSHAKEFAILED = 4411,

        RLM_ERR_CALLBACK = 1000000,
        RLM_ERR_UNKNOWN = 2000000,

        RowDetached = 1000004000,
        RealmClosed = 1000004001,
        DuplicatePrimaryKey = 1000004002,
        InvalidSchema = 1000004003,
        ObjectManagedByAnotherRealm = 1000004004,
        PropertyTypeMismatch = 1000004006,
        KeyAlreadyExists = 1000004007,
        DuplicateSubscription = 1000004008,
        IndexOutOfRange = 1000004009,
    }
}
