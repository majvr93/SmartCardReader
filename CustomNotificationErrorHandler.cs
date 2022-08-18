using Newtonsoft.Json;
using SmartCardReader.DTO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;

namespace SmartCardReader
{
    public class CustomNotificationErrorHandler : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            if (NotificationErrorContext.HasNotifications)
            {
                NotificationError _notification = NotificationErrorContext.Notifications.First();
                NotificationErrorResponse response = new NotificationErrorResponse((long)_notification.ErrorCode, _notification.ErrorMsg);
                var jsonData = JsonConvert.SerializeObject(new DomainResult<NotificationErrorResponse>(response, false));               
                actionContext.Response = new HttpResponseMessage(_notification.StatusCode)
                {
                    Content = new StringContent(jsonData, UnicodeEncoding.UTF8, "application/json")
                };

                return;
            }
        }

    }
}
