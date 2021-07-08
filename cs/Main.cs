using System;
using System.Linq;
using System.Collections.Generic;

namespace _6052
{
    internal class Opcodes<T> where T : Opcodes<T>
    {
        public static implicit operator T(Opcodes<T> obj)
            => obj as T;

        private readonly IDictionary<byte, Func<T, Op<T>>> table;

        public Start<T> Reset()
            => new Start<T>(this);

        public Opcodes(IEnumerable<KeyValuePair<byte, Func<T, Op<T>>>> table)
            => this.table = table.ToDictionary(t => t.Key, t => t.Value);

        public Op<T> Make(Func<T, Op<T>> maker)
            => maker(this);

        public Op<T> ByOpcode(byte opcode)
            => this.Make(this.table[opcode]);
    }

    internal abstract class Op<T> where T : Opcodes<T>
    {
        protected T Table { get; private set; }

        public abstract Reader Run();

        public Op(T table)
            => this.Table = table;
    }

    internal abstract class Complete<T> : Op<T> where T : Opcodes<T>
    {
        protected Complete(T table) :
            base(table)
        { }

        protected abstract void Execute();

       public override Reader Run()
        {
           this.Execute();
            return this.Table.Reset();
        }
    }

    internal abstract class Incomplete<T> : Op<T> where T : Opcodes<T>
    {
        private readonly byte[] parameters;
        private uint unread;
        private readonly Reader reader;

        protected abstract Func<T, Complete<T>> Finisher(byte[] parameters);

        public Incomplete(T table, uint last)
            : base(table)
        {
            this.unread = ++last;
            this.parameters = new byte[this.unread];
            this.reader = new Parameters<T>(this);
        }

        private Reader Finish()
            => this.Table.Make(this.Finisher(parameters)).Run();

        public void Next(byte param) =>
            this.parameters[parameters.Count() - (--this.unread)] = param;

        public override Reader Run() =>
            this.unread > 0 ? this.reader : this.Finish();
    }

    public abstract class Reader
    {
        public abstract Reader Read(byte input);
    }

    internal abstract class Reader<T> : Reader where T : Opcodes<T>
    {
        protected abstract Op<T> Consume(byte input);

        public override Reader Read(byte input) =>
            this.Consume(input).Run();
    }

    internal class Start<T> : Reader<T> where T : Opcodes<T>
    {
        private readonly T table;

        public Start(T table)
            => this.table = table;

        protected override Op<T> Consume(byte input) =>
            table.ByOpcode(input);
    }

    internal class Parameters<T> : Reader<T> where T : Opcodes<T>
    {
        private readonly Incomplete<T> op;

        public Parameters(Incomplete<T> op) =>
            this.op = op;

        protected override Op<T> Consume(byte input)
        {
            this.op.Next(input);
            return this.op;
        }
    }
}
