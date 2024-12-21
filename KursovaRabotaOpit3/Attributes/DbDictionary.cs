using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaRabotaOpit3.Attributes
{
    public class DbDictionary<TKey, TValue>
    {
        private TKey[] keys;
        private TValue[] values;
        public int size;

        public DbDictionary(int capacity)
        {
            keys = new TKey[capacity];
            values = new TValue[capacity];
            size = 0;
        }

        public void Add(TKey key, TValue value)
        {
            if (size < keys.Length)
            {
                keys[size] = key;
                values[size] = value;
                size++;
            }
            else
            {
                throw new InvalidOperationException("Dictionary is full.");
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (int i = 0; i < size; i++)
            {
                if (object.Equals(keys[i], key))
                {
                    value = values[i];
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }
        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                {
                    return value;
                }
                else
                {
                    throw new Exception();
                }
            }
            set
            {
                for (int i = 0; i < size; i++)
                {
                    if (object.Equals(keys[i], key))
                    {
                        values[i] = value;
                        return;
                    }
                }

                // Key not found, add it with the provided value
                if (size < keys.Length)
                {
                    keys[size] = key;
                    values[size] = value;
                    size++;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }

        public void Remove(TKey key)
        {
            for (int i = 0; i < size; i++)
            {
                if (object.Equals(keys[i], key))
                {
                    for (int j = i; j < size - 1; j++)
                    {
                        keys[j] = keys[j + 1];
                        values[j] = values[j + 1];
                    }
                    size--;
                    return;
                }
            }
        }
        public TKey[] GetKeys()
        {
            return keys;
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            foreach(TKey key in keys)
            {
                if (!this[key].Equals(((DbDictionary<TKey,TValue>)obj)[key])) return false;
            }
            return true;
        }
        public TValue[] GetValues() { return values; }
    }
}
