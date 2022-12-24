namespace SomeShop.Common.Domain;

public class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}