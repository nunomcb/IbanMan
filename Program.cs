using System;
using System.Threading;
using System.Collections.ObjectModel;

namespace Iban {
	class MainClass {

		public static void Main (string[] args) {

			string[] input = Console.ReadLine().Split( ' ' );
				
			IbanMan ibanMan = new IbanMan(
				Int32.Parse( input[0] ),
				Int32.Parse( input[1] ),
				Int32.Parse( input[2] ),
				Int32.Parse( input[3] ),
				Int32.Parse( input[4] )
			);

			switch ( Int32.Parse( input[5] ) ) {
			case 0:
				Console.WriteLine( ibanMan.Count() );

				break;
			case 1:
				ObservableCollection<int> ret = new ObservableCollection<int>();
				ret.CollectionChanged += listPrinter;
				ibanMan.List( ret );

				break;
			case 2:
				Console.WriteLine( ibanMan.Search( input[6] ) );

				break;
			}

			return;
		}

		private static void listPrinter(object sender, EventArgs e) {
			ObservableCollection<int> c = (ObservableCollection<int>)sender;
			int n = c.Count;

			Console.WriteLine( "{0} {1}", n, c[n - 1] );
		}
	}
}
