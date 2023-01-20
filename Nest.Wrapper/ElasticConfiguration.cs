using System.Collections.Generic;

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents elastic configuration.
    /// </summary>
    public class ElasticConfiguration
    {

        /// <summary>
        /// Server addresses - nodes
        /// </summary>
        public List<string> Nodes { get; set; }

        /// <summary>
        /// Number of shards
        /// </summary>
        public uint NumberOfShards { get; set; }

        /// <summary>
        /// Number of replicas
        /// </summary>
        public uint NumberOfReplicas { get; set; }

        /// <summary>
        /// Max result window
        /// </summary>
        public uint MaxResultWindow { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

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