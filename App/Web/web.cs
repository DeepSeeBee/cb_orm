using CbOrm.Attributes;
using CbOrm.Converters;
using CbOrm.Crypt;
using CbOrm.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbOrm.App.Web
{

    [CSaveAs(typeof(string))]
    [CSaveConverter(typeof(CStringWrapperConverter<CEmailAdress>))]
    public class CEmailAdress
    {
        public CEmailAdress(string aText)
        {
            this.Text = aText.AvoidNullString().Trim();
        }
        public CEmailAdress()
        {
            this.Text = string.Empty;
        }
        private readonly string Text;

        public override int GetHashCode()=> this.Text.ToLower().GetHashCode();
        public static bool operator ==(CEmailAdress lhs, CEmailAdress rhs) => lhs.Text.ToLower() == rhs.Text.ToLower();
        public static bool operator !=(CEmailAdress lhs, CEmailAdress rhs) => ! (lhs == rhs);
        public override bool Equals(object obj) => obj is CEmailAdress ? (this == (CEmailAdress)obj) : false;

        public bool Ok { get=>true; } // TODO

        public override string ToString()
        {
            return this.Text;
        }
    }

    public sealed class CPasswordEncryptConverter : CConverter
    {
        public CPasswordEncryptConverter()
        {
        }
        private readonly string EncryptPassword = "1a0ed9b7-4ef2-4f72-8b1a-3742c53f0edb";

        public override object Convert(object aValue, Type aTargetType)
        {
            var aPassword = (CPassword)aValue;
            var aPasswordText = aPassword.ToString();
            var aEncryption = new CEncryption();
            var aEncryptedString = aEncryption.EncryptToString(aPasswordText, EncryptPassword);
            return aEncryptedString;
        }

        public override object ConvertBack(object aValue, Type aTargetType)
        {
            var aEncryptedString = (string)aValue;
            var aEncryption = new CEncryption();
            var aDecryptedString = aEncryption.DecryptToString(aEncryptedString, EncryptPassword, 128);
            var aPassword = new CPassword(aDecryptedString);
            return aPassword;
        }
    }

    [CSaveAs(typeof(string))]
    [CSaveConverter(typeof(CPasswordEncryptConverter))]
    public class CPassword
    {
        public CPassword(string aText)
        {
            this.Text = aText.Trim();
        }
        public CPassword()
        {
            this.Text = string.Empty;
        }
        private readonly string Text;
        public override int GetHashCode() => this.Text.ToLower().GetHashCode();
        public static bool operator ==(CPassword lhs, CPassword rhs) => lhs.Text.ToLower() == rhs.Text.ToLower();
        public static bool operator !=(CPassword lhs, CPassword rhs) => !(lhs == rhs);
        public override bool Equals(object obj) => obj is CPassword ? (this == (CPassword)obj) : false;

        public override string ToString()
        {
            return this.Text;
        }
        public bool Ok { get => this.Text.Length > 0; } // TODO
    }

}
