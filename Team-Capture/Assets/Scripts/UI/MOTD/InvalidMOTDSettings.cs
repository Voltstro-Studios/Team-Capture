using System;

namespace Team_Capture.UI.MOTD
{
    public class InvalidMOTDSettings : Exception
    {
	    public InvalidMOTDSettings(string message) : base(message)
	    {
	    }
    }
}