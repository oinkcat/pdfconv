using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfConverter.Simple
{
    internal class UnbufferedStreamReader : TextReader
    {
        public Stream BaseStream { get; }

        public bool EndOfStream => BaseStream.Position >= BaseStream.Length;

        public override void Close()
        {
            BaseStream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            BaseStream.Dispose();
        }

        public override int Read()
        {
            return BaseStream.ReadByte();
        }

        public override string ReadLine()
        {
            var buffer = new StringBuilder();

            int current;
            while ((current = Read()) != -1 && current != (int)'\n'
                                            && current != (int)'\r')
            {
                buffer.Append((char)current);
            }

            if(Read() != '\n' && !EndOfStream)
            {
                BaseStream.Seek(-1, SeekOrigin.Current);
            }

            return buffer.ToString();
        }

        public override Task<string> ReadLineAsync()
        {
            return Task.FromResult(this.ReadLine());
        }

        public override int Read(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override Task<int> ReadAsync(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int Peek()
        {
            return EndOfStream ? -1 : 0;
        }

        public override int Read(Span<char> buffer)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int ReadBlock(Span<char> buffer)
        {
            throw new NotImplementedException();
        }

        public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override string ReadToEnd()
        {
            throw new NotImplementedException();
        }

        public override Task<string> ReadToEndAsync()
        {
            throw new NotImplementedException();
        }

        public UnbufferedStreamReader(Stream stream)
        {
            this.BaseStream = stream;
        }
    }
}