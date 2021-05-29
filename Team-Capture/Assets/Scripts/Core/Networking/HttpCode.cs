namespace Team_Capture.Core.Networking
{
    public enum HttpCode
    {
        //Success
        Ok = 200,
        
        //Client
        Unauthorized = 401,
        PreconditionFailed = 412
    }
}