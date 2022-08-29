

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents basic description of the elastic index entity.
    /// </summary>
    /// <typeparam name="T">Type of the id</typeparam>
    public interface IElasticEntity<T> : IElasticEntityKeyable<T>, IElasticEntityDeletable
    {
    }

}