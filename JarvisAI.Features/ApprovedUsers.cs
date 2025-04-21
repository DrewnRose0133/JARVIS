
// Static approved user store
public static class ApprovedUsers
{
    public static HashSet<string> Users = new HashSet<string> { "tony", "pepper", "james" };

    public static bool IsApproved(string user) => Users.Contains(user);
}
