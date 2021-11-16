

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents basic description of the elastic index deletable entity.
    /// </summary>
    public interface IElasticEntityDeletable
    {

        /// <summary>
        /// Either delete current entity or not.
        /// </summary>
        bool Delete { get; set; }

    }

}
