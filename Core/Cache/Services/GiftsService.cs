using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase;
using Document;
using Couchbase.Query;
using App.Metrics;

namespace Cache.Services
{
    public interface IGiftsService
    {
        Task<IEnumerable<WishlistItem>> LoadWishlistItemAsync(CancellationToken token = default);
        Task<WishlistItem> GetWishlistByIdAsync(Guid id, CancellationToken token = default);
        Task CreateOrEditAsync(WishlistItemCreateOrUpdate documentToCreateOrUpdate, CancellationToken token = default);
        Task DeleteAsync(Guid id, bool isSoftDelete = false, CancellationToken token = default);
    }

    public sealed class GiftsService : IGiftsService
    {
        private readonly IBucketProvider _bucketProvider;
        private const string _bucketName = "Demo";
        private const string _collectionName = "wishlist";

        public GiftsService(IBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider;
        }

        public async Task<IEnumerable<WishlistItem>> LoadWishlistItemAsync(CancellationToken token = default)
        {
            string? executionTime = string.Empty;
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var cluster = bucket.Cluster;
                var result = await cluster.QueryAsync<WishlistItem>(
@$"
SELECT 
META(w).id
,w.name
FROM Demo._default.wishlist w
WHERE w.deleted IS MISSING;", options =>
                {
                    options.Timeout(TimeSpan.FromSeconds(5));
                    options.Metrics(true);
                    options.Readonly(true);
                    options.CancellationToken(token);
                })
                .ConfigureAwait(false);

                if (result.MetaData?.Status is not QueryStatus.Success)
                {
                    throw new CouchbaseException("Query execution error");
                }

                var metrics = result.MetaData.Metrics;
                executionTime = metrics?.ExecutionTime;

                return await result.ToListAsync(cancellationToken: token).ConfigureAwait(false);
            }
            catch (DocumentNotFoundException)
            {
                // cache miss - get value from permanent storage

                // repopulate cache so subsequent calls get cache hit
                throw;
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (CouchbaseException)
            {
                // error performing insert
                throw;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                // Log information
                Console.WriteLine($"Execution time: {executionTime}");
            }
        }

        public async Task<WishlistItem> GetWishlistByIdAsync(Guid id, CancellationToken token = default)
        {
            try
            {
                if (id == default)
                {
                    throw new DocumentNotFoundException("Not a valid Id");
                }
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var collection = await bucket.CollectionAsync(_collectionName).ConfigureAwait(false);

                var item = await collection.GetAsync(id.ToString(), options =>
                {
                    options.Timeout(TimeSpan.FromSeconds(5));
                    options.AsReadOnly();
                    options.CancellationToken(token);
                }).ConfigureAwait(false);

                var document = item.ContentAs<WishlistItem>();

                Validation(document.Deleted);

                return document with { Id = id };
            }
            catch (DocumentNotFoundException)
            {
                // cache miss - get value from permanent storage

                // repopulate cache so subsequent calls get cache hit
                throw;
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (CouchbaseException)
            {
                // error performing insert
                throw;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateOrEditAsync(WishlistItemCreateOrUpdate documentToCreateOrUpdate, CancellationToken token = default)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var collection = await bucket.CollectionAsync(_collectionName).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(documentToCreateOrUpdate.Id) is false)
                {
                    var item = await collection.GetAsync(documentToCreateOrUpdate.Id, options =>
                    {
                        options.Timeout(TimeSpan.FromSeconds(5));
                        options.AsReadOnly();
                        options.CancellationToken(token);
                    }).ConfigureAwait(false);

                    var document = item.ContentAs<WishlistItemCreateOrUpdate>();

                    Validation(document.Deleted);
                    
                    if (document with { Id = documentToCreateOrUpdate.Id } == documentToCreateOrUpdate)
                    {
                        throw new InvalidOperationException("No change detected");
                    };
                }
                else
                {
                    documentToCreateOrUpdate = documentToCreateOrUpdate with { Id = Guid.NewGuid().ToString() };
                }

                await collection.UpsertAsync(documentToCreateOrUpdate.Id, new { documentToCreateOrUpdate.Name }, options =>
                {
                    //options.Expiry(TimeSpan.FromMinutes(1));
                    //options.Durability(PersistTo.One, ReplicateTo.One); /* or */ // options.Durability(DurabilityLevel.Majority);
                    options.Timeout(TimeSpan.FromSeconds(5));
                    options.CancellationToken(token);
                }).ConfigureAwait(false);
            }
            catch (DocumentNotFoundException)
            {
                // cache miss - get value from permanent storage

                // repopulate cache so subsequent calls get cache hit
                throw;
            }
            catch (DocumentExistsException)
            {
                throw;
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (InvalidOperationException) 
            {
                throw;
            }
            catch (CouchbaseException)
            {
                // error performing insert
                throw;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteAsync(Guid id, bool isSoftDelete = false, CancellationToken token = default)
        {
            try
            {
                if (id == default)
                {
                    throw new DocumentNotFoundException("Not a valid Id");
                }

                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var collection = await bucket.CollectionAsync(_collectionName).ConfigureAwait(false);

                var previousResult = await collection.GetAsync(id.ToString(), options =>
                {
                    options.Timeout(TimeSpan.FromSeconds(5));
                    options.AsReadOnly();
                    options.CancellationToken(token);
                }).ConfigureAwait(false);

                if (isSoftDelete is false)
                {
                    await collection.RemoveAsync(id.ToString(), options =>
                    {
                        options.Cas(previousResult.Cas);
                        options.Timeout(TimeSpan.FromSeconds(5));
                        options.CancellationToken(token);

                    }).ConfigureAwait(false);
                }
                else
                {
                    var document = previousResult.ContentAs<WishlistItem>();

                    Validation(document.Deleted);

                    await collection.MutateInAsync(id.ToString(), specs =>
                    {
                        specs.Upsert("deleted", DateTime.Now);
                    }, options =>
                    {
                        options.Timeout(TimeSpan.FromSeconds(5));
                        options.CancellationToken(token);
                    }
                    ).ConfigureAwait(false);
                }
            }
            catch (DocumentNotFoundException)
            {
                // cache key doesn't exist
                throw;
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (CouchbaseException)
            {
                // error performing insert
                throw;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void Validation(DateTime? deletedDate)
        {
            if (deletedDate is not null)
            {
                throw new DocumentNotFoundException($"Document has been soft deleted on {deletedDate}");
            }
        }
    }
}
