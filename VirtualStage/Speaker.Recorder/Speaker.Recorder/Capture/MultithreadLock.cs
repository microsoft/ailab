using System;

namespace Speaker.Recorder.Capture
{
    public sealed class MultithreadLock : IDisposable
    {
        private SharpDX.Direct3D11.Multithread multithread;

        public MultithreadLock(SharpDX.Direct3D11.Multithread multithread)
        {
            this.multithread = multithread;
            this.multithread?.Enter();
        }

        public void Dispose()
        {
            this.multithread?.Leave();
            this.multithread = null;
        }
    }
}
