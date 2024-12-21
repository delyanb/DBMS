using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KursovaRabotaOpit3.Attributes
{
    public class DbList<T> : IEnumerable<T>
    {
        private T[] items;
        private int count;
        private const int DefaultCapacity = 0;

        public DbList()
        {
            items = new T[DefaultCapacity];
            count = 0;
        }
        public DbList(params T[] initialItems)
        {
            items = new T[DefaultCapacity];
            count = 0;

            foreach (var item in initialItems)
            {
                Add(item);
            }
        }

        public int Count => count;

        public void Add(T item)
        {
            EnsureCapacity();
            items[count] = item;
            count++;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException();
                return items[index];
            }
            set
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException();
                items[index] = value;
            }
        }

        private void EnsureCapacity()
        {
            if (count == items.Length)
            {
                if(items.Length == DefaultCapacity)
                {
                    T[] newItems = new T[1];
                    items = newItems;

                }
                else
                {

                int newCapacity = items.Length + 1;
                T[] newItems = new T[newCapacity];
                for (int i = 0; i < count; i++)
                {
                    newItems[i] = items[i];
                }
                items = newItems;
                }
            }
        }
        public bool Exists(Predicate<T> match)
        {
            for (int i = 0; i < count; i++)
            {
                if (match(items[i]))
                {
                    return true;
                }
            }
            return false;
        }
        public DbList<T> Where(Func<T, bool> predicate)
        {
            DbList<T> result = new DbList<T>();
            for (int i = 0; i < count; i++)
            {
                if (predicate(items[i]))
                {
                    result.Add(items[i]);
                }
            }
            return result;
        }
        public DbList<T> OrderBy<TKey>(Func<T, TKey> keySelector)
        {
            T[] orderedItems = items.Take(count).ToArray();
            Array.Sort(orderedItems, (x, y) => Comparer<TKey>.Default.Compare(keySelector(x), keySelector(y)));
            return new DbList<T>(orderedItems);
        }
        public T[] ToArray => items;
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();

            T[] newitems = new T[count -1];

            for (int i = index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
            }
            for(int i = 0; i < count -1; i++)
            {
                newitems[i] = items[i];
            }
            items = newitems;
            count--;
            
        }
        public DbList<T> Distinct()
        {
            DbList<T> distinctList = new DbList<T>();
            for(int i = 0;i< items.Length;i++)
            {
                if(distinctList.Count == 0)
                {
                    distinctList.Add(items[i]);
                }
                else
                {
                    bool found = false;
                    for(int  j = 0;j< distinctList.Count; j++)
                    {
                        if (distinctList[j].Equals(items[i]))
                            {
                            found = true;break;
                        }
                    }
                    if(!found)
                    {
                        distinctList.Add(items[i]);
                    }
                }
            }
            return distinctList;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return items[i];
            }
        }
        public DbList<T> WithoutLast()
        {
            DbList<T> newlist = new DbList<T> (items);
            
            if(count!=0)
            {
                newlist.RemoveAt(count - 1);
                return newlist;
            }
            return newlist;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
