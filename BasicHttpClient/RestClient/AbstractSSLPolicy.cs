using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace BasicRestClient.RestClient {
    /// <summary>
    /// This class helps implement an SSL Certifcate Policy
    /// </summary>
    public abstract class AbstractSSLPolicy : ICertificatePolicy {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="certificateFileLocation"></param>
        protected AbstractSSLPolicy(string certificateFileLocation) {
            CertificateFileLocation = certificateFileLocation;
            X509Certificate = LoadCertificate();
        }

        private X509Certificate LoadCertificate() {
            if (!string.IsNullOrEmpty(CertificateFileLocation)) {
                X509Certificate Cert =
                    X509Certificate.CreateFromCertFile(CertificateFileLocation);
                return Cert;
            }
            return null;
        }

        /// <summary>
        /// Validate SSL certificate
        /// </summary>
        /// <param name="srvPoint"></param>
        /// <param name="certificate"></param>
        /// <param name="request"></param>
        /// <param name="certificateProblem"></param>
        /// <returns></returns>
        public bool CheckValidationResult(ServicePoint srvPoint,
            X509Certificate certificate,
            WebRequest request,
            int certificateProblem) {
            return CheckCertificate(srvPoint, certificate, request, certificateProblem);
        }

        /// <summary>
        /// Path of the certificate file
        /// </summary>
        public string CertificateFileLocation { get; private set; }

        /// <summary>
        /// X509 SSL Certificate
        /// </summary>
        public X509Certificate X509Certificate { get; private set; }

        /// <summary>
        /// This method helps check SSL certificate.
        /// </summary>
        /// <param name="servicePoint"></param>
        /// <param name="certificate"></param>
        /// <param name="request"></param>
        /// <param name="certificateProblem"></param>
        /// <returns></returns>
        protected abstract bool CheckCertificate(ServicePoint servicePoint,
            X509Certificate certificate,
            WebRequest request,
            int certificateProblem);
    }
}