namespace SteganographyApp
{
    using System;

    internal static class Suppressed
    {
        public static void TryRun(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
            }
        }

        public static T TryRunForResult<T>(Func<T> func, T defaultValue)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}