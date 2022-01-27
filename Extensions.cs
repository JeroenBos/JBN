public static class Extensions
{
    public static HashSet<T> Unique<T>(this T[,] array)
    {
        var result = new HashSet<T>();
        for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); i++)
        {
            for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); j++)
            {
                result.Add(array[i, j]);
            }
        }
        return result;
    }
    public static bool All<T>(this T[,] array, Func<T, bool> predicate)
    {
        for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); i++)
        {
            for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); j++)
            {
                if (!predicate(array[i, j]))
                    return false;
            }
        }
        return true;
    }
}
