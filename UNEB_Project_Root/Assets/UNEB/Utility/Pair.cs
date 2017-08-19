
namespace UNEB.Utility
{
    /// <summary>
    /// Simple pair tuple.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class Pair<T1, T2>
    {
        public readonly T1 item1;
        public readonly T2 item2;

        public Pair(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }
    }
}