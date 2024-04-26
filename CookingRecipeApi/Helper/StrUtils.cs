using System.Text.RegularExpressions;

namespace CookingRecipeApi.Helper
{
    public class StrUtils
    {
        public static string[] SplitSpecialCharacters(string intput)
        {
            string pattern = @"[.,!?;'\s]+";
            return Regex.Split(intput, pattern).Where(s => s.Length > 0).ToArray();
        }
    }
}
