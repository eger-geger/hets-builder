namespace BoilerplateBuilders.ToString.Primitives
{
    public static class ToStringFunctions
    {
        public static string ToString<T>(T @object) => @object?.ToString() ?? string.Empty;
    }
}