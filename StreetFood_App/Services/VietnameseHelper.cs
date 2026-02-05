using System.Text;
using System.Text.RegularExpressions;

namespace StreetFood_App.Services;

public static class VietnameseHelper
{
    public static string ConvertToUnSign(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
        string temp = s.Normalize(NormalizationForm.FormD);
        return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').ToLower();
    }
}