namespace Common
{
    using System;

    public static class EnvironmentVariables
    {
        public static string GetValue(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
