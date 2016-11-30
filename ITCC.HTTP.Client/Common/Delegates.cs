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
    }
}