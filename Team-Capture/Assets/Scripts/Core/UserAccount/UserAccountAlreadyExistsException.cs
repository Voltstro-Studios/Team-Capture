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