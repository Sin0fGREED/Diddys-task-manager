public interface IUserContext
{
    string? CurrentUsername { get; }
    void SetCurrentUser(string username);
    void Logout();
}
