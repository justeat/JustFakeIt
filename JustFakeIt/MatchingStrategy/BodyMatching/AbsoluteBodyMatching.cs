namespace JustFakeIt
{
    public class AbsoluteBodyMatching : IBodyMatchingOption
    {
        public bool MatchBody(string expected, string actualBody)
        {
            return string.IsNullOrEmpty(expected) || actualBody.Equals(expected);
        }
    }
}