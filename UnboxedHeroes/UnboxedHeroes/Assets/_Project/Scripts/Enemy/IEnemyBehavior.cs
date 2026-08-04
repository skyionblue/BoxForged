namespace Boxhead.Enemy
{
    public interface IEnemyBehavior
    {
        void SetRooted(bool rooted);
        void ApplyHitStagger(float durationMultiplier = 1f);
    }
}
