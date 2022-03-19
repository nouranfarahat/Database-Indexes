using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
namespace first
{
    public static class Program
    {

        public class Person
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
            public string Country { get; set; }

        }
        public static List<Person> personList;

        public static void Add()
        {
            Console.WriteLine("Enter ID");
            string id = Console.ReadLine();
            Console.WriteLine("Enter Name");
            string name = Console.ReadLine();
            Console.WriteLine("Enter City");
            string city = Console.ReadLine();
            Console.WriteLine("Enter Country");
            string country = Console.ReadLine();

            if ((!File.Exists("People.xml")))
            {
                XmlWriter writer = XmlWriter.Create("People.xml");

                writer.WriteStartDocument();
                writer.WriteStartElement("Table");
                writer.WriteAttributeString("name", "People");

                writer.WriteStartElement("Person");

                writer.WriteStartElement("ID");
                writer.WriteString(id);
                writer.WriteEndElement();

                writer.WriteStartElement("Name");
                writer.WriteString(name);
                writer.WriteEndElement();

                writer.WriteStartElement("City");
                writer.WriteString(city);
                writer.WriteEndElement();


                writer.WriteStartElement("Country");
                writer.WriteString(country);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Close();
            }
            else
            {
                //Console.WriteLine("gvcf");
                XmlDocument doc = new XmlDocument();

                XmlElement person = doc.CreateElement("Person");

                XmlElement node = doc.CreateElement("ID");
                node.InnerText = id;
                person.AppendChild(node);

                node = doc.CreateElement("Name");
                node.InnerText = name;
                person.AppendChild(node);


                node = doc.CreateElement("City");
                node.InnerText = city;
                person.AppendChild(node);

                node = doc.CreateElement("Country");
                node.InnerText = country;
                person.AppendChild(node);

                doc.Load("People.xml");
                XmlElement root = doc.DocumentElement;
                root.AppendChild(person);
                doc.Save("People.xml");
            }
            Console.WriteLine("Successfully Added!!");
        }
        public static void SerializeObject(this List<Person> list, string fileName) /*This function serialize the List 
                                                                                     * of type "Person" into xml file.*/ 
        {
            var serializer = new XmlSerializer(typeof(List<Person>));
            using (var stream = File.OpenWrite(fileName))
            {
                serializer.Serialize(stream, list);
            }
        }
  
        public static void SortXML() //This function to sort data in xml based on the "ID" field (index on the "ID" field).
        {
            XElement root = XElement.Load("People.xml");
            var orderedtabs = root.Elements("Person").OrderBy(xtab => (int)xtab.Element("ID")).ToArray(); /*here we put the xml file 
                                                                                                           * in array and sort by ID.*/
            root.RemoveAll(); 
            foreach (XElement tab in orderedtabs) // iterate on every element in "ordertabs" and put it in root (which is now sorted).
                root.Add(tab);
            root.Save("People.xml"); 
        }
        public static int Search(string SearchedValue, string FileToSearch) /*"SearchedValue" is the value which we want to 
                                                                             * search for and "FileToSearch" is the file 
                                                                             * to search in.*/
        {
            int counter = 1;
            XmlDocument document = new XmlDocument();
            document.Load(FileToSearch);
            XmlNodeList node = document.GetElementsByTagName("Person");

            for (int i = 0; i < node.Count; i++)
            {
                XmlNodeList childs = node[i].ChildNodes;
                string Id = childs[0].Name;
                string idvalue = childs[0].InnerText;

                string Name = childs[1].Name;
                string namevalue = childs[1].InnerText;

                string City = childs[2].Name;
                string cityvalue = childs[2].InnerText;

                string Country = childs[3].Name;
                string countryvalue = childs[3].InnerText;

                if (idvalue == SearchedValue)
                {
                    Console.WriteLine(node[i].InnerXml);
                    break;
                }
                else if (cityvalue == SearchedValue)
                {
                    Console.WriteLine(node[i].InnerXml);
                    break;
                }
                else
                {
                    counter++;        //"counter" to count the numer of iterations to reach the right xml file to search in. 
                }
            }
            return counter;
        }

