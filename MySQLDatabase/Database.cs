using log4net;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Reflection;

namespace MySQLDatabase
{
    public class DBUtil
    {
        //DB接続情報
        private static string? _ConnectionString;

        //トランザクション情報
        private static DbTransaction? _Transaction = null;

        public static int DBTimeout { get; set; } = 9999;

        //AutoCommitの設定 True:オートコミットする　False:オートコミットしない
        private readonly bool _AutoCommit;

        //接続情報
        private MySqlConnection connection = new();

        //ログ設定
        private readonly ILog log = LogManager.GetLogger(typeof(DBUtil).FullName);

        /// <summary>
        /// 接続情報を取得生成するコンストラクタ
        /// </summary>
        /// <param name="AutoCommitBool">自動コミットの可否(True：自動コミットあり,False：自動コミット無し)</param>
        /// <exception cref="Exception"></exception>
        public DBUtil(bool AutoCommitBool = true)
        {
            log.Info("=====DB ConnectionProcess Start=====");

            // 環境変数から接続に必要な情報を取得
            string? server = Environment.GetEnvironmentVariable("DB_SERVER");
            string? database = Environment.GetEnvironmentVariable("DB_NAME");
            string? username = Environment.GetEnvironmentVariable("DB_USERNAME");
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            bool SettingCheckFlg = true;
            List<string> ErrorList = [];
            if (server is null) { SettingCheckFlg = false; ErrorList.Add("DB_SERVER"); }
            if (database is null) { SettingCheckFlg = false; ErrorList.Add("DB_NAME"); }
            if (username is null) { SettingCheckFlg = false; ErrorList.Add("DB_USERNAME"); }
            if (password is null) { SettingCheckFlg = false; ErrorList.Add("DB_PASSWORD"); }
            string ErrorMessage = string.Join(",", ErrorList);
            if (SettingCheckFlg == false)
            {
                //環境変数のエラーチェック
                log.Error("$\"環境変数に必要な情報が見つかりませんでした。\\r\\n必要な情報>>{ErrorMessage}");
                throw new Exception($"環境変数に必要な情報が見つかりませんでした。\r\n必要な情報>>{ErrorMessage}");
            }
            // MySQLに接続するための接続文字列を構築
            _ConnectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
            _AutoCommit = AutoCommitBool;
            DBOpen();

            log.Info("=====DB ConnectionProcess End=====");
        }

        /// <summary>
        /// DB接続
        /// </summary>
        /// <exception cref="Exception"></exception>
        protected void DBOpen()
        {
            connection = new MySqlConnection(_ConnectionString);
            try
            {
                connection.Open();
                //接続成功のメッセージ表示
                log.Info("接続に成功しました");

                //AutoCommitを行わない場合は、トランザクション開始
                if (_AutoCommit == false)
                {
                    DBTransaction();
                }

            }
            catch (Exception e)
            {
                throw new Exception($"接続に失敗しました:{e}");
            }
        }

        /// <summary>
        /// トランザクション開始処理
        /// </summary>
        /// <param name="connection">接続情報</param>
        /// <exception cref="Exception"></exception>
        protected void DBTransaction()
        {
            if (_Transaction is not null)
            {
                string Message = "すでにトランザクションが開始されています。";
                log.Error(Message);
                throw new Exception($"{Message}");
            }
            try
            {
                _Transaction = connection.BeginTransaction();
            }
            catch (Exception e) { throw new Exception($"トランザクションを開始できませんでした:{e}"); }
        }

        /// <summary>
        /// コミット
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void DBCommit()
        {
            try
            {
                if (_Transaction is null)
                {
                    log.Warn("トランザクションが開始されていません。コミットの必要はありません");
                }
                else
                {
                    _Transaction.Commit();
                }
            }
            catch (Exception e) { throw new Exception($"コミットに失敗しました:{e}"); }
        }
        /// <summary>
        /// ROLLBACK
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void DBRollback()
        {
            try
            {
                if (_Transaction is null)
                {
                    log.Warn("トランザクションが開始されていません。ロールバックは行われません");
                }
                else { _Transaction.Commit(); }
            }
            catch (Exception e) { throw new Exception($"ロールバックに失敗しました:{e}"); }

        }

