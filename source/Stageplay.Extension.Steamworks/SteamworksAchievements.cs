using Steamworks;
using Steamworks.Data;

namespace Radish.Steamworks;

internal sealed class SteamworksAchievements : IPlatformAchievements
{
    private sealed class AchievementImpl(Achievement data) : IAchievement
    {
        public string Name => _data.Name;
        public bool Unlocked => _data.State;
        
        private Achievement _data = data;
        
        public void Unlock()
        {
            _data.Trigger();
        }
    }

    public IReadOnlyCollection<IAchievement> Achievements => _achievements.Value;

    private readonly Lazy<List<AchievementImpl>> _achievements =
        new(() => [.. SteamUserStats.Achievements.Select(a => new AchievementImpl(a))]);
}