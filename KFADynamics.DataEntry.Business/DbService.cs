using System.Data;
using MySqlConnector;

namespace KFA.ItemCodes.Classes;

public static class DbService
{
  public static string? user = null;
  public static string ConnectionString { get; set; } = string.Empty;

  public static DataSet GetMySqlDataSet(string sql, params MySqlParameter[] parameters)
  {
    using var con = new MySqlConnector.MySqlConnection(ConnectionString);
    if (con.State != ConnectionState.Open)
      con.Open();

    var dbCommand = con.CreateCommand();
    dbCommand.CommandText = sql;
    dbCommand.CommandType = CommandType.Text;
    dbCommand.CommandTimeout = 3000;
    if (parameters?.Any() ?? false)
      dbCommand.Parameters.AddRange(parameters);

    var ds = new DataSet();
    var adapter = new MySqlDataAdapter(dbCommand);
    adapter.Fill(ds);
    return ds;
  }

  public static object? GetMySqlScalar(string sql, params IDbDataParameter[] parameters)
  {
    using var con = new MySqlConnector.MySqlConnection(ConnectionString);
    if (con.State != ConnectionState.Open)
      con.Open();

    var dbCommand = con.CreateCommand();
    dbCommand.CommandText = sql;
    dbCommand.CommandType = CommandType.Text;
    if (parameters?.Any() ?? false)
      dbCommand.Parameters.AddRange(parameters);
    return dbCommand.ExecuteScalar();
  }

  public static int ExecuteMySqlNonQuery(string sql, params IDbDataParameter[] parameters)
  {
    using var con = new MySqlConnector.MySqlConnection(ConnectionString);
    if (con.State != ConnectionState.Open)
      con.Open();

    var dbCommand = con.CreateCommand();
    dbCommand.CommandText = sql;
    dbCommand.CommandType = CommandType.Text;
    if (parameters?.Any() ?? false)
      dbCommand.Parameters.AddRange(parameters);
    return dbCommand.ExecuteNonQuery();
  }

  private static string? _loginId = null;

  internal static bool Login(string? username, string password)
  {
    using var ds = GetMySqlDataSet(@"SELECT
	tbl_system_users.user_id,
	tbl_system_users.name_of_the_user,
	tbl_system_users.username,
	tbl_system_users.password_hash,
	tbl_system_users.password_salt,
	tbl_system_users.is_active
FROM
	tbl_system_users  -- WHERE
-- username = @username");
    var data = ds.Tables[0].AsEnumerable()
      .Select(row => new
      {
        Id = row["user_id"].ToString(),
        Name = row["name_of_the_user"].ToString(),
        Username = row["username"].ToString(),
        PasswordHash = (byte[]?)row["password_hash"],
        PasswordSalt = (byte[]?)row["password_salt"],
        IsActive = (bool?)row["is_active"]
      }).Where(m => m.Username?.ToLower() == username?.ToLower()).ToArray();

    var users = data.Where(c => VerifyPasswordHash(password, c.PasswordHash, c.PasswordSalt));

    var user = users.First();
    if (user.IsActive != true)
      throw new Exception("You have been deactivated. Please contact information system");

    try
    {
      var narration = $"Dynamics Data Entry System - Computer: {Environment.MachineName}(User: {Environment.UserName})";
      var userId = users.First().Id;
      var sql = $@"SET @prefix = (SELECT MAX(CAST(`code` AS UNSIGNED)) FROM (SELECT SUBSTR(login_id,6) `code` FROM tbl_user_logins WHERE login_id LIKE 'CODE-%') A);
-- SET @prefix = (SELECT MAX CAST( AS UNSIGNED);
SET @id =  CONCAT('CODE-',LPAD(IFNULL(@prefix, 0)+1, 2,'0'));
SET @date = now();
SET @narration = '{narration}';
SET @userId = '{userId}';
SET @timeStamp = {DateTime.Now:yyyyMMddHHmmss}000;

INSERT INTO `kfa_sub_systems`.`tbl_user_logins`(`login_id`, `device_id`, `from_date`, `narration`, `upto_date`, `user_id`, `UserLoginId`, `record_verification_id`, `record_comment_id`, `is_currently_enabled`, `date_added`, `date_updated`, `originator_id`) VALUES (@id, '01C4 57C5 B16F AC7B 1889 76E8 50C2 65BB', @date, @narration, @date, @userId , NULL, NULL, NULL, 1, @timeStamp, @timeStamp, 100000001);

SELECT @id;";

      _loginId = GetMySqlScalar(sql)?.ToString();
    }
    catch { }

    if (users.Any())
    {
      DbService.user = $"{users.First().Name}[ Login Id: {_loginId} ]";
      return true;
    }
    return false;
  }

  private static bool VerifyPasswordHash(string password, byte[]? passwordHash, byte[]? passwordSalt)
  {
    try
    {
      if (passwordHash != null && passwordSalt != null)
      {
        using var hmac = new global::System.Security.Cryptography.HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(global::System.Text.Encoding.UTF8.GetBytes(password));
        for (int i = 0; i < computedHash.Length; i++)
          if (computedHash[i] != passwordHash[i]) return false;

        return true;
      }
    }
    catch
    {
      throw new Exception("Error in retrieving your current password");
    }

    return false;
  }
}
