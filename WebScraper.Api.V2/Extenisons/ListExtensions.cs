namespace WebScraper.Api.V2.Extenisons
{
    public static class ListExtensions
    {
        public static object? GetParameterValue(this List<KeyValuePair<string, object>> parameterCollection, string parameterName)
        {
            if (parameterCollection == null || parameterCollection.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return null;
            }

            KeyValuePair<string, object>? param = parameterCollection.Where(param => param.Key == parameterName).FirstOrDefault();
            if (param != null)
            {
                return param.Value.Value;
            }

            return null;
        }
    }
}
