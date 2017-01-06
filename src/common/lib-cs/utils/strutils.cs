using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpringCard.LibCs
{
	class StrUtils
	{	
		public static int CountTokens(string source, char searched)
		{
			int count = 0;
			foreach (char c in source) {
				if (c == searched) count++;
			}
			return count;
		}	
	}
		
}
