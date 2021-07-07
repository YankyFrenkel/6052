#override or implement with a dict { byte : lambda o -> Op(o) } passed to init
class Opcodes:
    def __init__(self, table):
        self.table = table

    def reset(self):
        return Start(self)

    def byopcode(self, code):
        return self.make(table[code])

    def make(self, func):
        return func(self)

class Op(object):
    def __init__(self, table):
        self.table = table

#override and implement execute to implement an operation for which all parameters have been read (if any are needed)
class Complete(Op):
    def __init__(self, table):
        Op.__init__(self, table)

    def run(self):
        self.execute()
        return self.table.reset()

#override and implement finisher to produce an executable Op once all parameters have been read
class Incomplete(Op):
    def __init__(self, table, length):
        Op.__init__(self, table)
        self.unread = length
        self.params = []
        self.reader = new Parameters(self)

    def finish(self):
        return self.table.make(self.finisher(self.params)).run()

    def add(self, param):
        self.params.append(param)
        self.unread = self.unread - 1

    def run(self)
        return self.reader if self.unread else self.finish()

#the main loop will call read on a Reader object, then on the returned value on the next iteration
class Reader(object):
    def read(self, byte):
        return self.consume(byte).run()

class Start(Reader):
    def __init__(self, table):
        self.table = table

    def consume(self, byte):
        return self.table.byopcode(byte)

class Parameters(Reader):
    def __init__(self, op):
        self.op = op

    def consume(self, byte):
        self.op.add(byte)
        return self.op
