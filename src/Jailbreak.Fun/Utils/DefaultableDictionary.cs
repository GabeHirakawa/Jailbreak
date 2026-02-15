using System.Collections;

namespace Jailbreak.Fun.Utils;

public class DefaultableDictionary<TKey, TValue>(
  IDictionary<TKey, TValue> dictionary, Func<TValue> defaultValueFactory)
  : IDictionary<TKey, TValue> {
  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
    return dictionary.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

  public void Add(KeyValuePair<TKey, TValue> item) { dictionary.Add(item); }

  public void Clear() { dictionary.Clear(); }

  public bool Contains(KeyValuePair<TKey, TValue> item) {
    return dictionary.Contains(item);
  }

  public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
    dictionary.CopyTo(array, arrayIndex);
  }

  public bool Remove(KeyValuePair<TKey, TValue> item) {
    return dictionary.Remove(item);
  }

  public int Count => dictionary.Count;

  public bool IsReadOnly => dictionary.IsReadOnly;

  public bool ContainsKey(TKey key) { return dictionary.ContainsKey(key); }

  public void Add(TKey key, TValue value) { dictionary.Add(key, value); }

  public bool Remove(TKey key) { return dictionary.Remove(key); }

  public bool TryGetValue(TKey key, out TValue value) {
    value = dictionary.ContainsKey(key) ? dictionary[key] : defaultValueFactory();
    return true;
  }

  public TValue this[TKey key] {
    get {
      if (!TryGetValue(key, out var value)) value = defaultValueFactory();
      return value;
    }

    set => dictionary[key] = value;
  }

  public ICollection<TKey> Keys => dictionary.Keys;

  public ICollection<TValue> Values {
    get {
      var values = new List<TValue>(dictionary.Values) { defaultValueFactory() };
      return values;
    }
  }
}
