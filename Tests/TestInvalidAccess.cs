using NUnit.Framework; 
using System.Net;

public class TestInvalidAccess {
    private SqlUserService db;

    [Test]
    public void TestLogin()
    {
        var username = "admin";
        var password = "wrong";

        var user = db.GetByUsername(username);
        if (user == null)
            return;
        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        Assert.That(isValid, Is.True);
    }

    [Test]
    public async Task TestUserAuthorization()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Cookie", "role=user");

        var response = await client.GetAsync("http://localhost:5110/admin_dashboard");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}