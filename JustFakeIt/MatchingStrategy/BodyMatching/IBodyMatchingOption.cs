namespace JustFakeIt
{
    public interface IBodyMatchingOption
    {
        bool MatchBody(string expected, string actualBody);
    }
}
