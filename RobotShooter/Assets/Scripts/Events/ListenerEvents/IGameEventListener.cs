namespace Shokai.Events.Listeners
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T item);
    }
}