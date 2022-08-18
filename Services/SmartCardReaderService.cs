using Cryptware.NCryptoki;
using pt.portugal.eid;
using SmartCardReader.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SmartCardReader.Services
{
    public class SmartCardReaderService
    {
        public SmartCardReaderService()
        {
        }

        public List<CertificateResponse> GetListCertificadosCC(int idNumber)
        {
            try
            {
                //Verifica se existem leitores
                PTEID_ReaderSet readerSet = PTEID_ReaderSet.instance();
                if (readerSet.readerCount(true) <= 0)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.CardReader_NotFound ,"No Card Reader Detected! ", HttpStatusCode.InternalServerError));
                    return null;
                }

                var readerList = readerSet.readerList(true);
                List<PTEID_EIDCard>cardList = new List<PTEID_EIDCard>();
                foreach ( string _readerName in readerList)
                {
                    PTEID_ReaderContext _readerContext = readerSet.getReaderByName(_readerName);
                    if (_readerContext.isCardPresent()
                        && _readerContext.getCardType() != PTEID_CardType.PTEID_CARDTYPE_UNKNOWN)
                    {
                        cardList.Add(_readerContext.getEIDCard());                        
                    }
                       
                }
                if (cardList.Count() <= 0)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.CC_NotFound, "No Card (CC) Detected! ", HttpStatusCode.InternalServerError));
                    return null;
                }

                PTEID_EIDCard _cc = null;
                foreach (PTEID_EIDCard _card in cardList)
                {
                    if (Int64.Parse(_card.getID().getCivilianIdNumber()) == idNumber)
                    {
                        _cc = _card;
                        break;
                    }                       
                }
                if (_cc == null)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.InvalidIdentification, "Invalid CC, Verify Your Card/Identity!", HttpStatusCode.InternalServerError));
                    return null;
                }

                List<CertificateResponse> certificados = new List<CertificateResponse>();
                // Get the ca certificate from the card
                PTEID_Certificate ca = _cc.getCA();
                var cerData = ca.getCertData();
                var bytes = cerData.GetBytes();
                string certB64 = System.Convert.ToBase64String(bytes);
                certificados.Add(new CertificateResponse { Subject = ca.getLabel(), CertBase64 = certB64 });

                // Get the authentication certificate from the card
                PTEID_Certificate authentication = _cc.getAuthentication();
                var cerData_2 = authentication.getCertData();
                var bytes_2 = cerData_2.GetBytes();
                string certB64_2 = System.Convert.ToBase64String(bytes_2);
                certificados.Add(new CertificateResponse { Subject = authentication.getLabel(), CertBase64 = certB64_2 });

                if (certificados.Count() <= 0)
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.Certificates_NotFound, "Certificates Not Found!", HttpStatusCode.NotFound));

                return new List<CertificateResponse>(certificados);
            }
            catch (Exception Ex)
            {
                NotificationErrorContext.AddNotification(
                    new NotificationError(ErrorCode.ExceptionError, "Exception Occured in  SmartCardReaderService.GetListCertificadosCC() => " + Ex.Message,
                    HttpStatusCode.InternalServerError));
                return null;
            }            
        }

        public List<CertificateResponse> GetListCertificadosCOM(int nProfCell)
        {
            try
            {
                // Creates a Cryptoki object related to the specific PKCS#11 native library 
                Cryptoki cryptoki = new Cryptoki();
                cryptoki.SetLicenseInfo("f3m.pt", "GKQ7-BFC3-RNM7-6LHP-P7MZ-GZQM-4UUC");
                cryptoki = new Cryptoki("OcsCryptoki.dll");
                cryptoki.Initialize();

                // Reads the set of slots containing a token
                SlotList slots = cryptoki.ActiveSlots;
                if (slots.Count == 0)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.CardReader_NotFound, "No Card Reader Detected! ", HttpStatusCode.InternalServerError));
                    return null;
                }

                // Gets the first slot available
                Token token = null;
                foreach (Slot slot in slots)
                {
                    if (slot.IsTokenPresent)
                    {
                        try 
                        {
                            TokenInfo info = slot.Token.Info;
                            token = slot.Token;
                        } 
                        catch (Exception ex)
                        {
                            token = null;
                            continue;
                        }
                    }
                }
                if (token == null)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.COM_NotFound, "No Card (COM) Detected! ", HttpStatusCode.InternalServerError));
                    return null;
                }
                //teste
                //var t = token.Info.SerialNumber;

                // Opens a read/write serial session
                Session session =
                    token.OpenSession(Session.CKF_SERIAL_SESSION | Session.CKF_RW_SESSION, null, null);

                CryptokiCollection template = new CryptokiCollection();
                // Searchs for an certificate object
                template.Add(new ObjectAttribute(ObjectAttribute.CKA_CLASS, CryptokiObject.CKO_CERTIFICATE));

                // Launchs the search specifying the template just created
                var certificates = session.Objects.Find(template, 10);
                if (certificates.Count == 0)
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.Certificates_NotFound, "Certificates Not Found!", HttpStatusCode.NotFound));

                List<CertificateResponse> certificados = new List<CertificateResponse>();
                bool _validIndetification = false;
                foreach (var certificado in certificates)
                {
                    X509Certificate2 cert = Utils.ConvertCertificate((Cryptware.NCryptoki.X509Certificate)certificado);
                    string certB64 = Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks);
                    CertificateResponse certificate = new CertificateResponse() { Subject = cert.Subject, CertBase64 = certB64 };

                    if (certificate.Verify_COM_Professional_Cell(nProfCell))
                        _validIndetification = true;

                    certificados.Add(certificate);                    
                }

                //valida Identidade Medico/cartao
                if (!_validIndetification)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.InvalidIdentification, "Invalid COM, Verify Your Card/Identity!", HttpStatusCode.InternalServerError));
                    return null;
                }

                return new List<CertificateResponse>(certificados);
            }
            catch (Exception Ex)
            {
                NotificationErrorContext.AddNotification(
                    new NotificationError(ErrorCode.ExceptionError, "Exception Occured in SmartCardReaderService.GetListCertificadosCOM() => " + Ex.Message,
                    HttpStatusCode.InternalServerError));
                return null;
            }           
        }

        public XmlDocument SignXML(string id, string tipoCartao, string body)
        {
            try
            {
                XmlDocument _xmlToSign = new XmlDocument();
                _xmlToSign.LoadXml(body);

                X509Certificate2 myCert = null;
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var certificates = store.Certificates;

                if (certificates.Count <= 0)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.Certificates_NotFound, "Certificates Not Found!", HttpStatusCode.NotFound));
                    return null;
                }

                foreach (var certif in certificates)
                {
                    CertificateResponse cert = new CertificateResponse() { Subject = certif.Subject };
                   
                    //COM
                    if (cert.Subject.ToLower().Contains("assinatura qualificada")
                        && tipoCartao == "COM"
                        && cert.Verify_COM_Professional_Cell(Int32.Parse(id)))
                    {
                        myCert = certif;
                        break;
                    }

                    //CC                       
                    else if (cert.Subject.ToLower().Contains("assinatura qualificada do cidadão")
                        && tipoCartao == "CC"
                        && cert.Verify_CC_Number(Int32.Parse(id)))
                    {
                        myCert = certif;
                        break;
                    }
                }

                if (myCert == null)
                {
                    NotificationErrorContext.AddNotification(new NotificationError(ErrorCode.SigningCertificate_NotFound, "Valid Signing Certificate Not Found!", HttpStatusCode.NotFound));
                    return null;
                }

                // Create a SignedXml object.
                SignedXml signedXml = new SignedXml(_xmlToSign);
                // Add the key to the SignedXml document.
                signedXml.SigningKey = myCert.PrivateKey;
                // Create a reference to be signed.
                Reference reference = new Reference();
                reference.Uri = "";
                // Add an enveloped transformation to the reference.
                var env = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(env);
                // Include the public key of the certificate in the assertion.
                signedXml.KeyInfo = new KeyInfo();
                signedXml.KeyInfo.AddClause(new KeyInfoX509Data(myCert, X509IncludeOption.WholeChain));
                // Add the reference to the SignedXml object.
                signedXml.AddReference(reference);
                // Compute the signature.
                signedXml.ComputeSignature();
                // Get the XML representation of the signature and save
                // it to an XmlElement object.
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                // Append the element to the XML document.
                _xmlToSign.DocumentElement.AppendChild(_xmlToSign.ImportNode(xmlDigitalSignature, true));

                return _xmlToSign;
            }
            catch (Exception ex)
            {
                NotificationErrorContext.AddNotification(
                       new NotificationError(ErrorCode.ExceptionError, "Exception Occured in  SmartCardReaderService.SignXML() => " + ex.Message,
                       HttpStatusCode.InternalServerError));
                return null;
            }
        }
    }
}
