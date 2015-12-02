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
        // dynamically, and of course equivalent to Maybe. So see
        // MStack below for the obvious (at least to me) way to
        // do that. But meanwhile, here's an overloading-based
        // version.
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

    // What I really want is to be able to use Nothing as a default
    // parameter value. That is, apparently, not possible. The only
    // things that can be default parameter values are constants (as
    // in aliases to literals of built-in types), new instances of value
    // types, or default constructions of value types. Still, I guess
    // `defval=default(Maybe<int>)` isn't much worse than the
    // `deval=Maybe<int>.Nothing` that I wanted. The bigger problem is
    // that this means Maybe has to be a value type. Which means it's
    // not covariant (so Maybe<U> is not a subclass of Maybe<T> even if
    // U is a subclass of T), it can't use default member values or a
    // nullary constructor and therefore needs a more-complicated and 
    // less-readable construction mess, etc. On the plus side, being a
    // value type means no chance of confusion between null and Nothing
    // (in particular, `default(Maybe<int>)` isn't null), so... thanks?
    public struct Maybe<T>
    {
        readonly T _value;
        readonly bool _something;
        private Maybe(T value, bool something) { _value = value; _something = something; }
        public Maybe(T value) : this(value, true) { }

        // This allows us to do `s.pop(2) instead of `s.pop(new Maybe<int>(2)`
        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public bool Empty() { return !_something; }
        // Maybe this is being too clever? But it seems like the best way
        // to do the C# equivalent of an exhaustive pattern match on maybe...
        public U Match<U>(Func<T, U> just, Func<U> nothing)
        {
            return _something ? just(_value) : nothing();
        }
        // And here's the dynamic equivalent, for when you're feeling more
        // Python than Haskell
        public static explicit operator T(Maybe<T> m)
        {
            if (!m._something) throw new InvalidCastException("Nothing");
            return m._value;
        }

        public override string ToString()
        {
            return _something ? String.Format("Maybe({0})", _value) : "Maybe.Nothing";
        }

        // Not very useful given the problems described above, but...
        public static readonly Maybe<T> Nothing = new Maybe<T>(default(T), false);
    }

    public class MStack<T>
    {
        Cons<T> lst = null;
        public MStack() { }
        public void push(T value)
        {
            lst = new Cons<T>(value, lst);
        }
        // Much simpler than the overload-based version, once you have Maybe
        public T pop(Maybe<T> defval = default(Maybe<T>))
        {
            if (lst == null)
                return defval.Match(t => t, () => { throw new InvalidOperationException(); });
            T value = lst.head;
            lst = lst.tail;
            return value;
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

            var ms = new MStack<int>();
            ms.push(1);
            ms.push(2);
            Console.WriteLine(ms.pop());
            Console.WriteLine(ms.pop());
            Console.WriteLine(ms.pop(0));
            try
            {
                Console.WriteLine(ms.pop());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
