using System.Collections.Generic;
using System.Linq;
using NUnitLite;

namespace Reflection.Differentiation
{
	class Program
	{
		static void Main(string[] args)
		{
			Algebra_should aS = new Algebra_should();
			aS.DifferentiateComplexExpression();
		}
	}
}