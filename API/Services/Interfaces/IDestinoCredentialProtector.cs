namespace API.Services.Interfaces;

public interface IDestinoCredentialProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
