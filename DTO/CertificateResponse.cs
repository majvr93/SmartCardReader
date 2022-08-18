namespace SmartCardReader.DTO
{
    public class CertificateResponse
    {
        public string Subject { get; set; }
        public string CertBase64 { get; set; }

        public bool Verify_COM_Professional_Cell(int nCell)
        {
            //"SERIALNUMBER=CP"
            int _startIndex = this.Subject.IndexOf("SERIALNUMBER=CP");
            if (_startIndex == -1)
                return false;

            string aux = this.Subject.Substring(_startIndex + 15);
            string[] strgs = aux.Split(',');
            string nr = strgs[0];

           if(nr.Contains(nCell.ToString()))
                return true;

            return false;
        }

        public bool Verify_CC_Number(int n)
        {
            //"SERIALNUMBER=CP"
            int _startIndex = this.Subject.IndexOf("SERIALNUMBER=BI");
            if (_startIndex == -1)
                return false;

            string aux = this.Subject.Substring(_startIndex + 15);
            string[] strgs = aux.Split(',');
            string nr = strgs[0];

            if (nr.Contains(n.ToString()))
                return true;

            return false;
        }
    }
}
