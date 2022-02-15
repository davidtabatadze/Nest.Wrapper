

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

    }

}
