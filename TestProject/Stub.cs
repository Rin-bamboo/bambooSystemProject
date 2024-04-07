


using Database;

namespace TestProject
{
    public class Stub
    {

        public Stub() { }

        public static void Main()
        {


            DBUtil dbUtil = new();

            dbUtil.ExcecuteSqlQuery("SELECT * FROM TESTDATA;");



        }



    }
}
