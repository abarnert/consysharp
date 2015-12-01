class Cons:
    def __init__(self, head=None, tail=None):
        self.head, self.tail = head, tail
    @classmethod
    def fromiter(cls, it):
        it = iter(it)
        try:
            return cls(next(it), cls.fromiter(it))
        except StopIteration:
            return None
    def __iter__(self):
        yield self.head
        if self.tail:
            yield from self.tail

class LazyCons:
    def __init__(self, head=None, tailfunc=None):
        self.head, self._delaytail = head, tailfunc
    @classmethod
    def strict(cls, head=None, tail=None):
        o = cls(head)
        o._tail = tail
    @property
    def tail(self):
        if self._delaytail:
            self._tail = self._delaytail()
            self._delaytail = None
            print('Evaluated {}'.format(self._tail))
        return self._tail
    @tail.setter
    def tail(self, value):
        self._tail, self._delaytail = value, None
    @classmethod
    def fromiter(cls, it):
        it = iter(it)
        try:
            return cls(next(it), lambda: cls.fromiter(it))
        except StopIteration:
            return None
    def __iter__(self):
        yield self.head
        if self.tail:
            yield from self.tail

class Stack:
    _sentinel = object()
    def __init__(self):
        self.lst = None
    def push(self, value):
        self.lst = Cons(value, self.lst)
    def pop(self, default=_sentinel):
        try:
            value, self.lst = self.lst.head, self.lst.tail
            return value
        except AttributeError:
            if default is self._sentinel:
                raise ValueError('pop from an empty {}'.format(
                    type(self).__name__))
            return default

if __name__ == '__main__':
    xs = Cons.fromiter([1, 2, 3])
    for x in xs: print(x)
    ys = xs.tail
    xs.tail.head *= -1
    for y in ys: print(y)

    zs = LazyCons.fromiter(xs)
    for z in zs: print(z)
    for z in zs: print(z)

    ss = Stack()
    ss.push(1)
    ss.push(2)
    print(ss.pop())
    print(ss.pop())
    print(ss.pop(None))
    try:
        print(ss.pop())
    except Exception as e:
        print(e)

