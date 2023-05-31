namespace Document
{
    public readonly record struct WishlistItem(Guid Id, string Name, DateTime? Deleted = null);
    public readonly record struct WishlistItemCreateOrUpdate(string Id, string Name, DateTime? Deleted = null);
    public readonly record struct WishlistItemCreateOrUpdateWithCustomer(string Id, string Name, string CustomerName, DateTime? Deleted = null);
}
