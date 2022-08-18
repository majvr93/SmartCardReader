using SmartCardReader.DTO;
using SmartCardReader.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmartCardReader.Contorllers
{
    [CustomNotificationErrorHandler]
    public class SmartCardReaderController : ApiController
    {
        private SmartCardReaderService _smartCardReader;
        public SmartCardReaderController()
        {
            NotificationErrorContext.InitContext();
            _smartCardReader = new SmartCardReaderService();
        }

        [HttpGet]
        [ActionName("Fetch")]
        public HttpResponseMessage Fetch()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new DomainResult<string>("Ready!", true));
        }

        [HttpGet]
        [ActionName("CertificadosCC")]
        public HttpResponseMessage CertificadosCC(int idNumber)
        {
            var certificados = _smartCardReader.GetListCertificadosCC(idNumber);

            return Request.CreateResponse(HttpStatusCode.OK, new DomainResult<List<CertificateResponse>>(certificados, true));
        }

        [HttpGet]
        [ActionName("CertificadosCOM")]
        public HttpResponseMessage CertificadosCOM(int nProfCell)
        {
            var certificados = _smartCardReader.GetListCertificadosCOM(nProfCell);

            return Request.CreateResponse(HttpStatusCode.OK, new DomainResult<List<CertificateResponse>>(certificados, true));
        }

        [HttpPost]
        [ActionName("SignXML")]
        public async Task<HttpResponseMessage> SignXMLAsync(string Id, string Cartao)
        {
            var body = await Request.Content.ReadAsStringAsync();
            var _signedXML = _smartCardReader.SignXML(Id,Cartao,body);
            
            var response = Request.CreateResponse(HttpStatusCode.OK, _signedXML);
            response.Content = new StringContent(_signedXML.OuterXml, Encoding.UTF8, "application/xml");
            return response;
        }
    }
}
