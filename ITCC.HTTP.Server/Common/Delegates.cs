using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ITCC.HTTP.Server.Auth;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Files;

namespace ITCC.HTTP.Server.Common
{
    /// <summary>
    ///     Static class that contains all library delegates
    /// </summary>
    public static class Delegates
    {
        /// <summary>
        ///     Server authentification methods
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Authentification task</returns>
        public delegate Task<AuthentificationResult> Authentificator(HttpListenerRequest request);

        /// <summary>
        ///     Kind of authorization procedure
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="requestProcessor">Way to process request</param>
        /// <returns>Authentification status</returns>
        public delegate Task<AuthorizationResult<TAccount>> Authorizer<TAccount>(
            HttpListenerRequest request,
            RequestProcessor<TAccount> requestProcessor)
            where TAccount : class;

        /// <summary>
        ///     Check if account is allowed to see server statistics
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public delegate Task<bool> StatisticsAuthorizer(HttpListenerRequest request);

        /// <summary>
        ///     Kind of authorization procedure
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="filename">Requested filename</param>
        /// <returns>Authentification status</returns>
        public delegate Task<AuthorizationResult<TAccount>> FilesAuthorizer<TAccount>(
            HttpListenerRequest request,
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
        ///     Method type user to handle user requests on server
        /// </summary>
        /// <param name="account">Server account</param>
        /// <param name="request">Received HTTP(S) request</param>
        /// <returns></returns>
        public delegate Task<HandlerResult> RequestHandler<in TAccount>(TAccount account, HttpListenerRequest request);

        /// <summary>
        ///     Used to find service requests, independent from user-registered handlers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal delegate bool ServiceRequestChecker(HttpListenerRequest request);

        /// <summary>
        ///     Used to serve service requests, independent from user-registered handlers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestStopWatch">For performance measuring</param>
        internal delegate Task ServiceRequestHandler(HttpListenerContext context, Stopwatch requestStopWatch);
    }
}