﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Celeste.Mod.Helpers {
    public sealed class UndisposableStream : Stream {

        // I'm overcomplicating this. -ade

        public readonly Stream Inner;

        public UndisposableStream(Stream inner) {
            Inner = inner;
        }

        public override bool CanRead {
            get {
                return Inner.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return Inner.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return Inner.CanWrite;
            }
        }

        public override long Length {
            get {
                return Inner.Length;
            }
        }

        public override long Position {
            get {
                return Inner.Position;
            }

            set {
                Inner.Position = value;
            }
        }

        public override void Flush() {
            Inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return Inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return Inner.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            Inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            Inner.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback callback, object state) {
            return Inner.BeginRead(array, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult) {
            return Inner.EndRead(asyncResult);
        }

        public override int ReadByte() {
            return Inner.ReadByte();
        }

        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback callback, object state) {
            return Inner.BeginWrite(array, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult) {
            Inner.EndWrite(asyncResult);
        }

        public override void WriteByte(byte value) {
            Inner.WriteByte(value);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return Inner.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return Inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken) {
            return Inner.FlushAsync(cancellationToken);
        }

        public override void Close() {
        }

        protected override void Dispose(bool disposing) {
        }

    }
}
