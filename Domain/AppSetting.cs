namespace Domain;

public class AppSetting
{
    public Guid AppId { get; set; }

    public AppSetting()
    {
        AppId = Guid.NewGuid();
    }
}