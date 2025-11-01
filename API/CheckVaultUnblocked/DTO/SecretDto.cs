namespace CheckVaultUnblocked.DTO;

public class SecretDto
{
    public InnerData data { get; set; }
}

public class InnerData
{
    public secret_data data { get; set; }
}

public class secret_data
{
    public string secret { get; set; }
}
