using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace FixMarkup
{
    class Markup
    {
        public int X;
        public int Y;
        public int W;
        public int H;
        public Markup(int x, int y, int w, int h)
        {
            this.X = x;
            this.Y = y;
            this.W = w;
            this.H = h;
        }
    }

    class Person
    {
        public int id;
        public Markup markup;
    }

    class Records
    {
        public int id;
        public XDocument f1;
        public string path;
        public List<Person> persons;
        public XDocument F1_new;
        public bool isFix = false;
        
        public static List<Records> ParsePerson (List<Records> rec)
        {
            foreach (var r in rec.FindAll(f=> f.f1 != null))
            {
                r.path = r.f1.XPathSelectElement("i").Attribute("src").Value.ToString();

                if (r.f1.XPathSelectElements("//s").Count() != 0)
                {
                    var persons = new List<Person>();
                    foreach (var element in r.f1.XPathSelectElements("//s"))
                    {
                        var pers = new Person {id = Convert.ToInt32(element.Attribute("id").Value)};
                        string[] razm = element.Attribute("c").Value.Split(',');
                        pers.markup = new Markup(Convert.ToInt32(razm[0]), Convert.ToInt32(razm[1]), Convert.ToInt32(razm[2]), Convert.ToInt32(razm[3]));
                        persons.Add(pers);
                    }
                    r.persons = persons;
                }
            }
            return rec;
        }

        public static List<Records> ProvMarkup (List<Records> records, ref int count, bool pix, Form1 form)
        {
            var newRecords = new List<Records>();
            foreach (var rec in records)
            {
                bool flag = false;
                if (rec.persons != null && rec.persons.Count > 1)
                {
                    //rec.persons.Sort((f,s) => f.markup.Y.CompareTo(s.markup.Y));
                    if (rec.persons.Find(f => f.markup.H < 0) != null)
                    {
                        Console.Write(flag);
                    }

                    int ITER = 0;
                    while (rec.persons.Find(f=>f.markup.H<30) != null)
                    {
                        var i = 0;
                        Console.WriteLine("ID={0} Иттерация-{1}",rec.id, ITER);
                        foreach (var person in rec.persons)
                        {
                            if (i != rec.persons.Count - 1 && person.markup.H < 30 && rec.persons.Count > 2 &&
                                i != rec.persons.Count - 2)
                            {
                                int secondHY = rec.persons[i + 1].markup.Y + rec.persons[i + 1].markup.H;
                                person.markup.H = (secondHY - person.markup.Y) / 2;
                                rec.persons[i + 1].markup.Y = person.markup.Y + person.markup.H;
                                rec.persons[i + 1].markup.H = secondHY - rec.persons[i + 1].markup.Y;
                                flag = true;
                            }
                            if (i != rec.persons.Count - 1 && person.markup.H < 30 && rec.persons.Count == 2)
                            {
                                person.markup.H = 30;
                                rec.persons[i + 1].markup.Y = person.markup.Y + person.markup.H;
                                rec.persons[i + 1].markup.H = rec.persons[i + 1].markup.H - 30;
                                flag = true;
                            }
                            if (i != rec.persons.Count - 1 && person.markup.H < 30 && rec.persons.Count > 2 &&
                                i == rec.persons.Count - 2)
                            {
                                person.markup.H = 30;
                                rec.persons[i + 1].markup.Y = person.markup.Y + person.markup.H;
                                rec.persons[i + 1].markup.H = rec.persons[i + 1].markup.H - 30;
                                flag = true;
                            }

                            i++;
                        }
                        ITER++;
                        if (ITER == 1000)
                        {
                            form.AddLog("ID="+rec.id+"; не удалось сделать автоматическую корректировку. Нужно посмотреть вручную",true);
                            break;
                        }
                    }
          
                }
                if (flag)
                {
                    count++;
                    var newxdoc = new XDocument(
                        new XElement("i",
                            new XAttribute("src", rec.path)
                            ));
                    foreach (var p in rec.persons)
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
                    rec.F1_new = newxdoc;
                    rec.isFix = true;
                }
                // новое правило от Тумаркина. 1 пиксель между разметкой.
                if (pix)
                {
                    rec.persons?.ForEach(f => f.markup.H = f.markup.H - 1);
                    var newtumxdoc = new XDocument(
                        new XElement("i",
                            new XAttribute("src", rec.path)
                            ));

                    if (rec.persons != null)
                        foreach (var p in rec.persons)
                        {
                            var s = new XElement("s",
                                new XAttribute("c",
                                    p.markup.X.ToString() + "," +
                                    p.markup.Y.ToString() + "," +
                                    p.markup.W.ToString() + "," +
                                    p.markup.H.ToString()),
                                new XAttribute("id", p.id));
                            newtumxdoc.Element("i")?.Add(s);
                        }
                    rec.F1_new = newtumxdoc;
                }
            }

            return records;
        }
    }
}
