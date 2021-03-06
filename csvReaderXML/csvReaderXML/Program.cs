﻿using csvReaderXML.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace csvReaderXML
{
    class Program
    {


        static StreamWriter logs = new StreamWriter(@"logs.txt");
        static String DEFAULT_SOURCE_PATH = "data.csv";
        static String DEFAULT_TARGET_PATH = "result.xml";
        static String DEFAULT_FORMAT = "xml";
        static void Main(string[] args)
            {

            String sourcePath,
                   targetPath,
                   format;
            if (args.Length == 3)
            {

                sourcePath = SetAsPath(args[0], DEFAULT_SOURCE_PATH);
                targetPath = SetAsPath(args[1], DEFAULT_TARGET_PATH);
                try
                {
                    format = SetFormat(args[2]);
                }catch(ArgumentException e)
                {
                    format = "XML";
                    logs.Write("Wrong argument.");
                }
            }
            else
            {
                sourcePath = DEFAULT_SOURCE_PATH;
                targetPath = DEFAULT_TARGET_PATH;
                format = DEFAULT_FORMAT;
            }

            FileInfo f = null;

            try
            {
                f = new FileInfo(sourcePath);
            }
            catch (DirectoryNotFoundException e)
            {
                f = new FileInfo(DEFAULT_SOURCE_PATH);
                Console.WriteLine(e.Message);
                logs.WriteLine(e.Message);
            }

            if (f != null)
            {
                try
                {
                    StreamReader reader = null;
                    try
                    {
                        reader = new StreamReader(f.OpenRead());
                    }catch(IOException e)
                    {
                        reader = new StreamReader(new FileInfo(DEFAULT_SOURCE_PATH).OpenRead());
                    }
                    finally
                    {
                        logs.WriteLine("Data doesn't exists.");
                        
                    }
                    StudentsSet students = new StudentsSet();
                    Dictionary<String, List<Student>> map = new Dictionary<string, List<Student>>();
                    for (String line = reader.ReadLine(); line != null; line = reader.ReadLine())
                    {

                        Student stud = createStudent(line.Split(','));
                        if (stud != null)
                        {
                            bool isAdded = students.Add(stud);
                            if (!isAdded)
                                logs.WriteLine("Value not unique: " + stud);
                            else
                            {
                                try
                                {

                                    List<Student> value;
                                    map.Add(stud.studies.name, new List<Student>());
                                    map.TryGetValue(stud.studies.name, out value);
                                    value.Add(stud);
                                    Console.WriteLine(stud.studies);
                                }
                                catch (ArgumentException) {

                                    List<Student> value;
                                    map.TryGetValue(stud.studies.name, out value);
                                    value.Add(stud);

                                }
                                                                

                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid value.");
                        }
                    }
                    reader.Dispose();



                    FileStream writer = new FileStream(targetPath, FileMode.Create);
                
                    if (format.ToUpper().Equals("XML")){

                        String dateT = DateTime.Now.ToString("dd.MM.yyy");

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml("<uczelnia></uczelnia>");
                        XmlElement root = doc.DocumentElement;
                        root.SetAttribute("createdAt", dateT);
                        root.SetAttribute("author", "Piotr Adarczyn");
                        XmlNode studenciXML = doc.CreateElement("studenci");
                        foreach (Student s in students.GetData())
                        {

                            studenciXML.AppendChild(CreateStudentXml(s, doc));

                        }
                        XmlNode activeStudiesXML = doc.CreateElement("activeStudies");
                        foreach (KeyValuePair<string, List<Student>> entry in map)
                        {
                            XmlElement studiesNameXML = doc.CreateElement("studies");
                            studiesNameXML.SetAttribute("name", entry.Key);
                            studiesNameXML.SetAttribute("numberOfStudents", entry.Value.Count.ToString());
                            activeStudiesXML.AppendChild(studiesNameXML);
                        }

                        root.AppendChild(studenciXML);
                        root.AppendChild(activeStudiesXML);
                        doc.Save(writer);
                        doc.Save(Console.Out);

                    }
                    else
                    {
                        var list = new List<Student>();
                        foreach (Student stud in students.GetData())
                        {
                            list.Add(stud);

                        }
                        var jsonString = JsonSerializer.Serialize(list);
                        jsonString = JsonSerializer.Serialize(map);
                        Console.WriteLine(jsonString);

                    }
               
                   
                    
                    writer.Dispose();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();


                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    logs.WriteLine(e.Message);
                }


                
               

            }
            logs.Dispose();
        }

        public static XmlElement CreateStudentXml(Student stud,XmlDocument doc)
        {

            XmlElement result = doc.CreateElement("student");
            String index = "s" + stud.index;
            
            result.SetAttribute("indexNumber", index);
            
           
            XmlElement firstName = doc.CreateElement("fname");
            firstName.InnerText = stud.firstname;
            result.AppendChild(firstName);

            XmlElement surName = doc.CreateElement("lname");
            surName.InnerText = stud.surname;
            result.AppendChild(surName);

            XmlElement birb = doc.CreateElement("birthdate");
            birb.InnerText = stud.date.ToString("dd.MM.yyy");
            result.AppendChild(birb);

            XmlElement email = doc.CreateElement("email");
            email.InnerText = stud.email;
            result.AppendChild(email);


            XmlElement motherN = doc.CreateElement("mothersName");
            motherN.InnerText = stud.motherName;
            result.AppendChild(motherN);

            XmlElement fatherN = doc.CreateElement("fathersName");
            fatherN.InnerText = stud.fatherName;
            result.AppendChild(fatherN);

            XmlNode studies = doc.CreateElement("studies");
            XmlElement name = doc.CreateElement("name");
            name.InnerText = stud.studies.name ;
            
            XmlElement mode = doc.CreateElement("mode");
            mode.InnerText = stud.studies.mode;


            studies.AppendChild(name);
            studies.AppendChild(mode);
            result.AppendChild(studies);

            return result;

        }
        public static Student createStudent(string[] array)
        {
            if (array.Length != 9)
            {

                logs.Write("Invalid row: ");
                foreach (String val in array)
                {
                    logs.Write(val + ", ");
                }
                return null;
            }else{
                bool valid = true;
                foreach(String val in array)
                {
                    if (val.Equals(""))
                        valid = false;
                }
                if (valid)
                {
                    DateTime date;
                 
                    if (DateTime.TryParse(array[5], out date)){
                        try
                        {
                            return new Student(array[0], array[1], array[2], array[3], Int32.Parse(array[4]), date
                                , array[6], array[7], array[8]);
                        }catch(FormatException e)
                        {
                            Console.WriteLine(e.Message);
                            logs.WriteLine(e.Message);
                            return null;
                        }
                    }
                    else
                    {
                        logs.WriteLine("Wrong date format.");
                        return null;
                    }
                }
                else
                {
                    logs.WriteLine("Empty values in: " + string.Join(", ",array));
                    return null;
                }
                

            }

        }
        public static void TestPathChars(string str)
        {

            char[] invalidPathChars = Path.GetInvalidPathChars();
            foreach (char letter in invalidPathChars)
            {

                if(str.Contains(letter))
                    throw new ArgumentException("Path contains invalid char code: " + (int)letter + "." );

            }
       

        }
        public static String SetAsPath(string str,string def)
        {

            try
            {

                TestPathChars(str);
                return str;

            }catch(ArgumentException e)
            {
                logs.WriteLine(e.Message);
                Console.WriteLine(e.Message);
                return def;
            }

        }
        public static String SetFormat(string str)
        {

            if (str.ToUpper().Equals("JSON"))
              return str;
            if (str.ToUpper().Equals("XML"))
                return str;
            else
                throw new ArgumentException();


        }
       

    }

}
