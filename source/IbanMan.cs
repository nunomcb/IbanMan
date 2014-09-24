using System;
using System.Threading;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Iban {
	public class IbanMan {
		public int LockType { get; set; }
		public int Nthreads { get; set; }
		public int CheckVal { get; set; }
		public int Max { get; set; }
		public int Min { get; set; }

		// CONSTRUCTORS
		public IbanMan (int lockType, int min, int max, int checkVal, int nthreads) {
			this.LockType = lockType;
			this.Nthreads = nthreads;
			this.Min = min;
			this.Max = max;
			this.CheckVal = checkVal;
		}

		// METHODS
		private Lock getLock() {
			switch ( this.LockType ) {
				case 0:
					return new TaSLock();
				case 1:
					return new TTaSLock();
				case 2:
					return new SpLock();
			}

			return null;
		}

		private bool isValid(int n) {
			int sum = 0,
			    i = 1;

			do {
				sum += (n % 10) * i;
				i++;
			} while ( (n /= 10) != 0 );

			return (sum % this.CheckVal == 0);
		}
			
		private void _count(ref int counter, Lock counterLock, ref int sum, Lock sumLock) {
			const int chunkSize = 20;
			int val;
			int top;
			int localsum = 0;

			for (;;) {
				counterLock.Lock();
				val = counter;
				counter += chunkSize;
				counterLock.Unlock();

				top = val + chunkSize;

				for ( ; val < top; val++ ) {
					if ( val >= this.Max ) {
						break;
					}

					if ( this.isValid( val ) ) {
						localsum++;
					}
				}

				if ( val >= this.Max ) {
					break;
				}
			}

			sumLock.Lock();
			sum += localsum;
			sumLock.Unlock();

			return;
		}

		public int Count() {
			Lock counterLock = this.getLock();
			Lock sumLock = this.getLock();
			int counter = this.Min;
			int sum = 0;

			Thread[] threads = new Thread[this.Nthreads];

			for ( int i = 0; i < this.Nthreads; i++ ) {
				threads[i] = new Thread( () => this._count( ref counter, counterLock, ref sum, sumLock ) );
				threads[i].Start();
			}

			for ( int i = 0; i < this.Nthreads; i++ ) {
				threads[i].Join();
			}

			return sum;
		}

		private void _list(ref int counter, Lock counterLock, Collection<int> list, Lock listLock) {
			const int chunkSize = 20;
			int val;
			int top;
			Collection<int> tempList = new Collection<int>();

			for (;;) {
				counterLock.Lock();
				val = counter;
				counter += chunkSize;
				counterLock.Unlock();

				top = val + chunkSize;

				for ( ; val < top; val++ ) {
					if ( val >= this.Max ) {
						break;
					}

					if ( this.isValid( val ) ) {
						tempList.Add( val );
					}
				}

				if ( tempList.Count > 0 ) {
					listLock.Lock();
					foreach ( int n in tempList ) {
						list.Add( n );
					}
					listLock.Unlock();
					tempList.Clear();
				}

				if ( val >= this.Max ) {
					break;
				}
			}

			return;
		}

		public void List(Collection<int> list) {
			Lock counterLock = this.getLock();
			Lock listLock = this.getLock();
			int counter = this.Min;

			Thread[] threads = new Thread[this.Nthreads];

			for ( int i = 0; i < this.Nthreads; i++ ) {
				threads[i] = new Thread( () => this._list( ref counter, counterLock, list, listLock ) );
				threads[i].Start();
			}

			for ( int i = 0; i < this.Nthreads; i++ ) {
				threads[i].Join();
			}

			return;
		}

		private void _search(ref int counter, Lock counterLock, string hash, ref int result) {
			const int chunkSize = 10;
			int val;
			int top;
			string newHash;

			for (;;) {
				if ( result != -1 ) {
					return;
				}

				counterLock.Lock();
				val = counter;
				counter += chunkSize;
				counterLock.Unlock();

				top = val + chunkSize;

				for ( ; val < top; val++ ) {
					if ( result != -1 || val >= this.Max ) {
						return;
					}

					if ( this.isValid( val ) ) {
						SHA1 sha = SHA1.Create();

						byte[] hashArray = sha.ComputeHash( Encoding.ASCII.GetBytes( val.ToString() ) );

						newHash = "";

						foreach ( byte b in hashArray ) {
							newHash += b.ToString( "x2" );
						}

						if ( newHash == hash ) {
							result = val;

							return;
						}
					}
				}
			}
		}

		public int Search(string hash) {
			Lock counterLock = this.getLock();
			int counter = this.Min;
			int result = -1;

			Thread[] threads = new Thread[this.Nthreads];

			for ( int i = 0; i < this.Nthreads; i++ ) {
				threads[i] = new Thread( () => this._search( ref counter, counterLock, hash, ref result ) );
				threads[i].Start();
			}

			for ( int i = 0; i < this.Nthreads; i++ ) {
				threads[i].Join();
			}

			return result;
		}
	}
}

