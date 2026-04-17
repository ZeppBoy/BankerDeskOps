using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Avalonia.Services;

public class SessionContext
{
    public UserDto? CurrentUser { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public void Clear()
    {
        CurrentUser = null;
        IsAnonymous = false;
    }
}
