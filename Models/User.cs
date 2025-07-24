public class User {
    public string Username { get; set; }
    public string PasswordHash { get; set; }  // ← No almacenamos password plano
    public string Role { get; set; }          // "admin" o "user"
}
