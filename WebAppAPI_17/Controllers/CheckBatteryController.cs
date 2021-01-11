using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;

namespace WebAppAPI_17.Controllers
{
    public class CheckBatteryController : ApiController
    {
        public class incidentBattery
        {
            public string ID { get; set; }
            public string Number { get; set; }
            public string Section { get; set; }
            public string Notation { get; set; }
            public string Date1 { get; set; }
            public string Date2 { get; set; }

            public incidentBattery(string ID, string Number, string Section, string Notation, string Date1, string Date2)
            {
                this.ID = ID;
                this.Number = Number;
                this.Section = Section;
                this.Notation = Notation;
                this.Date1 = Date1;
                this.Date2 = Date2;
            }

            public string GetToString()
            {
                string str = Notation + " " + Number + " " + Section + " отсутствуют данные с " + Date1 + " по " + Date2;
                return (str);
            }
        }

        // GET: api/CheckBattery
        public IHttpActionResult Get()
        {
            var jsondata = "{\"object1\":{\"Период отсутствия данных\": \"Секция тепловоза\"";
            //co

            string _SQL1 = "", _SQL2 = "";
            string s = "";
            DateTime Date1 = DateTime.Now, Date2;
            int i = 0, j = 0, h = 0;

            List<incidentBattery> Rezult = new List<incidentBattery>();
            //запрос 1
            _SQL1 = "SELECT [A].[ID],[A].[Section],[B].[Number],[C].[Name],[D].[Notation] " +
                "FROM[diag_lcm].[Lcm].[Sections][A] " +
                "join[diag_lcm].[Lcm].[Locomotives][B] on[A].[LocomotiveID] = [B].ID " +
                "join[diag_lcm].[Config].[Configurations][C] on[B].ConfigurationID = [C].ID " +
                "join[diag_lcm].[Config].[Models][D] on[C].ModelID = [D].ID " +
                "where[C].[Name] = 'TE25KM_HZM'";
            SqlConnection CoNn = new SqlConnection();
            CoNn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection2"].ConnectionString;
            CoNn.Open();
            SqlCommand cmd = CoNn.CreateCommand();
            cmd.CommandTimeout = 600; //увеличение время выполнения запроса сек
            cmd.CommandText = _SQL1;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) // если есть данные
            {
                while (reader.Read())
                {
                    j++;
                    //запрос 2
                    _SQL2 = "SELECT [SectionID], [MeasDateTime], [Mpsu_Mess] " +
                        "FROM [diag_lcm].[Res]._TE25KM_HZM " +
                        "WHERE [MeasDateTime]<CONVERT(DATETIME, '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 102) and [MeasDateTime]>CONVERT(DATETIME, '" + DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss") + "', 102) and [SectionID]=" + reader["ID"].ToString() + " ORDER BY MeasDateTime";
                    SqlConnection CoNn2 = new SqlConnection();
                    CoNn2.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection2"].ConnectionString;
                    CoNn2.Open();
                    SqlCommand cmd2 = CoNn2.CreateCommand();
                    cmd2.CommandTimeout = 600;
                    cmd2.CommandText = _SQL2;
                    SqlDataReader reader2 = cmd2.ExecuteReader();
                    i = 0;
                    if (reader2.HasRows) // если есть данные
                    {
                        while (reader2.Read())
                        {
                            i++;
                            if (i == 1)
                            {
                                DateTime.TryParse(Convert.ToString(reader2["MeasDateTime"]), out Date1);
                            }
                            else
                            {
                                DateTime.TryParse(Convert.ToString(reader2["MeasDateTime"]), out Date2);
                                h = (Date2.Subtract(Date1)).Hours;
                                if (h == 1)
                                {
                                    //jsondata += ", \"" + Convert.ToString(reader2["MeasDateTime"]) + "\":\"2ТЭ25КМ-" + Convert.ToString(reader["Number"]) + Convert.ToString(reader["Section"]) + "\"";
                                    DateTime.TryParse(Convert.ToString(reader2["MeasDateTime"]), out Date1);
                                }
                                if (h > 1)
                                {
                                    jsondata += ", \"с " + Convert.ToString(Date1) + " до " + Convert.ToString(Date2) + "\":\"2ТЭ25КМ-" + Convert.ToString(reader["Number"]) + Convert.ToString(reader["Section"]) + "\"";
                                    DateTime.TryParse(Convert.ToString(reader2["MeasDateTime"]), out Date1);
                                }
                            }
                            //jsondata += ", \"" + Convert.ToString(reader2["MeasDateTime"]) + "\":\"2ТЭ25КМ-" + Convert.ToString(reader["Number"]) + Convert.ToString(reader["Section"])+"\"";
                        }
                    }
                    CoNn2.Close();
                    //s = s + " _ " + Convert.ToString(reader["Number"]) + " " + Convert.ToString(reader["Section"]) + "\n";
                }
            }

            CoNn.Close();
            jsondata += "}}";

            //запись лог файла

            return Json(jsondata);
        }

        // GET: api/CheckBattery/5
        public string Get(int id)
        {
            return "value: " + id.ToString();
        }

        // POST: api/CheckBattery
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/CheckBattery/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/CheckBattery/5
        public void Delete(int id)
        {
        }
    }
}
