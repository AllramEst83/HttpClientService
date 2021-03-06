﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HttpClientService.Helpers
{
    public static class HttpConstants
    {
        public const string Authorization = "Authorization";

        public const int UnauthorizedStatusCode = 401;
        public const string UnauthorizedError = "Unauthorized access";
        public const string UnauthorizedDescription = "Validation error. Token is not valid or missing";
        public const string UnauthorizedCode = "unauthorized_access";

        public const int ForbiddenStatusCode = 403;
        public const string ForbiddenError = "Forbidden Access.";
        public const string ForbiddenDescription = "User is not authorized to access this resource";
        public const string ForbiddenCode = "forbidden_access";

        public const int InternalServerErrorStatusCode = 500;
        public const string InternalServerError_Error = "Internal server error.";
        public const string InternalServerErrorDescription = "The server could not complete the request at this time. Please refresh the page.";
        public const string InternalServerErrorCode = "internal_server_error";

        public const string MediaContentType_APPLICATION_JSON = "application/json";

    }
}
