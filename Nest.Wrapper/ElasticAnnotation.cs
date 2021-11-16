

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents annotations for the wrapper
    /// </summary>
    internal class ElasticAnnotation
    {

        /// <summary>
        /// Invalid id type
        /// </summary>
        public const string InvalidIdType = "Presented id type is not implemented for the entity ({0}). Only 'long' and 'string' are acceptable.";

        /// <summary>
        /// Entity not mapped
        /// </summary>
        public const string EntityNotMapped = "Mapping is not defined for the entity. Corresponding elastic index was not found.";

        /// <summary>
        /// Entity exists
        /// </summary>
        public const string EntityExists = "Unable to insert record(s). Corresponding id(s) already exists.";

    }

}
