namespace Boxhead.Enemy
{
    public interface IEnemyBehavior
    {
        void SetRooted(bool rooted);
        void ApplyHitStagger(float durationMultiplier = 1f);

        /// <summary>
        /// Multiplies the enemy's movement speed by <paramref name="multiplier"/> for
        /// <paramref name="duration"/> seconds, then restores original speed.
        /// Calling again while active restarts the timer with the new values.
        /// </summary>
        void SetSpeedMultiplier(float multiplier, float duration);
    }
}
