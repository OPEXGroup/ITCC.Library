namespace ITCC.HTTP.API.Utils
{
    public class Either<TFirst, TSecond>
        where TFirst : class
        where TSecond : class
    {
        public Either(TFirst first)
        {
            First = first;
        }

        public Either(TSecond second)
        {
            Second = second;
        }

        private Either() { }

        public bool HasFirst => First != null;
        public bool HasSecond => Second != null;

        public TFirst First { get; }
        public TSecond Second { get; }
    }
}
