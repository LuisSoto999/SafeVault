using NUnit.Framework;
using System.Text.RegularExpressions;

public class TestInputValidation
{
    [Test]
    public static void TestForSQLInjection() {
        var maliciousInput = "'; DROP TABLE Users; --";
        var result = ValidateInput(maliciousInput);
        Assert.That(result, Is.False);
    }

    [Test]
    public static void TestForXSS() {
        var maliciousInput = "<script>alert('XSS')</script>";
        var result = ValidateInput(maliciousInput);
        Assert.That(result, Is.False);
    }

    private static bool ValidateInput(string input) {
        return Regex.IsMatch(input, @"<script>|[;'\-]{1,2}", RegexOptions.IgnoreCase);
    }
}