using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSQLWrapper
{
    public static class SQLWrapper
    {
        static SqlDataAdapter dataAdapter = new SqlDataAdapter();
        
        static string database = @"Server=WIN-9PP8GDINMAP;Database=Users;User Id = admin; Password=1C,e[extn;";

        public static bool CheckAuthorizationData(string userName, string password)
        {
            
            DataTable dt = new DataTable("Users");
            using (SqlConnection sqlConnection = new SqlConnection(database))
            {
               
                string selectQueryUsers = @"SELECT Name,Password FROM Users WHERE Name = @Name AND Password = @Password";

                SqlCommand checkAuthorization = new SqlCommand(selectQueryUsers, sqlConnection);

                SqlParameter sqlParameterName = new SqlParameter("Name", userName);
                SqlParameter sqlParameterPassword = new SqlParameter("Password", password);

                checkAuthorization.Parameters.Add(sqlParameterName);
                checkAuthorization.Parameters.Add(sqlParameterPassword);

                dataAdapter.SelectCommand = checkAuthorization;
                dataAdapter.Fill(dt);
            }

            return dt.Rows.Count != 0;
        }

        public static DataTable GetUsers()
        {

            DataTable dt = new DataTable("Users");
            using (SqlConnection sqlConnection = new SqlConnection(database))
            {

                string selectQuery = "SELECT * FROM Users";

                dataAdapter.SelectCommand = new SqlCommand(selectQuery, sqlConnection);


                dataAdapter.Fill(dt);

            }

            return dt;
        }

        public static void AddUsers(DataTable dtUsers)
        {
            using (SqlConnection sqlConnection = new SqlConnection(database))
            {
                //insert
                string insertQueryUsers = @"INSERT INTO Users (Name,Password) VALUES (@Name,@Password); SET @ID = @@IDENTITY";

                SqlCommand insertUsers = new SqlCommand(insertQueryUsers, sqlConnection);
                insertUsers.Parameters.Add("@Name", SqlDbType.NVarChar, -1, "Name");
                insertUsers.Parameters.Add("@Password", SqlDbType.NVarChar, -1, "Password");
                insertUsers.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

                dataAdapter.InsertCommand = insertUsers;

                dataAdapter.Update(dtUsers);
            }
        }

        public static DataTable GetBases()
        {

            DataTable dt = new DataTable("Bases");
            using (SqlConnection sqlConnection = new SqlConnection(database))
            {

                string selectQuery = "SELECT * FROM Bases";

                dataAdapter.SelectCommand = new SqlCommand(selectQuery, sqlConnection);


                dataAdapter.Fill(dt);

            }

            return dt;
        }

        public static void UpdateUsers(DataTable dtUsers)
        {
            using (SqlConnection sqlConnection = new SqlConnection(database))
            {

                //insert
                string updateQueryUsers = @"UPDATE Users SET Name = @Name, Password = @Password WHERE Id = @Id";

                SqlCommand updateUsers = new SqlCommand(updateQueryUsers, sqlConnection);
                updateUsers.Parameters.Add("@Name", SqlDbType.NVarChar, -1, "Name");
                updateUsers.Parameters.Add("@Password", SqlDbType.NVarChar, -1, "Password");
                updateUsers.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

                dataAdapter.InsertCommand = updateUsers;

                dataAdapter.Update(dtUsers);
            }
        }
    }
}


