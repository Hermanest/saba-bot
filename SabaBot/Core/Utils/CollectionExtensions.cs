namespace SabaBot.Utils;

public static class CollectionExtensions {
    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        var index = 0;
        foreach (var item in source) {
            if (predicate(item)) return index;
            index++;
        }
        return -1;
    }
}