using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase;
using Document;

namespace Cache.Services
{
    public interface IGiftsService
    {
        Task<IEnumerable<WishlistItem>> LoadWishlistItemAsync(CancellationToken token = default);
        Task<WishlistItem> GetWishlistByIdAsync(Guid id);
        Task CreateOrEditAsync(WishlistItemCreateOrUpdate documentToCreateOrUpdate);
        Task DeleteAsync(Guid id, bool isSoftDelete = false);
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
WHERE w.deleted IS MISSING;")
                    .ConfigureAwait(false);
                return await result.ToListAsync(cancellationToken: token).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (DocumentNotFoundException)
            {
                // cache miss - get value from permanent storage

                // repopulate cache so subsequent calls get cache hit
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

        public async Task<WishlistItem> GetWishlistByIdAsync(Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var collection = await bucket.CollectionAsync(_collectionName).ConfigureAwait(false);

                var item = await collection.GetAsync(id.ToString()).ConfigureAwait(false);

                var document = item.ContentAs<WishlistItem>();

                Validation(document.Deleted);

                return document;
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (DocumentNotFoundException)
            {
                // cache miss - get value from permanent storage

                // repopulate cache so subsequent calls get cache hit
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

        public async Task CreateOrEditAsync(WishlistItemCreateOrUpdate documentToCreateOrUpdate)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var collection = await bucket.CollectionAsync(_collectionName).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(documentToCreateOrUpdate.Id) is false)
                {
                    var item = await collection.GetAsync(documentToCreateOrUpdate.Id).ConfigureAwait(false);

                    var document = item.ContentAs<WishlistItemCreateOrUpdate>();

                    Validation(document.Deleted);

                    documentToCreateOrUpdate = document;
                }
                else
                {
                    documentToCreateOrUpdate = documentToCreateOrUpdate with { Id = Guid.NewGuid().ToString() };
                }

                await collection.UpsertAsync(documentToCreateOrUpdate.Id, new { documentToCreateOrUpdate.Name }).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (DocumentNotFoundException)
            {
                // cache miss - get value from permanent storage

                // repopulate cache so subsequent calls get cache hit
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

        public async Task DeleteAsync(Guid id, bool isSoftDelete = false)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var collection = await bucket.CollectionAsync(_collectionName).ConfigureAwait(false);

                if (isSoftDelete == false)
                {
                    await collection.RemoveAsync(id.ToString()).ConfigureAwait(false);
                }
                else
                {
                    var item = await collection.GetAsync(id.ToString()).ConfigureAwait(false);
                    var document = item.ContentAs<WishlistItem>();

                    Validation(document.Deleted);

                    await collection.MutateInAsync(id.ToString(), specs =>
                        specs.Upsert("deleted", DateTime.Now)
                    ).ConfigureAwait(false);
                }
            }
            catch (TimeoutException)
            {
                // propagate, since time budget's up
                throw;
            }
            catch (DocumentNotFoundException)
            {
                // cache key doesn't exist
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
