public class UserContext : IUserContext
{
    private string? _currentUsername;

    public string? CurrentUsername => _currentUsername;

    public void SetCurrentUser(string username)
    {
        _currentUsername = username;
    }

    public void Logout()
    {
        _currentUsername = null;
    }
}
