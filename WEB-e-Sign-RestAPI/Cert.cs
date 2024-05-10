using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.IO;
using System.Collections;

namespace WEB_e_Sign_RestAPI
{
    class Cert
    {
        #region Attributes

        private string path = "";
        private string password = "";
        private AsymmetricKeyParameter akp;
        private Org.BouncyCastle.X509.X509Certificate[] chain;

        #endregion

        #region Accessors
        public Org.BouncyCastle.X509.X509Certificate[] Chain
        {
            get { return chain; }
            set { chain = value; }
        }
        public AsymmetricKeyParameter Akp
        {
            get { return akp; }
            set { akp = value; }
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        #endregion

        #region Helpers

        private void processCert()
        {
            string alias = null;
            Org.BouncyCastle.Pkcs.Pkcs12Store pk12;

            //First we'll read the certificate file
            pk12 = new Pkcs12Store(new FileStream(this.Path, FileMode.Open, FileAccess.Read), this.password.ToCharArray());

            //then Iterate throught certificate entries to find the private key entry
            IEnumerator i = pk12.Aliases.GetEnumerator();
            while (i.MoveNext())
            {
                alias = ((string)i.Current);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            this.akp = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            this.chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
                chain[k] = ce[k].Certificate;


        }
        #endregion

        #region Constructors
        public Cert()
        { }
        public Cert(string cpath)
        {
            this.path = cpath;
            this.processCert();
        }
        public Cert(string cpath, string cpassword)
        {
            this.path = cpath;
            this.Password = cpassword;
            this.processCert();
        }
        #endregion
    }
}