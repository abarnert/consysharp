using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consy
{
    public class Cons<T> : IEnumerable<T>
    {
        public T head;
        public Cons<T> tail;
        public Cons(T head = default(T), Cons<T> tail=null)
        {
            this.head = head;
            this.tail = tail;
        }
        static public Cons<T> FromEnumerable(IEnumerable<T> e)
        {
            return FromEnumerable(e.GetEnumerator());
        }
        static public Cons<T> FromEnumerable(IEnumerator<T> e)
        {
            if (!e.MoveNext()) return null;
            return new Cons<T>(e.Current, Cons<T>.FromEnumerable(e));
        }
        public IEnumerator<T> GetEnumerator()
        {
            yield return head;
            if (tail != null) foreach (T val in tail) yield return val;
        }

        // Because IEnumerable<T> inherits IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();    
        }
    }

    public class LazyCons<T> : IEnumerable<T>
    {
        public T head;
        Func<LazyCons<T>> _delaytail = null;
        LazyCons<T> _tail = null;
        public LazyCons<T> tail
        {
            get
            {
                if (_delaytail != null)
                {
                    _tail = _delaytail();
                    _delaytail = null;
                    Console.WriteLine("Evaluated {0}", _tail);
                }
                return _tail;
            }
            set
            {
                _tail = value;
                _delaytail = null;
            }
        }
        public LazyCons(T head = default(T), LazyCons<T> tail = null)
        {
            this.head = head;
            this._tail = tail;
        }
        public LazyCons(T head = default(T), Func<LazyCons<T>> tail = null)
        {
            this.head = head;
            this._delaytail = tail;
        }
        static public LazyCons<T> FromEnumerable(IEnumerable<T> e)
        {
            return FromEnumerable(e.GetEnumerator());
        }
        static public LazyCons<T> FromEnumerable(IEnumerator<T> e)
        {
            if (!e.MoveNext()) return null;
            return new LazyCons<T>(e.Current, () => LazyCons<T>.FromEnumerable(e));
        }
        public IEnumerator<T> GetEnumerator()
        {
            yield return head;
            if (tail != null) foreach (T val in tail) yield return val;
        }

        // Because IEnumerable<T> inherits IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var xs = Cons<int>.FromEnumerable(new int[]{ 1, 2, 3});
            foreach (var x in xs) Console.WriteLine(x);
            var ys = xs.tail;
            xs.tail.head *= -1;
            foreach (var y in ys) Console.WriteLine(y);

            var zs = LazyCons<int>.FromEnumerable(xs);
            foreach (var z in zs) Console.WriteLine(z);
            foreach (var z in zs) Console.WriteLine(z);
        }
    }
}
