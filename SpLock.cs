using System;
using System.Threading;

namespace Iban {
	public class SpLock : Lock {
		private SpinLock splock;

		public SpLock () {
			this.splock = new SpinLock();
		}

		public void Lock() {
			bool b = false;

			splock.Enter( ref b );

			return;
		}

		public void Unlock() {
			splock.Exit();

			return;
		}
	}
}

