using System;
using System.Text;

namespace MatrixCalculator
{
    public class MatrixException : Exception
    {
        public MatrixException(string message) : base(message) { }
    }

    public class SizeMismatchException : MatrixException
    {
        public SizeMismatchException(string message) : base(message) { }
    }

    public class SingularMatrixException : MatrixException
    {
        public SingularMatrixException(string message) : base(message) { }
    }

    public interface IMyCloneable
    {
        object Clone();
    }

    public class SquareMatrix : IMyCloneable, IComparable<SquareMatrix>
    {
        private double[,] _m;
        private int _n;
        private const double eps = 1e-9;

        public SquareMatrix(int n)
        {
            if (n <= 0) throw new MatrixException("Size must be greater than zero.");
            _n = n;
            _m = new double[n, n];
        }

        public SquareMatrix(int n, int lo, int hi) : this(n)
        {
            var rnd = new Random();
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++)
                    _m[i, j] = rnd.Next(lo, hi);
        }

        public int Size
        {
            get { return _n; }
        }

        public double this[int i, int j]
        {
            get { return _m[i, j]; }
            set { _m[i, j] = value; }
        }

        public static SquareMatrix operator +(SquareMatrix a, SquareMatrix b)
        {
            if (a._n != b._n) throw new SizeMismatchException("Matrices must have the same size.");
            var c = new SquareMatrix(a._n);
            for (var i = 0; i < a._n; i++)
                for (var j = 0; j < a._n; j++)
                    c[i, j] = a[i, j] + b[i, j];
            return c;
        }

        public static SquareMatrix operator *(SquareMatrix a, SquareMatrix b)
        {
            if (a._n != b._n) throw new SizeMismatchException("Matrices must have the same size.");
            var c = new SquareMatrix(a._n);
            for (var i = 0; i < a._n; i++)
                for (var j = 0; j < a._n; j++)
                {
                    double s = 0;
                    for (var k = 0; k < a._n; k++)
                        s += a[i, k] * b[k, j];
                    c[i, j] = s;
                }
            return c;
        }

        public static bool operator >(SquareMatrix a, SquareMatrix b)
        {
            return a.Det() > b.Det();
        }

        public static bool operator <(SquareMatrix a, SquareMatrix b)
        {
            return a.Det() < b.Det();
        }

        public static bool operator >=(SquareMatrix a, SquareMatrix b)
        {
            return a.Det() >= b.Det();
        }

        public static bool operator <=(SquareMatrix a, SquareMatrix b)
        {
            return a.Det() <= b.Det();
        }

        public static bool operator ==(SquareMatrix a, SquareMatrix b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a._n != b._n) return false;
            for (var i = 0; i < a._n; i++)
                for (var j = 0; j < a._n; j++)
                    if (Math.Abs(a[i, j] - b[i, j]) > eps) return false;
            return true;
        }

        public static bool operator !=(SquareMatrix a, SquareMatrix b)
        {
            return !(a == b);
        }

        public static bool operator true(SquareMatrix m)
        {
            return Math.Abs(m.Det()) > eps;
        }

        public static bool operator false(SquareMatrix m)
        {
            return Math.Abs(m.Det()) <= eps;
        }

        public static explicit operator double(SquareMatrix m)
        {
            return m.Det();
        }

        public double GetDeterminant()
        {
            return Det();
        }

        private double Det()
        {
            return DetRec(_m, _n);
        }

        private double DetRec(double[,] a, int n)
        {
            if (n == 1) return a[0, 0];
            double det = 0;
            for (var c = 0; c < n; c++)
            {
                var sign = (c % 2 == 0) ? 1 : -1;
                var minor = Minor(a, 0, c, n);
                det += sign * a[0, c] * DetRec(minor, n - 1);
            }
            return det;
        }

        private double[,] Minor(double[,] a, int r, int c, int n)
        {
            var m = new double[n - 1, n - 1];
            for (int i = 0, mi = 0; i < n; i++)
            {
                if (i == r) continue;
                for (int j = 0, mj = 0; j < n; j++)
                {
                    if (j == c) continue;
                    m[mi, mj] = a[i, j];
                    mj++;
                }
                mi++;
            }
            return m;
        }

        public SquareMatrix GetInverse()
        {
            var det = Det();
            if (Math.Abs(det) < eps) throw new SingularMatrixException("Matrix is singular, inverse does not exist.");
            var inv = new SquareMatrix(_n);
            for (var i = 0; i < _n; i++)
                for (var j = 0; j < _n; j++)
                {
                    var minor = Minor(_m, i, j, _n);
                    var cof = ((i + j) % 2 == 0 ? 1 : -1) * DetRec(minor, _n - 1);
                    inv[j, i] = cof / det;
                }
            return inv;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < _n; i++)
            {
                for (var j = 0; j < _n; j++)
                    sb.Append($"{_m[i, j],8:F2} ");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public int CompareTo(SquareMatrix other)
        {
            if (other is null) return 1;
            return Det().CompareTo(other.Det());
        }

        public override bool Equals(object obj)
        {
            return obj is SquareMatrix m && this == m;
        }

        public override int GetHashCode()
        {
            return (_m?.GetHashCode() ?? 0) ^ _n.GetHashCode();
        }

        public object Clone()
        {
            var copy = new SquareMatrix(_n);
            for (var i = 0; i < _n; i++)
                for (var j = 0; j < _n; j++)
                    copy[i, j] = _m[i, j];
            return copy;
        }
    }

    class Program
    {
        static void Main()
        {
            try
            {
                Console.WriteLine("=== Matrix Calculator ===");
                Console.Write("Enter matrix size: ");
                var n = int.Parse(Console.ReadLine()!);

                var A = new SquareMatrix(n, 1, 10);
                var B = new SquareMatrix(n, 1, 5);

                Console.WriteLine("\nMatrix A:");
                Console.WriteLine(A);
                Console.WriteLine("Matrix B:");
                Console.WriteLine(B);

                var C = A + B;
                Console.WriteLine("Sum A + B:");
                Console.WriteLine(C);
                Console.WriteLine($"Determinant of A: {A.GetDeterminant():F2}");

                var cnt = 0;
                const int Max = 2;
                Console.WriteLine("Processing iterations (While):");
                while (cnt < Max)
                {
                    Console.WriteLine($"Iteration step: {cnt}");
                    cnt++;
                }

                cnt = 0;
                Console.WriteLine("\nProcessing iterations (Do-While):");
                do
                {
                    Console.WriteLine($"Step recorded: {cnt}");
                    cnt++;
                } while (cnt < Max);
            }
            catch (MatrixException ex)
            {
                Console.WriteLine($"Matrix error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"System error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}