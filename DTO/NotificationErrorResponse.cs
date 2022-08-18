namespace SmartCardReader.DTO
{
    public class NotificationErrorResponse
    {
        public NotificationErrorResponse(long code, string error)
        {
            this.ErrorCode = code;
            this.ErrorMsg = error;
        }
        public long ErrorCode { get;}
        public string ErrorMsg{ get; }
    }
}
