namespace Radish;

public interface IPlatformAchievements
{
    public IReadOnlyCollection<IAchievement> Achievements { get; }
}