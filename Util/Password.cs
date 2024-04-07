namespace common
{
    /// <summary>
    /// パスワードのハッシュ化共通処理
    /// </summary>
    public class PasswordHash
    {
        /// <summary>
        /// パスワードハッシュ化
        /// </summary>
        /// <param name="Password"></param>
        /// <returns>ハッシュ化されたパスワードを返却</returns>
        public string Hash(string Password)
        {
            return BCrypt.Net.BCrypt.HashPassword(Password);
        }
        /// <summary>
        /// ハッシュ比較
        /// </summary>
        /// <param name="input">入力された生のパスワード</param>
        /// <param name="comparison">DBのハッシュ化されたパスワード</param>
        /// <returns>ハッシュ化が一致していたらTrue 一致していなかったらFalse</returns>
        public bool Validate(String input, string comparison)
        {
            return BCrypt.Net.BCrypt.Verify(Hash(input), comparison);
        }
    }
}
