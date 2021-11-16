using System;

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents additinal functionality for the wrapper
    /// </summary>
    public class ElasticHelper
    {

        /// <summary>
        /// Validates elastic entity for acceptable id type
        /// </summary>
        /// <param name="type">Type of the validation object</param>
        internal static void ValidateElasticEntity(Type type)
        {
            // Only long and string are acceptable
            if (
                type != typeof(long) &&
                type != typeof(string) &&
                (!typeof(IElasticEntityKeyable<long>).IsAssignableFrom(type)) &&
                (!typeof(IElasticEntityKeyable<string>).IsAssignableFrom(type))
            )
            {
                // Otherwise throwing exception
                throw new NotImplementedException(string.Format(ElasticAnnotation.InvalidIdType, type.Name));
            }
        }

        /// <summary>
        /// Validates elastic entity for acceptable id type
        /// </summary>
        /// <typeparam name="T">Type of the validation object</typeparam>
        /// <param name="entity">Validation object</param>
        internal static void ValidateElasticEntity<T>()
        {
            // Do validation
            ValidateElasticEntity(typeof(T));
        }

        /// <summary>
        /// Generates fresh id for keyable entity
        /// </summary>
        /// <typeparam name="T">Type of the id</typeparam>
        /// <param name="entity">keyable entity</param>
        public static void GenerateKey<T>(IElasticEntityKeyable<T> entity)
        {
            // Validating id type
            ValidateElasticEntity<T>();
            // In case of long, when value is not requested by user ...
            if (typeof(T) == typeof(long) && Convert.ToInt64(entity.Id) <= 0)
            {
                // Generating long id from current date
                entity.Id = (T)(object)Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmssfffffff"));
            }
            // In case of string, when value is not requested by user ...
            if (typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(Convert.ToString(entity.Id)))
            {
                // Generating string id from current date
                entity.Id = (T)(object)DateTime.Now.ToString("yyMMddHHmmssfffffff");
            }
        }

    }

}
