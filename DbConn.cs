using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Dapper;

namespace FixMarkup
{
    class DbConn
    {
        public string dataBasePath;
        public List<Records> ReadDb()
        {
            var csAccess = new OleDbConnectionStringBuilder
            {
                DataSource = dataBasePath,
                Provider = @"Microsoft.Jet.OleDb.4.0",
            };
            var conAccess = new OleDbConnection(csAccess.ConnectionString);
            var records = new List<Records>();
            try
            {
                records = conAccess.Query<Records>(@"select id, f1, null, null from main where entity like 'Страница%' order by id").ToList();
            }
            catch (OleDbException ex)
            {
                if (ex.Message == "Отсутствует значение для одного или нескольких требуемых параметров.")
                {
                    MessageBox.Show(@"Кривая база");
                }
                else { MessageBox.Show(ex.Message, @"Ошибка"); }
            }
            return records;
        }

        public void deletparents(int ID)
        {
            var csAccess = new OleDbConnectionStringBuilder
            {
                DataSource = dataBasePath,
                Provider = @"Microsoft.Jet.OleDb.4.0",
            };
            var conAccess = new OleDbConnection(csAccess.ConnectionString);
            
            try
            {
                conAccess.Execute("delete from parents where id = " + ID);
            }
            catch (OleDbException ex)
            {
                 MessageBox.Show(ex.Message, @"Ошибка");
            }
            
        }

        public List<Records> ReadMain()
        {
            var csAccess = new OleDbConnectionStringBuilder
            {
                DataSource = dataBasePath,
                Provider = @"Microsoft.Jet.OleDb.4.0",
            };
            var conAccess = new OleDbConnection(csAccess.ConnectionString);
            var records = new List<Records>();
            try
            {
                records = conAccess.Query<Records>(@"select id, f1, null, null from main order by id").ToList();
            }
            catch (OleDbException ex)
            {
                if (ex.Message == "Отсутствует значение для одного или нескольких требуемых параметров.")
                {
                    MessageBox.Show(@"Кривая база");
                }
                else { MessageBox.Show(ex.Message, @"Ошибка"); }
            }
            return records;
        }
        public List<Parents> ReadParents()
        {
            var csAccess = new OleDbConnectionStringBuilder
            {
                DataSource = dataBasePath,
                Provider = @"Microsoft.Jet.OleDb.4.0",
            };
            var conAccess = new OleDbConnection(csAccess.ConnectionString);
            var parents = new List<Parents>();
            try
            {
                parents = conAccess.Query<Parents>(@"select id, pid from parents").ToList();
            }
            catch (OleDbException ex)
            {
                if (ex.Message == "Отсутствует значение для одного или нескольких требуемых параметров.")
                {
                    MessageBox.Show(@"Кривая база");
                }
                else { MessageBox.Show(ex.Message, @"Ошибка"); }
            }
            return parents;
        }
        public void Updatef1(List<Records> records,Form1 form)
        {
            var csAccess = new OleDbConnectionStringBuilder
            {
                DataSource = dataBasePath,
                Provider = @"Microsoft.Jet.OleDb.4.0",
            };
            var accessConn = new OleDbConnection(csAccess.ConnectionString);
            try
            {
                accessConn.Execute("Alter Table main add oldf1 Memo");
            }   
            catch (Exception)
            {
                accessConn.Execute("update main set oldf1 = null");
                form.AddLog("Поле oldf1 уже есть. Старые данные затёрты",true);
            }
            form.AddLog("Обновление полей");
            var p = new DynamicParameters();
            foreach (var rec in records.FindAll(f=> f.F1_new != null))
            {
                p.Add("f1",rec.F1_new.ToString());
                p.Add("oldf1", rec.isFix?rec.f1.ToString():"");
                p.Add("id",rec.id);
                accessConn.Execute("Update main set f1 = ?, oldf1 = ? where id = ?  ", p);
            }
            form.AddLog("Всё загруженно");
        }
    }
}
