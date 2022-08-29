using System;

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents elastic index configuration.
    /// </summary>
    public sealed class ElasticIndexConfiguration
    {

        /// <summary>
        /// Name of the index
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the linked model
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Index configuration
        /// </summary>
        public Func<CreateIndexDescriptor, ICreateIndexRequest> Configuration { get; set; }

    }

}