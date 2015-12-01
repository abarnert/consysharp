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

    public class Stack<T>
    {
        Cons<T> lst = null;
        public Stack() {}
        public void push(T value)
        {
            lst = new Cons<T>(value, lst);
        }
        public T pop()
        {
            if (lst == null)
                throw new InvalidOperationException();
            T value = lst.head;
            lst = lst.tail;
            return value;
        }
        // Annoyingly, there doesn't seem to be a good way to
        // do this with an optional parameter. You wouldn't really
        // want null as a default value even for reference types
        // (because then you can't safely put null on a stack), but
        // for value types you can't use it even if you wanted to
        // (and there's no way to restrict T to be a reference type
        // or V? for some value type V). Of course the right way to
        // do it is to use a union type of T | PrivateSentinel with
        // default(PrivateSentinel), equivalent to what Python does
        // dynamically, and of course equivalent to Maybe. But .NET
        // doesn't come with any way to do that built-in, and it
        // seemed a bit silly to implement Maybe just for this one
        // use, so I went with overloading instead. Sigh...
        public T pop(T defval)
        {
            try
            {
                return pop();
            }
            catch (InvalidOperationException)
            {
                return defval;
            }
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

            var ss = new Stack<int>();
            ss.push(1);
            ss.push(2);
            Console.WriteLine(ss.pop());
            Console.WriteLine(ss.pop());
            Console.WriteLine(ss.pop(0));
            try
            {
                Console.WriteLine(ss.pop());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
