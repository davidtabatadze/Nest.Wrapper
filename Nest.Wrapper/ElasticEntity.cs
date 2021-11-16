

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents entity of the elastic index
    /// </summary>
    /// <typeparam name="T">Type of the id</typeparam>
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class ElasticEntity<T> : IElasticEntity<T>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public ElasticEntity()
        {
            // Generating entity key
            ElasticHelper.GenerateKey(this);
        }

        /// <summary>
        /// Entity unique id.
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        /// Either delete current entity or not.
        /// This property will not be stored in the index.
        /// </summary>
        [Ignore]
        public bool Delete { get; set; }

    }

}
