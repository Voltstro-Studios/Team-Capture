// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;

namespace Team_Capture.Core.UserAccount
{
    public class UserAccountAlreadyExistsException : Exception
    {
	    public UserAccountAlreadyExistsException(string message) : base(message)
	    {
	    }
    }
}