        public static void Split()   //This function split the xml file into subfiles.
        {
            XElement root = XElement.Load("People.xml");
            personList = (from e in XDocument.Load("People.xml").Root.Elements("Person")
                          select new Person
                          {
                              ID = (string)e.Element("ID"),
                              Name = (string)e.Element("Name"),
                              City = (string)e.Element("City"),
                              Country = (string)e.Element("Country")
                          }).ToList();
            int Size = personList.Count();
            var fileno = 0;
            List<List<Person>> page = new List<List<Person>>(); /* Create List of List to hold the elements 
                                                                 * that will be put in the sub xml files.*/
            int count = 0;
            List<Person> temp = new List<Person>();
            string pages = "Page";
            foreach (var member in personList)
            {
                if (count++ == 8)           /* each subfile will hold maximum 8 elements 
                                             * (splitting the original xml file 8 by 8).*/
                {
                    page.Add(temp);
                    temp = new List<Person>();
                    count = 1;
                }
                temp.Add(member);
            }
            page.Add(temp);
            foreach (var member in page)
            {
                fileno += 1;
                SerializeObject(member, String.Format(pages + "{0}.xml", fileno));  /* the second parameter holds 
                                                                                     * the name of the file.*/
            }
        }
        public static void IndexPage()   /*This function to create the indexes page which have 
                                          * the entery key of each xml block and the page of it.*/
        {
            long size = -1;
            if (File.Exists("Indexes.xml"))
            {
                FileStream fs = new FileStream("Indexes.xml", FileMode.Truncate);
                fs.Close();
                size = new FileInfo("Indexes.xml").Length;
            }
            
            XElement root = XElement.Load("People.xml");
            personList = (from e in XDocument.Load("People.xml").Root.Elements("Person")
                          select new Person
                          {
                              ID = (string)e.Element("ID"),
                              Name = (string)e.Element("Name"),
                              City = (string)e.Element("City"),
                              Country = (string)e.Element("Country")
                          }).ToList();
            int i = 0;
            int pageNo = 1;
            foreach (var temp in personList)
            {
                if (i % 8 == 0)  // the entery key will be always divisible by 8 as we split the original xml file 8 by 8.
                {   

                    if ((size==0)||(!File.Exists("Indexes.xml")))
                    {
                        XmlWriter writer = XmlWriter.Create("Indexes.xml");

                        writer.WriteStartDocument();
                        writer.WriteStartElement("Indexes");

                        writer.WriteStartElement("ID");
                        writer.WriteString(temp.ID);
                        writer.WriteEndElement();

                        writer.WriteStartElement("PageNo");
                        writer.WriteString(pageNo.ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement();

                        writer.WriteEndDocument();

                        writer.Close();
                        size=new FileInfo("Indexes.xml").Length;
                    }
                    else
                    {
                        XmlDocument doc = new XmlDocument();

                        XmlElement Index = doc.CreateElement("Indexes");
                        XmlElement node = doc.CreateElement("ID");
                        node.InnerText = temp.ID;
                        Index.AppendChild(node);

                        node = doc.CreateElement("PageNo");
                        node.InnerText = pageNo.ToString();
                        Index.AppendChild(node);

                        doc.Load("Indexes.xml");
                        XmlElement root1 = doc.DocumentElement;
                        root1.AppendChild(Index);
                        doc.Save("Indexes.xml");
                    }
                    pageNo++;
                }
                i++;
            }
        }
        public static int PageSearch(string IdValue)  //This function search in the index.
        {
            XmlDocument document = new XmlDocument();
            document.Load("Indexes.xml");
            XmlNodeList node = document.GetElementsByTagName("Indexes");
            string pages = "Page";
            int counter2 = 0;
            for (int i = 0; i < node.Count; i++)
            {
                XmlNodeList childs = node[i].ChildNodes;
                string Id = childs[0].Name;
                string idvalue = childs[0].InnerText;

                string page = childs[1].Name;
                string pagevalue = childs[1].InnerText;
                int IntID = Convert.ToInt32(IdValue);
                if ((IntID - 1) / 8 == Convert.ToInt32(idvalue) / 8)
                {
                    Console.WriteLine(String.Format(pages + "{0}.xml", (Convert.ToInt32(idvalue) / 8) + 1)); /*print the XML page number that 
                                                                                                              * hold the required ID value.*/
                    counter2 += Search(IdValue, String.Format(pages + "{0}.xml", (Convert.ToInt32(idvalue) / 8) + 1)); 
                    break;
                }
                else
                {   counter2++;      /*It counts the number of iterations to reach the right ID 
                                      * and added to the counter returned from "Search" function to 
                                      * calculate the total number of iterations to reach the right ID.*/
                }
            }
            return counter2;
        }
        static void Main(string[] args)
        {
            string ans = "yes";
            while (ans == "yes")
            {
                Console.WriteLine("To add press 1");
                Console.WriteLine("To sort press 2");
                Console.WriteLine("To split press 3");
                Console.WriteLine("To make Index press 4");
                Console.WriteLine("To search by Index press 5"); //Search in file with indexing (by ID).
                Console.WriteLine("To Normal search press 6");  //Search in file without indexing (by city).
                Console.WriteLine("To add, sort and split automticaly  press 7"); /*If you want to add new data and other 
                                                                                   * functions will be called automatically.*/
                int choice = Convert.ToInt32(Console.ReadLine());
                if (choice == 1)
                {
                    Add();
                }
                else if (choice == 2)
                {
                    SortXML();
                }
                else if (choice == 3)
                {
                    Split();
                }
                else if (choice == 4)
                {
                    IndexPage();
                }
                else if (choice == 5)
                {
                    Console.WriteLine("Enter ID:");
                    string Entered = Console.ReadLine();
                    Console.WriteLine("Number of iterations :" + PageSearch(Entered));
                }
                else if (choice == 6)
                {
                    Console.WriteLine("Enter City: \n");
                    string Entered = Console.ReadLine();
                    Console.WriteLine("Number of iterations :"+Search(Entered, "People.xml"));
                }
                else if (choice == 7)
                {
                    Add();
                    SortXML();
                    Split();
                    IndexPage();
                }
                Console.WriteLine("Do you want anything else (yes/no)?");
                ans=Console.ReadLine();
            }
        }

    }
}
