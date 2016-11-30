namespace ITCC.HTTP.Client.Interfaces
{
    public interface IBodySerializer
    {
        string ContentType { get; }

        string Serialize(object data);
    }
}
