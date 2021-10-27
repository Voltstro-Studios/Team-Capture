// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Core.Networking
{
    public enum HttpCode
    {
        //Success
        Ok = 200,

        //Client
        Unauthorized = 401,
        PreconditionFailed = 412,

        //Server
        InternalServerError = 500
    }
}