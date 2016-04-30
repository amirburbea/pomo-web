using System;

namespace PoMo.Server
{
    internal static class RandomMethods
    {
        public static void PopulateRandomOrdinals(this Random random, int[] array, int maxValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                while (true)
                {
                    int number = random.Next(0, maxValue);
                    if (Array.IndexOf(array, number, 0, i) == -1)
                    {
                        array[i] = number;
                        break;
                    }
                }
            }
        }
    }
}