using Couchbase.Core.Exceptions.KeyValue;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using Document;

namespace Cache.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Customer>> LoadCustomerAsync(CancellationToken token = default);
        Task<IEnumerable<Order>> LoadOrderAsync(CancellationToken token = default);
        Task<IEnumerable<OrderWithCustomer>> LoadOrderWithCustomerAsync(CancellationToken token = default);
        Task SeedDataAsync(int start, int totalRecord, CancellationToken token = default);
    }

    public sealed class OrderService : IOrderService
    {
        private readonly IBucketProvider _bucketProvider;
        private const string _bucketName = "Demo";
        private const string _collectionCustomer = "customer";
        private const string _collectionOrder = "order";

        public OrderService(IBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider;
        }

        public async Task<IEnumerable<Customer>> LoadCustomerAsync(CancellationToken token = default)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var cluster = bucket.Cluster;
                var result = await cluster.QueryAsync<Customer>(
@$"
SELECT
META(c).id
,c.name
,c.address
FROM Demo._default.customer c
WHERE c.deleted IS MISSING;", options =>
{
    //options.Timeout(TimeSpan.FromSeconds(15));
    options.Readonly(true);
    options.CancellationToken(token);
})
                .ConfigureAwait(false);

                if (result.MetaData?.Status is not QueryStatus.Success)
                {
                    throw new CouchbaseException("Query execution error");
                }

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
        }

        public async Task<IEnumerable<Order>> LoadOrderAsync(CancellationToken token = default)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var cluster = bucket.Cluster;
                var result = await cluster.QueryAsync<Order>(
@$"
SELECT
META(o).id
,o.customerId
,o.items
,o.price
FROM `Demo`.`_default`.`order` o
WHERE o.deleted IS MISSING;", options =>
{
    //options.Timeout(TimeSpan.FromSeconds(15));
    options.Readonly(true);
    options.CancellationToken(token);
})
                .ConfigureAwait(false);

                if (result.MetaData?.Status is not QueryStatus.Success)
                {
                    throw new CouchbaseException("Query execution error");
                }

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
        }

        public async Task<IEnumerable<OrderWithCustomer>> LoadOrderWithCustomerAsync(CancellationToken token = default)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
                var cluster = bucket.Cluster;
                var result = await cluster.QueryAsync<OrderWithCustomer>(
@$"
SELECT
META(o).id
,o.items
,o.price
,c.name AS customerName
,c.address AS customerAddress
FROM `Demo`.`_default`.`order` o
INNER JOIN Demo._default.customer c
ON o.customerId = Meta(c).id
WHERE o.deleted IS MISSING;", options =>
{
    //options.Timeout(TimeSpan.FromSeconds(15));
    options.Readonly(true);
    options.CancellationToken(token);
})
                .ConfigureAwait(false);

                if (result.MetaData?.Status is not QueryStatus.Success)
                {
                    throw new CouchbaseException("Query execution error");
                }

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
        }

        public async Task SeedDataAsync(int start, int totalRecord, CancellationToken token = default)
        {
            var bucket = await _bucketProvider.GetBucketAsync(_bucketName).ConfigureAwait(false);
            var collectionCustomer = await bucket.CollectionAsync(_collectionCustomer).ConfigureAwait(false);
            var collectionOrder = await bucket.CollectionAsync(_collectionOrder).ConfigureAwait(false);

            for (var i = start; i < totalRecord; i++)
            {
                var newCustomer = new Customer { Id = Guid.NewGuid(), Name = $"Customer_{i + 1}", Address = $"Address_{i + 1}" };
                await collectionCustomer.InsertAsync(newCustomer.Id.ToString(), new { newCustomer.Name, newCustomer.Address }).ConfigureAwait(false);
                var newOrder = new Order { Id = Guid.NewGuid(), CustomerId = newCustomer.Id, Items = $"Items_{i + 1}", Price = i + 1 };
                await collectionOrder.InsertAsync(newOrder.Id.ToString(), new { newOrder.CustomerId, newOrder.Items, newOrder.Price }).ConfigureAwait(false);
            }
        }
    }
}
