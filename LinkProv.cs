using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FixMarkup
{
    internal class LinkProv
    {
        public static List<Records> DelPers(Form1 form, string basename)
        {
            var bdconn = new DbConn { dataBasePath = basename };
            form.AddLog("Загрузка main");
            var main = bdconn.ReadMain();
            main = Records.ParsePerson(main);
            form.AddLog("Проверка..");
            foreach (var m in main.FindAll(f=>f.persons!=null && f.persons.Count != 0))
            {
                bool flag = false;
                foreach (var pers in m.persons.Where(pers => !main.Exists(f => f.id == pers.id)).ToList())
                {
                    form.AddLog("main.id = " + m.id.ToString() + " лишняя персоналия в f1 id = " + pers.id.ToString(), true);
                    m.persons.Remove(pers);
                    bdconn.deletparents(pers.id);
                    flag = true;
                }
                if (flag)
                {
                    var newxdoc = new XDocument(
                        new XElement("i",
                            new XAttribute("src", m.path)
                            ));
                    foreach (var p in m.persons)
                    {
                        var s = new XElement("s",
                            new XAttribute("c",
                                p.markup.X.ToString() + "," +
                                p.markup.Y.ToString() + "," +
                                p.markup.W.ToString() + "," +
                                p.markup.H.ToString()),
                            new XAttribute("id", p.id));
                        newxdoc.Element("i")?.Add(s);
                    }
                    m.F1_new = newxdoc;
                    m.isFix = true;
                }
            }

            return main;
        }
        public static void LinkProvFunc(Form1 form, string basename)
        {
            var bdconn = new DbConn { dataBasePath = basename};
            form.AddLog("Загрузка parents");
            var person = bdconn.ReadParents();
            form.AddLog("Загрузка main");
            var main = bdconn.ReadMain();
            main = Records.ParsePerson(main);
            form.AddLog("Проверка..");
            foreach (var m in main)
            {
                if (!person.Exists(f => f.id == m.id))
                {
                    form.AddLog("main.id = " + m.id.ToString() + " остутствует в parents.id",true);
                }
                if (m.persons!=null && m.persons.Count != 0)
                {
                    foreach (var pers in m.persons)
                    {
                        if (!main.Exists(f=> f.id == pers.id))
                        {
                            form.AddLog("main.id = " + m.id.ToString() + " лишняя персоналия в f1 id = "+ pers.id.ToString(), true);
                        }
                        if (pers.markup.H <= 0 || pers.markup.W <= 0)
                        {
                            form.AddLog("main.id = " + m.id.ToString() + " нулевые или отрицательные координаты", true);
                        }
                    }
                }
            }
            foreach (var p in person)
            {
                if (!main.Exists(f => f.id == p.id))
                {
                    form.AddLog("parents.id = " + p.id.ToString() + " остутствует в main.id", true);
                }
                if (!main.Exists(f => f.id == p.pid) && p.pid != 0)
                {
                    form.AddLog("parents.pid = " + p.pid.ToString() + " остутствует в main.id", true);
                }
            }
        }
    }

    class Parents
    {
        public int id;
        public int pid;
    }


}
