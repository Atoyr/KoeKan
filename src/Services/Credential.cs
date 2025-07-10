using System.Text;
using System.Security.Cryptography;

namespace Medoz.KoeKan.Services;

/// <summary>
/// シークレット情報を保持するクラス
/// </summary>
public class Credential
{
    /// <summary>
    /// シークレット情報を暗号化します。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string EncryptString(string input)
    {
        byte[] encryptedData = ProtectedData.Protect(
            Encoding.Unicode.GetBytes(input),
            null,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// シークレット情報を復号化します。
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <returns></returns>
    public static string DecryptString(string encryptedData)
    {
        byte[] decryptedData = ProtectedData.Unprotect(
            Convert.FromBase64String(encryptedData),
            null,
            DataProtectionScope.CurrentUser);

        return Encoding.Unicode.GetString(decryptedData);
    }
}