namespace frontend.Services;

public class RoleState
{
    public string CurrentRole { get; private set; } = "Worker";

    public event Action? OnChange;

    public void SetRole(string role)
    {
        CurrentRole = role;
        OnChange?.Invoke(); 
    }
}
