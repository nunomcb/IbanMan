using System;

namespace Iban {
	public interface Lock {
		void Lock();
		void Unlock();
	}
}