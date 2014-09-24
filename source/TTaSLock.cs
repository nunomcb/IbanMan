using System;
using System.Threading;

namespace Iban {
	public class TTaSLock : Lock {
		private int lockVal;

		public TTaSLock () {
			this.lockVal = 0;
		}

		public void Lock() {
			do {
				while ( lockVal != 0 );
			} while ( Interlocked.Exchange( ref this.lockVal, 1 ) != 0 );

			return;
		}

		public void Unlock() {
			this.lockVal = 0;

			return;
		}
	}
}

