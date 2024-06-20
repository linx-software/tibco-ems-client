using System.Collections;
using TIBCO.EMS;

public static class SslHelpers
{
    public static void InitSSLParams(string serverUrl, string[] args)
    {
        if (serverUrl != null && serverUrl.IndexOf("ssl://") >= 0)
        {
            var ssl = new SSLParams(args);
            ssl.Init();
        }
    }

    public static void SslUsage()
    {
        System.Console.WriteLine("\nSSL options:");
        System.Console.WriteLine("");
        System.Console.WriteLine(" -ssl_trace                            - trace SSL initialization");
        System.Console.WriteLine(" -ssl_trusted[n]           <file-name> - file with trusted certificates,");
        System.Console.WriteLine("                                         this parameter may repeat if more");
        System.Console.WriteLine("                                         than one file required");
        System.Console.WriteLine(" -ssl_target_hostname    <string>    - expected name in the certificate");
        System.Console.WriteLine(" -ssl_custom                           - use custom verifier (it shows names");
        System.Console.WriteLine("                                         always approves them).");
        System.Console.WriteLine(" -ssl_identity             <file-name> - client identity file");
        System.Console.WriteLine(" -ssl_password             <string>    - password to decrypt client identity");
        System.Console.WriteLine("                                         or key file");
        System.Environment.Exit(0);
    }

    private class SSLParams
    {
        private bool ssl_trace;
        private string ssl_target_hostname;
        private ArrayList ssl_trusted;
        private string ssl_identity;
        private string ssl_password;
        private bool ssl_custom;

        public SSLParams(string[] args)
        {
            int trusted_pi = 0;
            string trusted_suffix = "";

            int i = 0;
            while (i < args.Length)
            {
                if (args[i].CompareTo("-ssl_trace") == 0)
                {
                    this.ssl_trace = true;
                    i += 1;
                }
                else
                if (args[i].CompareTo("-ssl_target_hostname") == 0)
                {
                    if ((i + 1) >= args.Length)
                        SslUsage();
                    this.ssl_target_hostname = args[i + 1];
                    i += 2;
                }
                else
                if (args[i].CompareTo("-ssl_custom") == 0)
                {
                    this.ssl_custom = true;
                    i += 1;
                }
                else
                if (args[i].CompareTo("-ssl_identity") == 0)
                {
                    if ((i + 1) >= args.Length)
                        SslUsage();
                    this.ssl_identity = args[i + 1];
                    i += 2;
                }
                else
                if (args[i].CompareTo("-ssl_password") == 0)
                {
                    if ((i + 1) >= args.Length)
                        SslUsage();
                    this.ssl_password = args[i + 1];
                    i += 2;
                }
                else
                if (args[i].CompareTo("-ssl_trusted" + trusted_suffix) == 0)
                {
                    if ((i + 1) >= args.Length)
                        SslUsage();
                    string cert = args[i + 1];
                    if (cert == null)
                        continue;
                    if (this.ssl_trusted == null)
                        this.ssl_trusted = new ArrayList();
                    this.ssl_trusted.Add(cert);
                    trusted_pi++;
                    trusted_suffix = System.Convert.ToString(trusted_pi);
                    i += 2;
                }
                else
                {
                    i++;
                }
            }
        }

        public void Init()
        {
            EMSSSLFileStoreInfo storeInfo = new EMSSSLFileStoreInfo();

            if (this.ssl_trace)
                EMSSSL.SetClientTracer(new System.IO.StreamWriter(System.Console.OpenStandardOutput()));

            if (this.ssl_target_hostname != null)
                EMSSSL.SetTargetHostName(this.ssl_target_hostname);

            if (this.ssl_custom)
            {
                HostVerifier v = new HostVerifier();
                EMSSSL.SetHostNameVerifier(new EMSSSLHostNameVerifier(v.VerifyHost));
            }

            if (this.ssl_trusted != null)
            {
                for (int i = 0; i < this.ssl_trusted.Count; i++)
                {
                    string certfile = (string)this.ssl_trusted[i];
                    storeInfo.SetSSLTrustedCertificate(certfile);
                }
            }

            if (this.ssl_identity != null)
            {
                storeInfo.SetSSLClientIdentity(this.ssl_identity);
                storeInfo.SetSSLPassword(this.ssl_password.ToCharArray());
            }

            EMSSSL.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE, storeInfo);
        }

        private class HostVerifier
        {
            public bool VerifyHost(object source, EMSSSLHostNameVerifierArgs args)
            {
                System.Console.WriteLine("-------------------------------------------");
                System.Console.WriteLine("HostNameVerifier: "
                                         + "certCN = [" + args.m_certificateCommonName + "]\n"
                                         + "connected Host = [" + args.m_connectedHostName + "]\n"
                                         + "expected Host = [" + args.m_targetHostName + "]");
                System.Console.WriteLine("-------------------------------------------");


                return true;
            }
        }
    }
}



