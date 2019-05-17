using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RipAndBurn.Rip {
    class MemoryWavStream : MemoryStream {

        public MemoryWavStream() {

        }
        public override int Read(byte[] buffer, int offset, int count) {
            return base.Read(buffer, offset, count);
        }
    }
}