        /// <summary>
        /// コネクション解放
        /// </summary>
        protected void DBClose()
        {
            try
            {
                connection.Close();
                connection.Dispose();
                log.Info("*****DB Connection Cloase****");
            }
            catch (Exception e) { throw new Exception($"コネクション解放に失敗しました{e}"); }
        }

        /// <summary>
        /// SQLの実行
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<object> ExcecuteSqlQuery(string Query, Dictionary<string, string>? parameters = null, object? EntityObj = null)
        {
            MySqlCommand command = new(Query, connection);
            try
            {
                command.CommandTimeout = DBTimeout;
                log.Info($"SQL:{Query}");
                if (parameters is not null && parameters.Count > 0)
                {
                    log.Info("<<パラメーター>>");
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        log.Info(parameter.Key + ":" + parameter.Value);
                    }
                }

                using MySqlDataReader reader = command.ExecuteReader();
                List<string> columnNames = GetColumnNames(reader);
                List<Dictionary<string, string?>> ResultListDic = [];

                while (reader.Read())
                {
                    Dictionary<string, string?> ResultDic = [];
                    foreach (var columnName in columnNames)
                    {
                        if (!ResultDic.ContainsKey(columnName))
                        {
                            ResultDic.Add(columnName, reader[columnName].ToString());
                        }
                    }
                    ResultListDic.Add(ResultDic);
                }

                List<object> resutlData = SetModelData(ResultListDic, EntityObj);

                if (_AutoCommit == true)
                {
                    DBCommit();
                }

                return resutlData;
            }
            catch (Exception e)
            {
                log.Error(e);
                DBRollback();
                DBClose();
                throw new Exception($"SQLクエリの実行中にエラーが発生しました:{e}");
            }
        }

        protected static List<string> GetColumnNames(MySqlDataReader reader)
        {
            List<string> columnNames = [];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                columnNames.Add(columnName);
            }
            return columnNames;
        }

        protected List<object> SetModelData(List<Dictionary<string, string?>> DbDataDicList, object? ModelObject)
        {
            try
            {

                if (ModelObject is null)
                {
                    log.Warn("オブジェクトが存在しません");
                    throw new Exception("オブジェクトが存在しません");
                }

                List<object> ResultObjList = [];

                foreach (var DbDataDic in DbDataDicList)
                {

                    object? clonedModelObject = Activator.CreateInstance(ModelObject.GetType()) ?? throw new Exception("オブジェクトのクローンに失敗しました");

                    foreach (var entry in DbDataDic)
                    {
                        //Object ResultObj = TryCast(ResultObj, EntityObjct.GetType());

                        string propertyName = entry.Key;
                        string? value = entry.Value;

                        // プロパティ名に対応する PropertyInfo オブジェクトを取得
                        PropertyInfo? propertyInfo = clonedModelObject.GetType().GetProperty(propertyName);

                        // プロパティが存在するかを確認
                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                propertyInfo.SetValue(clonedModelObject, null);
                            }
                            else
                            {

                                // プロパティの型がNullable<DateTime>であるかをチェック
                                if (propertyInfo.PropertyType == typeof(DateTime?))
                                {
                                    // 値がnullまたは空文字列の場合、プロパティにnullを設定
                                    // 値がnullまたは空文字列でない場合、値をDateTime型に変換してプロパティに設定
                                    object convertedValue = Convert.ChangeType(value, typeof(DateTime));
                                    propertyInfo.SetValue(clonedModelObject, convertedValue);
                                }
                                else
                                {
                                    if (propertyInfo.PropertyType == typeof(int?))
                                    {
                                        object convertedValue = Convert.ChangeType(value, typeof(int));
                                        propertyInfo.SetValue(clonedModelObject, convertedValue);
                                    }
                                    else
                                    {
                                        // プロパティがNullable<DateTime>型でない場合、通常の変換を行う
                                        object convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                                        propertyInfo.SetValue(clonedModelObject, convertedValue);

                                    }
                                }
                            }

                        }
                        else
                        {
                            throw new ArgumentException($"プロパティ '{propertyName}' が存在しないか、読み取り専用です。");
                        }

                    }
                    ResultObjList.Add(clonedModelObject);
                }
                return ResultObjList;

            }
            catch (Exception)
            {

                throw new Exception("プロパティの設定で失敗しました");
            }
        }
    }
}
