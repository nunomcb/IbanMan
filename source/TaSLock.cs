using System;
using System.Threading;

namespace Iban {
	public class TaSLock : Lock {
		private int lockVal;

		public TaSLock () {
			this.lockVal = 0;
		}

		public void Lock() {
			while ( Interlocked.Exchange( ref this.lockVal, 1 ) != 0 );

			return;
		}

		public void Unlock() {
			this.lockVal = 0;

			return;
		}
	}
}

