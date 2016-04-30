namespace PoMo.Server
{
    public interface ISubscribable
    {
        string[] GetSubscribers();

        bool Subscribe(string subscriberId, out int subscriberCount);

        bool Unsubscribe(string subscriberId, out int subscriberCount);
    }
}