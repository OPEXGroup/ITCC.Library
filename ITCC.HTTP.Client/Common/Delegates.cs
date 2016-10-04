using System.Net.Http;

namespace ITCC.HTTP.Client.Common
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
    }
}