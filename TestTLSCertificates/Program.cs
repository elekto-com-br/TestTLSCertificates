using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Elekto
{
    class Program
    {
        /// <summary>
        /// Se deve usar a validação customizada de certificados
        /// </summary>
        private static bool _useCustomCertificateValidation;

        /// <summary>
        /// Comportamento padrão da validação customizada
        /// </summary>
        private static CustomCertificateValidationBehaviour _customCertificateValidationBehaviour = CustomCertificateValidationBehaviour.SystemDefault;

        /// <summary>
        /// Thumbprints de certificados na lista negra
        /// </summary>
        private static HashSet<string> _customCertificateValidationBlackListedThumbprints = new HashSet<string>();

        /// <summary>
        /// Thumbprints de certificados na lista branca
        /// </summary>
        private static HashSet<string> _customCertificateValidationWhiteListedThumbprints = new HashSet<string>();

        static int Main(string[] args)
        {
            try
            {
                var appSettings = new ConfigFileGeneralProperties();

                var url = args.Length >= 1 ? args[0] : appSettings.Get("testUrl", string.Empty);

                if (string.IsNullOrWhiteSpace(url))
                {
                    Console.WriteLine("URL não especificada. Passe no 1º parâmetro ou em /configuration/appSettings[@key='testUrl']/@value do arquivo .config desse programa");
                    return 1;
                }

                Console.WriteLine($"A URL usada será: {url}");

                var configs = new ConfigFileGeneralProperties();
                _useCustomCertificateValidation = configs.Get("customCertificateValidation", false);
                if (_useCustomCertificateValidation)
                {
                    Console.WriteLine("Usando validação SSL/TLS customizada...");

                    _customCertificateValidationBehaviour = (CustomCertificateValidationBehaviour)configs.Get("customCertificateValidationBehaviour", 0);

                    Console.WriteLine($"Modo de validação SSL/TLS customizada: {_customCertificateValidationBehaviour}");

                    if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.BlackListByThumbprint))
                    {
                        _customCertificateValidationBlackListedThumbprints = configs.Get("customCertificateValidationBlackListThumbprints", string.Empty)
                            .Split(new[] { ';', ' ', '\r', '\n', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).ToHashSet();
                    }

                    if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.WhiteListByThumbprint))
                    {
                        _customCertificateValidationWhiteListedThumbprints = configs.Get("customCertificateValidationWhiteListThumbprints", string.Empty)
                            .Split(new[] { ';', ' ', '\r', '\n', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).ToHashSet();
                    }

                    System.Net.ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                }

                var proxy = GetWebProxy();

                var tempFileName = Path.GetTempFileName() + ".txt";
                using (var request = new WebClient())
                {
                    if (proxy != null)
                    {
                        request.Proxy = proxy;
                        Console.WriteLine("Proxy configurado.");
                    }

                    request.Headers.Add(HttpRequestHeader.UserAgent, "TestTLSCertificate-by-Elekto");

                    Console.WriteLine($"Baixando '{url}' em '{tempFileName}'...");
                    request.DownloadFile(url, tempFileName);
                    Console.WriteLine("Baixado!");
                    var fi = new FileInfo(tempFileName);
                    Console.WriteLine($"Arquivo baixado tem {fi.Length:N0} bytes.");

                    if ((args.Length >= 2) && (args[1].Equals("keep", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Console.WriteLine($"Arquivo baixado foi mantido em {tempFileName}");
                    }
                    else
                    {
                        fi.Delete();
                    }
                    
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 2;
            }
        }

        /// <summary>
        /// Rotina de log e autorização, opcional dos SSLs
        /// </summary>
        /// <remarks>
        /// Super perigoso e inseguro mexer com isso!
        /// </remarks>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate originalCert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            try
            {

                if (_customCertificateValidationBehaviour == CustomCertificateValidationBehaviour.SystemDefault)
                {
                    return policyErrors == SslPolicyErrors.None;
                }


                if (originalCert == null)
                {
                    // Nunca deveria acontecer, não?
                    Console.WriteLine("O certificado é nulo!");
                    return policyErrors == SslPolicyErrors.None;
                }

                var cert = new X509Certificate2(originalCert);

                if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.Log))
                {
                    Console.WriteLine($"Custom Certificate Validation Behaviour: {_customCertificateValidationBehaviour}");
                    Console.WriteLine($"system policyErrors: {policyErrors}");
                    Console.WriteLine($"Subject: {cert.Subject}");
                    Console.WriteLine($"Issuer:  {cert.Issuer}");
                    Console.WriteLine($"Effective Date:  {cert.NotBefore:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"Expiration Date:  {cert.NotAfter:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"Thumbprint:  {cert.Thumbprint}");

                    if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.DumpChain))
                    {
                        if (chain == null)
                        {
                            Console.WriteLine("A cadeia é nula!");
                        }
                        else
                        {
                            Console.WriteLine("Iniciando Dump da Cadeia...");
                            foreach (var chainElement in chain.ChainElements)
                            {
                                Console.WriteLine("----->");

                                Console.WriteLine($"Subject: {chainElement.Certificate.Subject}");
                                Console.WriteLine($"Issuer:  {chainElement.Certificate.Issuer}");
                                Console.WriteLine($"Effective Date:  {chainElement.Certificate.NotBefore:yyyy-MM-dd HH:mm:ss}");
                                Console.WriteLine($"Expiration Date:  {chainElement.Certificate.NotAfter:yyyy-MM-dd HH:mm:ss}");
                                Console.WriteLine($"Thumbprint:  {chainElement.Certificate.Thumbprint}");

                                foreach (var status in chainElement.ChainElementStatus)
                                {
                                    Console.WriteLine($"  Status: {status.Status}, {status.StatusInformation}");
                                }

                                Console.WriteLine("<-----");
                            }
                            Console.WriteLine("Findo Dump da Cadeia.");
                        }
                    }
                }

                if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.BlackListByThumbprint))
                {
                    if (cert.Thumbprint != null && _customCertificateValidationBlackListedThumbprints.Contains(cert.Thumbprint.ToLowerInvariant()))
                    {
                        Console.WriteLine($"Rejeitado por ter o thumbprint {cert.Thumbprint} na black list.");
                        return false;
                    }
                }

                if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.WhiteListByThumbprint))
                {
                    if (cert.Thumbprint != null && _customCertificateValidationWhiteListedThumbprints.Contains(cert.Thumbprint.ToLowerInvariant()))
                    {
                        Console.WriteLine($"Aceito por ter o thumbprint {cert.Thumbprint} na white list.");
                        return true;
                    }
                }

                if (_customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.AcceptAll))
                {
                    Console.WriteLine($"Aceitando sempre o certificado {cert.Subject}.{cert.Thumbprint}.");
                    return true;
                }

                if (policyErrors != SslPolicyErrors.None && _customCertificateValidationBehaviour.HasFlag(CustomCertificateValidationBehaviour.IgnoreExpired) &&
                    cert.NotAfter < DateTime.Now)
                {
                    Console.WriteLine($"Aceitando o certificado {cert.Subject}.{cert.Thumbprint} expirado em {cert.NotAfter:yyyy-MM-dd HH:mm:ss}.");
                    return true;
                }

                var res = policyErrors == SslPolicyErrors.None;
                Console.WriteLine($"Certificado será aceito? {res}");

                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private static IWebProxy GetWebProxy()
        {
            var appSettings = new ConfigFileGeneralProperties();

            //obtendo a string de conexão com o proxy. Se nao houver, nao usarei proxy;
            var proxyUrl = appSettings.Get("proxyLocation", string.Empty);

            //se nao estiver setado para usar proxy, retorno nulo;
            if (string.IsNullOrEmpty(proxyUrl))
            {
                Console.WriteLine("Nenhum proxy configurado.");
                return null;
            }
            Console.WriteLine($"Endereço do Proxy: {proxyUrl}");

            var webProxy = new WebProxy(proxyUrl)
            {
                BypassProxyOnLocal = appSettings.Get("proxyBypassProxyOnLocal", true)
            };

            Console.WriteLine($"BypassProxyOnLocal: {webProxy.BypassProxyOnLocal}");

            // Tipo de credencial no proxy
            var proxyCredentialMode = appSettings.Get("proxyCredentialMode", string.Empty);
            if (string.IsNullOrEmpty(proxyCredentialMode))
            {
                Console.WriteLine("Erro! Foi setada a URL do proxy mas nao a credencial. Não será utilizado proxy.");
                return null;
            }

            switch (proxyCredentialMode)
            {
                case "1":
                    webProxy.UseDefaultCredentials = true;
                    Console.WriteLine("Usando credencias padrão.");
                    break;
                case "2":
                    {
                        var proxyCredentialLogin = appSettings.Get("proxyCredentialLogin", string.Empty);
                        var proxyCredentialPassword = appSettings.Get("proxyCredentialPassword", string.Empty);
                        var proxyCredentialDomain = appSettings.Get("proxyCredentialDomain", string.Empty);
                        if (!string.IsNullOrEmpty(proxyCredentialLogin))
                        {
                            Console.WriteLine($"Proxy Login: {proxyCredentialLogin}");

                            var proxyCredential = new NetworkCredential { UserName = proxyCredentialLogin };
                            if (!string.IsNullOrEmpty(proxyCredentialPassword))
                            {
                                proxyCredential.Password = proxyCredentialPassword;
                                Console.WriteLine($"Proxy Password Length: {proxyCredentialPassword.Length}");
                            }

                            if (!string.IsNullOrEmpty(proxyCredentialDomain))
                            {
                                proxyCredential.Domain = proxyCredentialDomain;
                                Console.WriteLine($"Proxy Domain: {proxyCredentialDomain}");
                            }

                            webProxy.Credentials = proxyCredential;
                        }
                    }
                    break;
            }

            return webProxy;
        }
    }
}
