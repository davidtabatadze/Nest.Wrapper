

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents basic description of the elastic entity.
    /// </summary>
    public interface IElasticEntityKeyable
    {
    }

    /// <summary>
    /// Represents basic description of the elastic index keyable entity.
    /// </summary>
    /// <typeparam name="T">Type of the key</typeparam>
    public interface IElasticEntityKeyable<T> : IElasticEntityKeyable
    {

        /// <summary>
        /// Entity unique id.
        /// </summary>
        T Id { get; set; }

    }

}