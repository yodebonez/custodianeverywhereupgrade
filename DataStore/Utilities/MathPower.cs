using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Utilities
{
    public class MathPower
    {
        public decimal MathPow(decimal x, decimal y)
        {
            return DecimalExp(y * LogN(x));
        }

        private decimal Exponentiate(decimal a, decimal b)
        {
            decimal total = a;
            for (int i = 1; i < b; i++) total = a * total;
            return total;
        }

        public int Factorial(int n)
        {
            int j = 1;
            for (int i = 1; i <= n; i++) { j = j * i; }
            return j;
        }

        // Adjust this to modify the precision
        private const int ITERATIONS = 27;
        // power series

        private decimal DecimalExp(decimal power)
        {
            int iteration = ITERATIONS;
            decimal result = 1;
            while (iteration > 0)
            {
                decimal fatorial = Factorial(iteration);
                result += Exponentiate(power, iteration) / fatorial;
                iteration--;
            }
            return result;
        }

        // natural logarithm series
        private decimal LogN(decimal number)
        {
            decimal aux = (number - 1);
            decimal result = 0;
            int iteration = ITERATIONS;
            while (iteration > 0)
            {
                result += Exponentiate(aux, iteration) / iteration;
                iteration--;
            }
            return result;
        }
    }
}
