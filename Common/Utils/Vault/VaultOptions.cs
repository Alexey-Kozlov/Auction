namespace Common.Utils.Vault;

public class VaultOptions
{
    public string Address { get; set; }
    public string Role { get; set; }
    public string Secret { get; set; }
    public string SecretPathPg { get; set; }
    public string SecretPathRt { get; set; }
    public string SecretPathApi { get; set; }
    public string SecretPathElk { get; set; }
    public string PasswordPolicy { get; set; }
}