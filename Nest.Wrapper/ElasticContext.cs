using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Elasticsearch.Net;

namespace Nest.Wrapper
{

    /// <summary>
    /// Represents wrapper of ElasticClient.
    /// </summary>
    public class ElasticContext : IDisposable
    {

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">We are disposing or not</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }
            // If disposing
            if (disposing)
            {
                Mappings = null;
                Configuration = null;
                ConnectionObject = null;
            }
            Disposed = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Configuration <see cref="ElasticConfiguration"/></param>
        /// <param name="mappings">Entity mappings: class type to elastic index connection</param>
        public ElasticContext(ElasticConfiguration configuration, Dictionary<string, Type> mappings)
        {
            // ...
            Configuration = configuration;
            // Defining mappings
            foreach (var mapping in mappings)
            {
                // Validation
                ElasticHelper.ValidateElasticEntity(mapping.Value);
                // ... If mapping is not present yet
                if (!Mappings.ContainsKey(mapping.Key))
                {
                    Mappings.Add(mapping.Key, mapping.Value);
                }
            }
        }

        /// <summary>
        /// Either disposed or not
        /// </summary>
        private bool Disposed = false;

        /// <summary>
        /// Class type to elastic index connection dictionary
        /// </summary>
        private Dictionary<string, Type> Mappings = new Dictionary<string, Type> { };

        /// <summary>
        /// Configuration object
        /// </summary>
        private ElasticConfiguration Configuration { get; set; }

        /// <summary>
        /// Connection object
        /// </summary>
        private ElasticClient ConnectionObject { get; set; }

        /// <summary>
        /// Configured conection
        /// </summary>
        public ElasticClient Connection
        {
            get
            {
                if (ConnectionObject == null)
                {
                    var settings = new ConnectionSettings(new Uri(Configuration.Server))
                                   .RequestTimeout(new TimeSpan(0, 10, 0))
                                   .DisableDirectStreaming(true);
                    foreach (var mapping in Mappings)
                    {
                        settings.DefaultMappingFor(mapping.Value, map => map.IndexName(mapping.Key));
                    }
                    ConnectionObject = new ElasticClient(settings);
                }
                // Returning as singleton
                return ConnectionObject;
            }
        }

        /// <summary>
        /// Extracts keys from IElasticEntityKeyables
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entities">Entities</param>
        /// <returns>Keys</returns>
        private IEnumerable<object> ExtractKeys<E>(IEnumerable<E> entities) where E : class, IElasticEntityKeyable
        {
            var keys = new List<object> { };
            var typeLong = typeof(IElasticEntityKeyable<long>).IsAssignableFrom(typeof(E));
            var typeString = typeof(IElasticEntityKeyable<string>).IsAssignableFrom(typeof(E));
            foreach (var entity in entities)
            {
                keys.Add(
                    typeLong ? (entity as IElasticEntityKeyable<long>).Id :
                    typeString ? (entity as IElasticEntityKeyable<string>).Id :
                    default(object)
                );
            }
            return keys;
        }

        #region Nest

        //public async Task<IResponse> Execute<E>(Task<ISearchResponse<E>> query) where E : class, IElasticEntityKeyable
        //{
        //    var result = await query;
        //    if (!result.IsValid)
        //    {
        //        throw new Exception(result.DebugInformation);
        //    }
        //    return result;
        //}
        //public async Task<IResponse> Execute<E>(Task<BulkResponse> query) where E : class, IElasticEntityKeyable
        //{
        //    var result = await query;
        //    if (!result.IsValid)
        //    {
        //        throw new Exception(result.DebugInformation);
        //    }
        //    return result;
        //}
        //public async Task<R> Execute<R, E>(Task<R> query) where E : IElasticEntityKeyable where R : IResponse
        //{
        //    var result = await query;
        //    if (!result.IsValid)
        //    {
        //        throw new Exception(result.DebugInformation);
        //    }
        //    return result;
        //}

