namespace Minimal.Data;

public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool MarketingConsent { get; set; }



    public User()
	{
		Id= string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        MarketingConsent = false;
	}

}
