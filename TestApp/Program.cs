﻿

// See https://aka.ms/new-console-template for more information
using MySQLDatabase;
using TestApp;

Console.WriteLine("Hello, World!");

//Envファイルテスト用　本番は削除
EnvFileReader.LoadEnvFile("C:\\Users\\ngsk_\\OneDrive\\ドキュメント\\開発ユーティリティ\\bambooSystem\\bambooSystemProject\\MySQLDatabase\\.env");


DBUtil dBUtil = new();

for (int i = 0; i < 1000; i++)
{

    string Queary = "SELECT * FROM TestTable";

    TestTableEntity testTable = new();

    List<object> resultList;

    resultList = dBUtil.ExcecuteSqlQuery(Queary, null, testTable);


    foreach (TestTableEntity result in resultList.Cast<TestTableEntity>())
    {
        testTable = result as TestTableEntity;
        Console.WriteLine($"ID:{testTable.ID} | InsertDate:{testTable.InsertDate} | UpdateDate:{testTable.UpdateDate} | UpdateUser:{testTable.UpdateUser} | Count:{testTable.Count}");
    }

}