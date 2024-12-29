namespace SabaBot.Utils;

public static class CollectionExtensions {
    public static T? RandomOrDefault<T>(this IList<T> source, bool remove = true) {
        if (source.Count == 0) {
            return default;
        }
        var random = new Random().Next(0, source.Count);
        var item = source[random];
        if (remove) source.RemoveAt(random);
        return item;
    }

    public static void Shift<T>(this IList<T> source, T value, int limit) {
        var diff = source.Count - limit;
        if (diff >= 0) {
            for (var i = 0; i <= diff; i++) {
                source.RemoveAt(0);
            }
        }
        source.Add(value);
    }

    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        var index = 0;
        foreach (var item in source) {
            if (predicate(item)) return index;
            index++;
        }
        return -1;
    }
}