        /// <summary>
        /// Execute nest query
        /// </summary>
        /// <typeparam name="R">Response type</typeparam>
        /// <param name="query">Nest query</param>
        /// <returns>Execution result</returns>
        public async Task<R> Execute<R>(Task<R> query) where R : IResponse
        {
            try
            {
                var result = await query;
                if (!result.IsValid)
                {
                    throw new Exception(result.DebugInformation);
                }
                return result;
            }
            catch (ArgumentException exception)
            {
                throw new Exception(
                    string.Format("Wrapper: {0}.\nRaw: {1}", ElasticAnnotation.EntityNotMapped, exception.Message)
                );
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region C Create

        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Empty</returns>
        public async Task Insert<E>(E entity) where E : class, IElasticEntityKeyable
        {
            await Insert(new List<E> { entity });
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entities">Entities</param>
        /// <returns>Empty</returns>
        public async Task Insert<E>(IEnumerable<E> entities) where E : class, IElasticEntityKeyable
        {
            var existings = await Load<E>(ExtractKeys(entities), 1, fields => fields.Includes(includes => includes.Field("id")));
            if (existings.Count > 0)
            {
                throw new Exception(ElasticAnnotation.EntityExists);
            }
            await Save(entities, true);
        }

        #endregion

        #region R Read

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="id">Entity id</param>
        /// <param name="fields">Entity fields</param>
        /// <returns>Entity</returns>
        public async Task<E> Get<E>(long id, Func<SourceFilterDescriptor<E>, ISourceFilter> fields = null) where E : class, IElasticEntityKeyable<long>
        {
            var result = await Load(new List<long> { id }, fields);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="id">Entity id</param>
        /// <param name="fields">Entity fields</param>
        /// <returns>Entity</returns>
        public async Task<E> Get<E>(string id, Func<SourceFilterDescriptor<E>, ISourceFilter> fields = null) where E : class, IElasticEntityKeyable<string>
        {
            var result = await Load(new List<string> { id }, fields);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="selector">Qeury selector</param>
        /// <returns>Entity</returns>
        /// <remarks>Using null parameter will try get the first available record</remarks>
        public async Task<E> Get<E>(Func<SearchDescriptor<E>, ISearchRequest> selector = null) where E : class, IElasticEntityKeyable
        {
            Func<SearchDescriptor<E>, ISearchRequest> sizer = descriptor => descriptor.Size(1);
            var result = await Load(selector + sizer);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="ids">Entity ids</param>
        /// <param name="fields">Entity fields</param>
        /// <returns>Entities</returns>
        public async Task<List<E>> Load<E>(IEnumerable<long> ids, Func<SourceFilterDescriptor<E>, ISourceFilter> fields = null) where E : class, IElasticEntityKeyable<long>
        {
            return await Load(ids.Cast<object>(), 0, fields);
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="ids">Entity ids</param>
        /// <param name="fields">Entity fields</param>
        /// <returns>Entities</returns>
        public async Task<List<E>> Load<E>(IEnumerable<string> ids, Func<SourceFilterDescriptor<E>, ISourceFilter> fields = null) where E : class, IElasticEntityKeyable<string>
        {
            return await Load(ids.Cast<object>(), 0, fields);
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="ids">Entity ids</param>
        /// <param name="count">Load size</param>
        /// <param name="fields">Entity fields</param>
        /// <returns>Entities</returns>
        private async Task<List<E>> Load<E>(IEnumerable<object> ids, int count = 0, Func<SourceFilterDescriptor<E>, ISourceFilter> fields = null) where E : class, IElasticEntityKeyable
        {
            //if (fields == null)
            //{
            //    fields = fields => fields.IncludeAll();
            //}
            return await Load<E>(
                load => load
                .Source(fields)
                .Size(count > 0 ? count : ids.Count())
                .Query(query => query
                    .Terms(terms => terms
                        .Field("id")
                        .Terms(ids)
                    )
                )
            );
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="selector">Query selector</param>
        /// <returns>Entities</returns>
        /// <remarks>Using null parameter will try to load all records</remarks>
        public async Task<List<E>> Load<E>(Func<SearchDescriptor<E>, ISearchRequest> selector = null) where E : class, IElasticEntityKeyable
        {
            //if (selector == null)
            //{
            //    selector = selector => selector.MatchAll();
            //}
            Func<SearchDescriptor<E>, ISearchRequest> hitter = descriptor => descriptor.TrackTotalHits(false);
            var result = await Execute(Connection.SearchAsync(selector + hitter));
            return result.Documents.ToList();
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="selector">Query selector</param>
        /// <returns>Count</returns>
        /// <remarks>Using null parameter will try to count all records</remarks>
        public async Task<long> Count<E>(Func<CountDescriptor<E>, ICountRequest> selector) where E : class, IElasticEntityKeyable
        {
            var result = await Execute(Connection.CountAsync(selector));
            return result.Count;
        }

        #endregion

        #region U Update

        /// <summary>
        /// Save
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Empty</returns>
        public async Task Save<E>(E entity) where E : class, IElasticEntityKeyable, IElasticEntityDeletable
        {
            await Save(new List<E> { entity });
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entities">Entities</param>
        /// <returns>Empty</returns>
        public async Task Save<E>(IEnumerable<E> entities) where E : class, IElasticEntityKeyable, IElasticEntityDeletable
        {
            await Delete(entities.Where(entity => entity.Delete));
            await Save(entities.Where(entity => !entity.Delete), true);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entities">Entities</param>
        /// <param name="refresh">Either refresh index or not</param>
        /// <returns>Empty</returns>
        private async Task Save<E>(IEnumerable<E> entities, bool refresh) where E : class, IElasticEntityKeyable
        {
            if (entities != null)
            {
                entities = entities.Where(entity => entity != null);
                if (entities.Count() > 0)
                {
                    await Execute(Connection.IndexManyAsync(entities));
                    if (refresh)
                    {
                        await Connection.Indices.RefreshAsync(Mappings.First(map => map.Value == typeof(E)).Key);
                    }
                }
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="id">Entity id</param>
        /// <param name="update">Entity update part</param>
        /// <param name="upsert">Either insert if new or not</param>
        /// <returns>Empty</returns>
        public async Task Save<E>(object id, object update, bool upsert = false) where E : class, IElasticEntityKeyable
        {
            await Save<E>(new Dictionary<object, object> { { id, update } }, upsert);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="updates">Update data: entity key - entity update part</param>
        /// <param name="upsert">Either insert if new or not</param>
        /// <returns>Empty</returns>
        public async Task Save<E>(Dictionary<object, object> updates, bool upsert = false) where E : class, IElasticEntityKeyable
        {
            var bulk = new BulkDescriptor();
            foreach (var entity in updates.Where(update => update.Key != null && update.Value != null))
            {
                bulk.Update<E, object>(update => update
                     .Id(new Id(new { id = entity.Key }))
                     .DocAsUpsert(upsert)
                     .Doc(entity.Value)
                ).Refresh(Refresh.WaitFor);
            }
            await Execute(Connection.BulkAsync(bulk));
        }

        #endregion

        #region D Delete

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="id">Entity id</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(long id) where E : class, IElasticEntityKeyable<long>
        {
            await Delete<E>(new List<long> { id });
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="id">Entity id</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(string id) where E : class, IElasticEntityKeyable<string>
        {
            await Delete<E>(new List<string> { id });
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="ids">Entity ids</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(IEnumerable<long> ids) where E : class, IElasticEntityKeyable<long>
        {
            await Delete<E>(ids.Cast<object>());
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="ids">Entity ids</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(IEnumerable<string> ids) where E : class, IElasticEntityKeyable<string>
        {
            await Delete<E>(ids.Cast<object>());
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(E entity) where E : class, IElasticEntityKeyable
        {
            await Delete(new List<E> { entity });
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="entities">Entities</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(IEnumerable<E> entities) where E : class, IElasticEntityKeyable
        {
            // Defining the actual keys
            var keys = ExtractKeys(entities);
            // Delete the data
            await Delete<E>(keys);
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="ids">Entity ids</param>
        /// <returns>Empty</returns>
        private async Task Delete<E>(IEnumerable<object> ids) where E : class, IElasticEntityKeyable
        {
            await Delete<E>(delete => delete
                .Query(query => query
                    .Terms(terms => terms
                        .Field("id")
                        .Terms(ids)
                    )
                )
            );
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="E">Entity type</typeparam>
        /// <param name="selector">Qeury selector</param>
        /// <returns>Empty</returns>
        public async Task Delete<E>(Func<DeleteByQueryDescriptor<E>, IDeleteByQueryRequest> selector = null) where E : class, IElasticEntityKeyable
        {
            if (selector == null)
            {
                selector = selector => selector.MatchAll();
            }
            Func<DeleteByQueryDescriptor<E>, IDeleteByQueryRequest> kit =
                descriptor => descriptor.Conflicts(Conflicts.Proceed).Refresh(true);
            await Execute(Connection.DeleteByQueryAsync(selector + kit));
        }

        #endregion

    }

}
