namespace Document
{
    public readonly record struct Customer(Guid Id, string Name, string Address);
    public readonly record struct Order(Guid Id, Guid CustomerId, string Items, double Price);
    public readonly record struct OrderWithCustomer(Guid Id, string Items, double Price, string CustomerName, string CustomerAddress);
}
