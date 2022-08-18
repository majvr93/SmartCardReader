using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SmartCardReader
{
    public static class NotificationErrorContext
    {
        private static List<NotificationError> _notifications;
        public static IReadOnlyList<NotificationError> Notifications => _notifications;
        public static bool HasNotifications => _notifications.Any();
        
        public static void AddNotification(NotificationError notification)
        {
            _notifications.Add(notification);
        }
        public static void InitContext()
        {
            _notifications = new List<NotificationError>();
        }

    }

    public class NotificationError
    {
        public NotificationError(ErrorCode cod, string msg, HttpStatusCode statusCode)
        {
            this.ErrorCode = cod;
            this.ErrorMsg = msg;
            this.StatusCode = statusCode;
        }       
        public ErrorCode ErrorCode { get; }
        public string ErrorMsg { get; }
        public HttpStatusCode StatusCode { get; }    
    }

    public enum ErrorCode
    {
        ExceptionError = 100,
        CardReader_NotFound = 200,
        CC_NotFound = 201,
        COM_NotFound = 202,
        Certificates_NotFound = 203,
        SigningCertificate_NotFound = 204,
        InvalidIdentification = 300
    }

}


