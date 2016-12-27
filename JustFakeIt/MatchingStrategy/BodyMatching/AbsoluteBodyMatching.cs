namespace JustFakeIt
{
    public class AbsoluteBodyMatching : IBodyMatchingOption
    {
        public bool MatchBody(string expectedBody, string actualBody)
        {
            return string.IsNullOrEmpty(expectedBody) || (!string.IsNullOrEmpty(actualBody) && actualBody.Equals(expectedBody));
        }
    }
}