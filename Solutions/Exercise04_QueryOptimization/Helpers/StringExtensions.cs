namespace Exercise04_QueryOptimization.Helpers;

public static class StringExtensions
{
    public static string Repeat(this string s, int count) => new string('=', count);
}
