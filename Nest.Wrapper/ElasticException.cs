using System;

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents an exception of Nest.Wrapper
    /// </summary>
    public sealed class ElasticException : Exception
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public ElasticException(string message) : base(message) { }

    }

}