

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents elastic configuration.
    /// </summary>
    public class ElasticConfiguration
    {

        /// <summary>
        /// Server address
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Either save null values or not
        /// </summary>
        public bool IncludeNullValues { get; set; }

        /// <summary>
        /// Timeout per request, in case of 0 default is 10
        /// </summary>
        public ushort RequestTimeOutMinutes { get; set; }

    }

}