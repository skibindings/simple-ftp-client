using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTP
{
    class Program
    {
        enum STATE
        {
            ENTER_IP,
            REGIME,
            ENTER_USER_INFO,
            WORKFLOW
        }


        static FTPClient client;
        static STATE state;

        static void Main(string[] args)
        {
            bool passive = true;
            string user = "";
            string pass = "";
            string ip = "";

            string dir = "/";

            state = STATE.ENTER_IP;

            //client.ListDirectory("/");

            while (true)
            {
                switch (state) {
                    case STATE.ENTER_IP:
                        while (true)
                        {
                            Console.WriteLine("Enter FTP server IP:");
                            String answer = Console.ReadLine();
                            IPAddress temp;
                            bool ValidateIP = IPAddress.TryParse(answer, out temp);
                            if (ValidateIP)
                            {
                                ip = answer;
                                state = STATE.REGIME;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid IP!");
                            }
                        }
                        break;
                    case STATE.REGIME:
                        while (true)
                        {
                            Console.WriteLine("Enable active regime of interaction with FTP server? (Y/N)");
                            String answer = Console.ReadLine();
                            if (answer.Equals("Y"))
                            {
                                passive = false;
                                state = STATE.ENTER_USER_INFO;
                                break;
                            }
                            else if (answer.Equals("N"))
                            {
                                passive = true;
                                state = STATE.ENTER_USER_INFO;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Invalid answer!");
                            }
                        }
                        break;
                    case STATE.ENTER_USER_INFO:
                        while (true)
                        {
                            Console.WriteLine("Enter username or blank for anonymous:");
                            String answer = Console.ReadLine();
                            if (answer.Length == 0)
                            {
                                user = "anonymous";
                                pass = "";
                                Console.WriteLine("Connecting FTP server as anonymous...");

                                client = new FTPClient(ip, user, pass, passive);
                                int resp_state = client.ListDirectory(dir);
                                if(resp_state == 0)
                                {
                                    Console.WriteLine("Timeout expired!");
                                    state = STATE.ENTER_IP;
                                }
                                else if(resp_state == 530)
                                {
                                    Console.WriteLine("Incorrect login-pass pair!");
                                }
                                else if(resp_state == 1)
                                {
                                    state = STATE.WORKFLOW;
                                }
                                break;
                            }
                            else
                            {
                                user = answer;
                                Console.WriteLine("Enter password:");
                                String answer_pass = Console.ReadLine();
                                pass = answer_pass;
                                Console.WriteLine("Connecting FTP server as "+ user +"...");

                                client = new FTPClient(ip, user, pass, passive);
                                int resp_state = client.ListDirectory(dir);

                                if (resp_state == 0)
                                {
                                    Console.WriteLine("Timeout expired!");
                                    state = STATE.ENTER_IP;
                                }
                                else if (resp_state == 530)
                                {
                                    Console.WriteLine("Incorrect login-pass pair!");
                                }
                                else if (resp_state == 1)
                                {
                                    state = STATE.WORKFLOW;
                                }
                                break;
                            }
                        }
                        break;
                    case STATE.WORKFLOW:
                        while (true)
                        {
                            String input = Console.ReadLine();

                            if (input.Length > 0)
                            {
                                String[] parts = input.Split(' ');

                                String command = parts[0];
                                String param = "";
                                if (parts.Length > 1)
                                {
                                    for(int i = 1; i < parts.Length; i++)
                                    {
                                        param += parts[i];
                                        if (i < parts.Length - 1) param += " ";
                                    }
                                }

                                if (command.Equals("cd"))
                                {
                                    String temp_dir = dir;
                                    if (param.Equals("../") || param.Equals("..")) // вернуться назад
                                    {
                                        if (!temp_dir.Equals("/"))
                                        {
                                            String[] dir_parts = temp_dir.Split('/');
                                            temp_dir = "/";
                                            for (int i = 1; i < dir_parts.Length - 2; i++)
                                            {
                                                temp_dir += dir_parts[i] + "/";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        temp_dir = temp_dir + param + "/";
                                    }

                                    int resp_state = client.ListDirectory(temp_dir);
                                    //Console.WriteLine(resp_state.ToString());
                                    if (resp_state == 1)
                                    {
                                        dir = temp_dir;
                                    }
                                    else if (resp_state == 550)
                                    {
                                        Console.WriteLine("Path "+ temp_dir+" does not exist!");
                                    }
                                }
                                else if (command.Equals("dir"))
                                {
                                    int resp_state = client.ListDirectory(dir);
                                }
                                else if (command.Equals("download"))
                                {
                                    int resp_state = client.DownloadFile(dir, param);
                                    //Console.WriteLine(resp_state.ToString());
                                    if(resp_state == 1)
                                    {
                                        Console.WriteLine("File " + param + " was successfully downloaded!");
                                    }
                                    else if (resp_state == 550)
                                    {
                                        Console.WriteLine("File " + param + " does not exist!");
                                    }
                                    Console.WriteLine();
                                }
                                else if (command.Equals("mkdir"))
                                {
                                    int resp_state = client.CreateDirectory(dir, param);
                                    if (resp_state == 1)
                                    {
                                        Console.WriteLine("Directory " + dir+param + " was successfully created");
                                    }
                                    else if (resp_state == 550)
                                    {
                                        Console.WriteLine("Access denied!");
                                    }
                                    Console.WriteLine();
                                }
                                else if (command.Equals("rmdir"))
                                {
                                    int resp_state = client.RemoveDirectory(dir+param);
                                    if (resp_state == 1)
                                    {
                                        Console.WriteLine("Directory " + dir+param + " was successfully removed");
                                    }
                                    else if (resp_state == 550)
                                    {
                                        Console.WriteLine("Access denied!");
                                    }
                                    Console.WriteLine();
                                }
                                else if (command.Equals("delete"))
                                {
                                    int resp_state = client.DeleteFile(dir + param);
                                    if (resp_state == 1)
                                    {
                                        Console.WriteLine("File " + dir + param + " was successfully deleted");
                                    }
                                    else if (resp_state == 550)
                                    {
                                        Console.WriteLine("Access denied!");
                                    }
                                    Console.WriteLine();
                                }
                                else if (command.Equals("upload"))
                                {
                                    int resp_state = client.UploadFile(dir,param);
                                    if (resp_state == 1)
                                    {
                                        Console.WriteLine("File " + param + " was successfully loaded to: " + dir);
                                    }
                                    else if (resp_state == 550)
                                    {
                                        Console.WriteLine("Access denied!");
                                    }
                                    Console.WriteLine();
                                }
                                else if (command.Equals("exit"))
                                {
                                    dir = "/";
                                    state = STATE.ENTER_IP;
                                }
                                break;
                            }
                        }
                        break;
                }
            }
        }
    }
}
 