using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace BasicRestClient.RestClient {
    /// <summary>
    /// Default Certificate Policy
    /// </summary>
    public class DefaultSslPolicy : AbstractSSLPolicy{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificateFileLocation"></param>
        public DefaultSslPolicy(string certificateFileLocation) : base(certificateFileLocation) {}

        /// <summary>
        /// No Certifcate file supplied
        /// </summary>
        public DefaultSslPolicy() : base(string.Empty){
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePoint"></param>
        /// <param name="certificate"></param>
        /// <param name="request"></param>
        /// <param name="certificateProblem"></param>
        /// <returns></returns>
        protected override bool CheckCertificate(ServicePoint servicePoint,
            X509Certificate certificate,
            WebRequest request,
            int certificateProblem) {
            return true;
        }
    }
}