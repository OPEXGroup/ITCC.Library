using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;
using ITCC.HTTP.Server;

namespace ITCC.HTTP.Common
{
    /// <summary>
    ///     Static class that contains all library delegates
    /// </summary>
    public static class Delegates
    {
        /// <summary>
        ///     Operation to be performed during authentification
        /// </summary>
        /// <param name="request">Outcoming request</param>
        /// <returns>Operation status</returns>
        public delegate bool AuthentificationDataAdder(HttpRequestMessage request);

        /// <summary>
        ///     Server authentification methods
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Authentification task</returns>
        public delegate Task<AuthentificationResult> Authentificator(HttpRequest request);

        /// <summary>
        ///     Kind of authorization procedure
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="requestProcessor">Way to process request</param>
        /// <returns>Authentification status</returns>
        public delegate Task<AuthorizationResult<TAccount>> Authorizer<TAccount>(
            HttpRequest request,
            RequestProcessor<TAccount> requestProcessor)
            where TAccount : class;

        /// <summary>
        ///     Kind of authorization procedure
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="filename">Requested filename</param>
        /// <returns>Authentification status</returns>
        public delegate Task<AuthorizationResult<TAccount>> FilesAuthorizer<TAccount>(
            HttpRequest request,
            FileSection section,
            string filename)
            where TAccount : class;

        /// <summary>
        ///     string -> TResult converter used to process request/response bodies.
        /// </summary>
        /// <typeparam name="TResult">Deserialization result type</typeparam>
        /// <param name="data">Raw body</param>
        /// <returns>Deserialized object</returns>
        public delegate TResult BodyDeserializer<out TResult>(string data);

        /// <summary>
        ///     TArg -> string converter used to create request/response bodies
        /// </summary>
        /// <param name="data">Object to put in body</param>
        /// <returns>Serialized data</returns>
        public delegate string BodySerializer(object data);

        /// <summary>
        ///     Method type used to load server TLS certificate
        /// </summary>
        /// <param name="subjectName">Subject name (server address of fqdn)</param>
        /// <param name="allowSelfSignedCertificates">If true, certificate is allowed to be generated in place</param>
        /// <returns>Certificate instance</returns>
        public delegate X509Certificate2 CertificateProvider(string subjectName, bool allowSelfSignedCertificates);

        /// <summary>
        ///     Method type user to handle user requests on server
        /// </summary>
        /// <param name="account">Server account</param>
        /// <param name="request">Received HTTP(S) request</param>
        /// <returns></returns>
        public delegate Task<HandlerResult> RequestHandler<in TAccount>(TAccount account, HttpRequest request);

        /// <summary>
        ///     Used to find service requests, independent from user-registered handlers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal delegate bool ServiceRequestChecker(HttpRequest request);

        /// <summary>
        ///     Used to serve service requests, independent from user-registered handlers
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="request"></param>
        internal delegate void ServiceRequestHandler(ITcpChannel channel, HttpRequest request);
    }
}