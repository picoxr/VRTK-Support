using System.Collections;
using System.Collections.Generic;

namespace Pvr_UnitySDKAPI.Achievement
{
    public class Pvr_DeserializableList<T> : IList<T>
    {
        public int Count { get { return data.Count; } }
        bool ICollection<T>.IsReadOnly { get { return ((IList<T>)data).IsReadOnly; } }
        public int IndexOf(T obj) { return data.IndexOf(obj); }
        public T this[int index] { get { return data[index]; } set { data[index] = value; } }

        public void Add(T item) { data.Add(item); }
        public void Clear() { data.Clear(); }
        public bool Contains(T item) { return data.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { data.CopyTo(array, arrayIndex); }
        public IEnumerator<T> GetEnumerator() { return data.GetEnumerator(); }
        public void Insert(int index, T item) { data.Insert(index, item); }
        public bool Remove(T item) { return data.Remove(item); }
        public void RemoveAt(int index) { data.RemoveAt(index); }

        private IEnumerator GetEnumerator1()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        protected List<T> data;
        protected string nextUrl;
        protected string previousUrl;

        public bool HasNextPage { get { return !string.IsNullOrEmpty(NextUrl); } }
        public bool HasPreviousPage { get { return !string.IsNullOrEmpty(PreviousUrl); } }
        public string NextUrl { get { return nextUrl; } }
        public string PreviousUrl { get { return previousUrl; } }
    }
}
