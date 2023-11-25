namespace SteganographyApp
{
    /// <summary>
    /// Static reference class containing the number of available storage bits
    /// provided by images in common resolutions.
    /// </summary>
    public readonly struct CommonResolutionStorageSpace
    {
        public static readonly int P360 = 518_400;
        public static readonly int P480 = 921_600;
        public static readonly int P720 = 2_764_800;
        public static readonly int P1080 = 6_220_800;
        public static readonly int P1440 = 11_059_200;
        public static readonly int P2160 = 25_012_800;
    }
}