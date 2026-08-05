namespace Boxhead.Systems
{
    /// <summary>
    /// Static flag checked by enemy projectile scripts (BossProjectile, etc.) on collision
    /// with the Player tag. When IsActive is true, the projectile reverses its velocity
    /// instead of dealing damage and clears the flag. Set by SendItBackBehaviour.
    /// </summary>
    public static class ProjectileDeflector
    {
        /// <summary>True when the Lightsaber Legendary "Send It Back" deflect is ready.</summary>
        public static bool IsActive { get; set; }
    }
}
