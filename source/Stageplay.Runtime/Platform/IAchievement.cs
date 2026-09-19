namespace Radish;

public interface IAchievement
{
    string Name { get; }
    bool Unlocked { get; }

    public void Unlock();
}